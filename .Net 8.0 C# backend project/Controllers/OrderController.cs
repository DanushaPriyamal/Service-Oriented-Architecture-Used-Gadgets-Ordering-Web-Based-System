using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpPost]
        [Route("PlaceOrder")]
        public IActionResult PlaceOrder([FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null || orderRequest.Items == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Order Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.PlaceOrder(orderRequest, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }

        [HttpGet]
        [Route("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            DBConnection dbc = new DBConnection();
            try
            {
                var orders = _dal.GetAllOrders(dbc.GetConn());
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
