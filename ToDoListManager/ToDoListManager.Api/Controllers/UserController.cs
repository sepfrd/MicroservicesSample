using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListManager.Business.Contracts;
using ToDoListManager.Common.Dtos;

namespace ToDoListManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserBusiness _userBusiness;

    public UserController(IUserBusiness userBusiness)
    {
        _userBusiness = userBusiness;
    }

    [Authorize]
    [HttpGet]
    [Route("me")]
    public async Task<ActionResult<CustomResponse<UserDto?>>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var result = await _userBusiness.GetCurrentUserAsync(cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomResponse<UserDto?>>> CreateUserAsync([FromBody] SignupDto signupDto, CancellationToken cancellationToken)
    {
        var result = await _userBusiness.CreateUserAsync(signupDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }
}