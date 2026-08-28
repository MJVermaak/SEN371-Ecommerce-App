//Calling ASP.NET CORE MVC
using Microsoft.AspNetCore.Mvc;

namespace GrandmastersHub.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
