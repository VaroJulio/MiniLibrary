using Microsoft.AspNetCore.Mvc;
using MiniLibrary.Application.Common;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.API.Extensions;

/// <summary>
/// Extension methods for converting PagedResult to standardized API responses.
/// </summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// Converts a PagedResult to an OkObjectResult with standard PagedResponse structure.
    /// </summary>
    public static OkObjectResult ToPagedOk<T>(this ControllerBase controller, PagedResult<T> result)
    {
        return controller.Ok(PagedResponse<T>.FromPagedResult(result));
    }
}
