using System;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("now")]
        public ActionResult<DateTime> Now() => DateTime.Now;

        [HttpGet("auth"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<DateTime> Auth() => DateTime.Now;

        [HttpGet("auth-username"),
         Authorize(Roles = "TestRole", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> AuthUsername()
        {
            var test1 = User.IsInRole("ROLE_USER");
            var test2 = User.IsInRole("offline_access");
            var test3 = User.IsInRole("manage-account-links");
            return User.Identity.Name;
        }
    }
}