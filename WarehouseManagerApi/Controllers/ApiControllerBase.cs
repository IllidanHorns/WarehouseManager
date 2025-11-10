using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Exceptions;

namespace WarehouseManagerApi.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                ModelValidationException validation => BadRequest(new
                {
                    message = validation.Message,
                    errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                }),
                ConflictException conflict => Conflict(new { message = conflict.Message }),
                InvalidCredentialsException invalid => Unauthorized(new { message = invalid.Message }),
                DomainException domain => BadRequest(new { message = domain.Message }),
                _ => StatusCode(500, new { message = ex.Message })
            };
        }
    }
}
