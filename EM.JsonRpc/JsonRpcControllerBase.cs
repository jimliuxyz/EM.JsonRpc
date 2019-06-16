
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Type = System.Type;

namespace EM.JsonRpc
{
    public abstract class JsonRpcControllerBase : ControllerBase
    {
        private static Type ActionResultTypeWithGeneric = typeof(IConvertToActionResult);
        private static Type ActionResultType = typeof(IActionResult);

        protected async Task<JsonRpcResponse> QueryControllerAsync(Type controllerType, JsonRpcRequest reqest)
        {
            var err = new JsonRpcError()
            {
                Jsonrpc = reqest.Jsonrpc,
                Id = reqest.Id,
            };

            // get controller
            ControllerBase controller;
            try
            {
                controller = (ControllerBase)ControllerContext.HttpContext.RequestServices.GetService(controllerType);
                controller.ControllerContext = this.ControllerContext;
            }
            catch (System.Exception e)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"can't get controller? try:\nservices.AddMvc().AddControllersAsServices();");
                Console.ForegroundColor = color;

                throw e;
            }

            //get method
            String reqMethodName = reqest.Method;
            if (reqest.Method.IndexOf("?") > 0)
                reqMethodName = reqest.Method.Substring(0, reqest.Method.IndexOf("?"));
            Console.WriteLine("method : {0}", reqMethodName);

            //get query string from method
            var reqQueryParams = GetUrlParams("method://" + reqest.Method);

            //get method info
            MethodInfo methodInfo;
            try
            {
                methodInfo = controller.GetType().GetMethod(reqMethodName);
                var isHttpMethod = methodInfo.IsDefined(typeof(HttpMethodAttribute));

                if (methodInfo == null || !isHttpMethod || !methodInfo.IsPublic)
                    throw new Exception();
            }
            catch (System.Exception)
            {
                err.Error = JsonError.MethodNotFound;
                return err;
            }

            // TODO : verify auth/role

            //mapping args
            var methodInfoArgs = methodInfo.GetParameters();
            var isBodyDtoTaken = false;
            var argList = new List<Object>();
            foreach (var item in methodInfoArgs)
            {
                var type = item.ParameterType;
                var name = item.Name;

                if (reqQueryParams.ContainsKey(name))
                {
                    var value = reqQueryParams[name];

                    try
                    {
                        if (type == typeof(string))
                            argList.Add(value);
                        else if (type == typeof(int))
                            argList.Add(int.Parse(value));
                        else if (type == typeof(long))
                            argList.Add(long.Parse(value));
                        else if (type == typeof(double))
                            argList.Add(double.Parse(value));
                        else if (type == typeof(decimal))
                            argList.Add(decimal.Parse(value));
                        else if (type == typeof(bool))
                            argList.Add(bool.Parse(value));
                        else if (type == typeof(DateTime))
                            argList.Add(DateTime.Parse(value));
                        else if (type.IsEnum)
                            argList.Add(Enum.Parse(type, value));
                        else
                        {
                            //unsupported type
                            err.Error = new JsonError(JsonError.ParseError.Code, $"unsupported type '{type}'");
                            return err;
                        }
                    }
                    catch (System.Exception)
                    {
                        err.Error = JsonError.ParseError;
                        return err;
                    }
                }
                else if (!isBodyDtoTaken)
                {
                    isBodyDtoTaken = true;
                    try
                    {
                        var str = JsonConvert.SerializeObject(reqest.Params);
                        object dto = JsonConvert.DeserializeObject(str, type);

                        // validate model
                        ValidationContext vc = new ValidationContext(dto);
                        ICollection<ValidationResult> results = new List<ValidationResult>();
                        bool isValid = Validator.TryValidateObject(dto, vc, results, true);
                        if (!isValid)
                        {
                            err.Error = new JsonError(JsonError.InvalidParams.Code, results.FirstOrDefault().ErrorMessage);
                            return err;
                        }

                        argList.Add(dto);
                    }
                    catch (System.Exception)
                    {
                        err.Error = new JsonError(JsonError.ParseError.Code, $"params missing. '{name}'");
                        return err;
                    }
                }
                else
                {
                    err.Error = JsonError.InvalidParams;
                    return err;
                }
            }

            //invoke
            object result = null;
            try
            {
                result = methodInfo.Invoke(controller, argList.Count == 0 ? null : argList.ToArray());
                if (result is Task)
                {
                    // result = result.GetType().GetProperty("Result").GetValue(result);
                    var task = ((Task)result);
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty.GetValue(task);
                }

                // for ActionResult<>
                if (ActionResultTypeWithGeneric.IsInstanceOfType(result))
                {
                    var actionResult = result.GetType().GetRuntimeProperty("Result").GetValue(result);
                    if (actionResult != null)
                        result = actionResult;
                    else
                        result = result.GetType().GetProperty("Value").GetValue(result);
                }

                // for ActionResult (ObjectResult,StatusCodeResult)
                if (ActionResultType.IsInstanceOfType(result))
                {
                    // handle status code
                    int statusCode = (int)HttpStatusCode.OK;
                    object objResult = null;
                    if (result is ObjectResult)
                    {
                        statusCode = ((ObjectResult)result).StatusCode.GetValueOrDefault();
                        objResult = ((ObjectResult)result).Value;
                    }
                    else if (result is StatusCodeResult)
                        statusCode = ((StatusCodeResult)result).StatusCode;

                    // transfer all not 'ok' status to 'InvalidRequest
                    if (statusCode != (int)HttpStatusCode.OK)
                    {
                        err.Error = new JsonError(JsonError.InvalidRequest.Code, $"{((HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), statusCode)).ToString()}({statusCode}) : {objResult}");
                        return err;
                    }
                    result = objResult;
                }

                return new JsonRpcResult()
                {
                    Jsonrpc = reqest.Jsonrpc,
                    Id = reqest.Id,
                    Result = result
                };
            }
            catch (Exception e)
            {
                var winEx = e.GetBaseException() as Win32Exception;
                if (winEx != null)
                {
                    err.Error = new JsonError(winEx.NativeErrorCode, winEx.Message);
                    return err;
                }
                Console.WriteLine(e);

                err.Error = JsonError.InternalError;
                return err;
            }
        }

        private static Dictionary<String, String> GetUrlParams(string pURL)
        {
            Uri uri = new Uri(pURL);
            var collection = HttpUtility.ParseQueryString(uri.Query, System.Text.Encoding.UTF8);

            var dict = new Dictionary<String, String>();
            foreach (var key in collection.AllKeys)
            {
                dict.Add(key, collection[key]);
            }
            return dict;
        }
    }
}
