using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Example.JsonRpc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebApiController : ControllerBase
    {
        public class ProfileDto
        {
            [Required]
            public string name { get; set; }
            [Required]
            public int age { get; set; }
        }

        public class ProfileResultDto
        {
            public string desc { get; set; }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type
        {
            ok, bad
        }

        /// <summary>
        /// test 1
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("EchoProfile")]
        public async Task<ActionResult<ProfileDto>> EchoProfile(ProfileDto dto)
        {
            return dto;
        }

        /// <summary>
        /// test 2
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("TestResult/{type}")]
        public async Task<ActionResult> TestResult(Type type)
        {
            if (type == Type.ok)
                return Ok("ok");
            if (type == Type.bad)
                return BadRequest("bad");

            throw new Win32Exception(-99999, "unknown type");
        }

    }
}
