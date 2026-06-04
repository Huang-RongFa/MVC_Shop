using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Accounts;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/roles")]
public class AdminRolesController : ApiControllerBase
{
    private readonly IRoleService _roleService;

    public AdminRolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost("{roleId:int}/permissions")]
    public ActionResult UpdatePermissions(int roleId)
    {
        return NotImplementedEndpoint("後台設定角色權限");
    }
}
