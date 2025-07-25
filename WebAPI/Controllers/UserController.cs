﻿using Main.Interfaces;
using Main.Requests.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService _userService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] UserRequest request)
    {
        var response = _userService.GetUsers(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var response = _userService.GetUserById(id);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public IActionResult Edit([FromRoute] Guid id, [FromBody] EditUserRequest request)
    {
        var response = _userService.EditUser(id, request);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var response = _userService.DeleteUser(id);
        return HandleResponse(response);
    }
}
