# EM.JsonRpc

Automatically map JsonRpc to existing web api.

# Install

```
dotnet add package EM.JsonRpc --version 0.0.1
```

# Usage

```cs
//Starup.cs

public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().AddControllersAsServices();
}

```

```cs
//add your jsonrpc controller

public class JsonRpcController : JsonRpcControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Object>> Query(JsonRpcRequest reqest)
    {
        //replace WebApiController to your web api controller
        return await QueryControllerAsync(typeof(WebApiController), reqest);
    }
}

```

# JsonRpc Request

```js
//request
//replace 'EchoProfile' to the method name of your web api controller
{
  "jsonrpc": "2.0",
  "id": 99,
  "method": "EchoProfile",
  "params": {
    "name": "JJ",
    "age": 10
  }
}
```

```js
//request with query string
{
  "jsonrpc": "2.0",
  "id": 99,
  "method": "TestResult?type=ok",
  "params": null
}
```

# Example

Run and try it by swagger.

```
https://localhost:6001/swagger/index.html
```