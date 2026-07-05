using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechWorldOrdersController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpPost]
        [Route("AddTechWorldOrders")]
        public IActionResult AddTechWorldOrders([FromBody] TechWorldOrders two)
        {
            if (two == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Order Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.AddTechWorldOrders(two, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }
    }
}
