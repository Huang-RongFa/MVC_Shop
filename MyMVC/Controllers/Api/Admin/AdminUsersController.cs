using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Accounts;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/users")]
public class AdminUsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public ActionResult GetUsers()
    {
        return NotImplementedEndpoint("後台使用者列表");
    }

    [HttpPost("{userId:long}/roles")]
    public ActionResult AssignRoles(long userId)
    {
        return NotImplementedEndpoint("後台指派使用者角色");
    }
}
