namespace EM.JsonRpc
{
    public class JsonError
    {
        public readonly int Code;
        public readonly string Message;

        public JsonError(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public static JsonError MethodNotFound = new JsonError(-32601, "Method Not Found");
        public static JsonError InvalidParams = new JsonError(-32602, "Invalid params");
        public static JsonError ParseError = new JsonError(-32700, "Parse error");
        public static JsonError InvalidRequest = new JsonError(-32600, "Invalid Request");
        public static JsonError InternalError = new JsonError(-32603, "Internal error");
    }

}
