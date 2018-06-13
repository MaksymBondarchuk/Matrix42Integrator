using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        // GET api/orders
        //[HttpGet("{access_token}")]
        [HttpGet]
		[Authorize(Roles = "Matrix42.MyWorkspace.Customer")]
		public ActionResult<IEnumerable<string>> Get()
        {
            return new[] { "Order1", "Order2" };
        }
    }
}
