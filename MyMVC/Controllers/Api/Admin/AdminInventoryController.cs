using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Inventory;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/inventory")]
public class AdminInventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventoryService;

    public AdminInventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("stocks")]
    public ActionResult GetStocks()
    {
        return NotImplementedEndpoint("後台庫存查詢");
    }

    [HttpPost("adjust")]
    public ActionResult AdjustStock()
    {
        return NotImplementedEndpoint("後台庫存調整");
    }
}
