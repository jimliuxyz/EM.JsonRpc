using EM.JsonRpc;
using Swashbuckle.AspNetCore.Filters;
using static Example.JsonRpc.Controllers.WebApiController;

namespace Example.JsonRpc.Controllers
{
    public class JsonRpcRequestExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new JsonRpcRequest()
            {
                Jsonrpc = "2.0",
                Id = 99,
                Method = "EchoProfile",
                Params = new ProfileDto
                {
                    name = "JJ",
                    age = 10
                },
            };
        }
    }
}
