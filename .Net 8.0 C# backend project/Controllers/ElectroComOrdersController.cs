using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectroComOrdersController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpPost]
        [Route("AddElectroComOrders")]
        public IActionResult AddElectroComOrders([FromBody] ElectroComOrders eco)
        {
            if (eco == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Order Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.AddElectroComOrders(eco, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }
    }
}
