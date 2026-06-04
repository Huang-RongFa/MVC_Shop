using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Logs;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/audit")]
public class AdminAuditLogsController : ApiControllerBase
{
    private readonly IAuditService _auditService;

    public AdminAuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("admin-actions")]
    public ActionResult GetAdminActions()
    {
        return NotImplementedEndpoint("後台操作紀錄查詢");
    }
}
