using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectroComFormulationQuatationController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpGet]
        [Route("GetElectroComFormulationQuatation")]
        public IActionResult GetTechWorldFormulateQuatation()
        {
            DBConnection dbc = new DBConnection();
            try
            {
                var orders = _dal.GetElectroComFormulateQuatation(dbc.GetConn());
                if (orders != null && orders.Any())
                {
                    return Ok(orders);
                }
                return NotFound(new Response { StatusCode = 404, StatusMessage = "No Quatation found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response { StatusCode = 500, StatusMessage = ex.Message });
            }
        }
    }
}
