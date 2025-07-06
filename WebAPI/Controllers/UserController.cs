using Main.Interfaces;
using Main.Requests.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;


        [HttpGet]
        public IActionResult Get([FromQuery] UserRequest request)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var response = _userService.GetUsers(request);
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var response = _userService.GetUserById(id);
            return HandleResponse(response);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _userService.DeleteUser(id);
            return HandleResponse(response);
        }
    }
}
