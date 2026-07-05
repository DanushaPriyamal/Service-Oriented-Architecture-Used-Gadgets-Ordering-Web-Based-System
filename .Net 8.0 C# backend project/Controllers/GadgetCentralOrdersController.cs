using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GadgetCentralOrdersController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpPost]
        [Route("AddGadgetCentralOrders")]
        public IActionResult AddGadgetCentralOrders([FromBody] GadgetCentralOrders gco)
        {
            if (gco == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Order Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.AddGadgetCentralOrders(gco, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }
    }
}
