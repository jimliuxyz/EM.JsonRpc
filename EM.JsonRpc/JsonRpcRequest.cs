using System.ComponentModel.DataAnnotations;

namespace EM.JsonRpc
{
    public class JsonRpcRequest
    {
        [Required]
        public string Jsonrpc { get; set; }
        [Required]
        public int Id { get; set; }
        [Required]
        public string Method { get; set; }
        public dynamic Params { get; set; }
    }
}
