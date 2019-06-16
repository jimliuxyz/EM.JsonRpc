using System.ComponentModel.DataAnnotations;

namespace EM.JsonRpc
{
    public abstract class JsonRpcResponse
    {
        [Required]
        public string Jsonrpc { get; set; }
        [Required]
        public int Id { get; set; }
    }

    public class JsonRpcResult : JsonRpcResponse
    {
        public dynamic Result { get; set; }
    }

    public class JsonRpcError : JsonRpcResponse
    {
        public JsonError Error { get; set; }
    }
}
