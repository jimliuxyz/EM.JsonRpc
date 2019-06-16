using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EM.JsonRpc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Example.JsonRpc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JsonRpcController : JsonRpcControllerBase
    {
        [HttpPost]
        [Produces("application/json")]
        [SwaggerRequestExample(typeof(JsonRpcRequest), typeof(JsonRpcRequestExample))]
        public async Task<ActionResult<Object>> Query(JsonRpcRequest reqest)
        {
            var respnese = await QueryControllerAsync(typeof(WebApiController), reqest);
            return respnese;
        }


        /// <summary>
        /// list error code
        /// </summary>
        [HttpGet("ListErrorCode")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ListErrorCode()
        {
            var res = new Dictionary<string, int>();
            foreach (ERRCODE code in Enum.GetValues(typeof(ERRCODE)))
            {
                res.Add(code.ToString(), (int)code);
            }

            return Ok(res);
        }
    }
}
