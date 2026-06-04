using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs.Common;

namespace MyWeb.Controllers.Api;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult NotImplementedEndpoint(string featureName)
    {
        return StatusCode(
            StatusCodes.Status501NotImplemented,
            ApiResponse.Failure(
                ErrorCode.NotImplemented,
                $"{featureName} 尚未實作，請先完成對應 Service、DTO、權限與交易規則。"));
    }
}
