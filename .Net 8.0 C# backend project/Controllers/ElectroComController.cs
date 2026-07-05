using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectroComController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpGet]
        [Route("QuatationRequest")]
        public IActionResult QuatationRequest()
        {
            DBConnection dbc = new DBConnection();
            try
            {
                var orders = _dal.QuatationRequest(dbc.GetConn());
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

        [HttpPost]
        [Route("AddFormulateQuatation")]
        public IActionResult AddElectroComFormulateQuatation([FromBody] FormulateQuatationElectroCom fqec)
        {
            if (fqec == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Quatation Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.AddElectroComFormulateQuatation(fqec, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }

        [HttpPost]
        [Route("ConfirmOrders")]

        public IActionResult ConfirmElectroComOrders([FromQuery] ElectroComOrders eco)
        {
            if (eco == null)
            {
                return BadRequest(new Response { StatusCode = 300, StatusMessage = "Invalid Data" });
            }

            DBConnection dbc = new DBConnection();
            Response response = _dal.ConfirmElectroComOrders(eco, dbc.GetConn());

            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            else if (response.StatusCode == 404)
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("GetElectroComOrders")]
        public IActionResult GetElectroOrderDetails()
        {
            DBConnection dbc = new DBConnection();
            try
            {
                var orders = _dal.GetElectroOrderDetails(dbc.GetConn());
                if (orders != null && orders.Any())
                {
                    return Ok(orders);
                }
                return NotFound(new Response { StatusCode = 404, StatusMessage = "No orders found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response { StatusCode = 500, StatusMessage = ex.Message });
            }
        }
    }
}
