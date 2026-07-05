using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly Dal _dal = new Dal();

        [HttpPost]
        [Route("AddNotification")]
        public IActionResult AddNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Notification Data" });
            }

            DBConnection dbc = new DBConnection();
            Response res = _dal.AddNotification(notification, dbc.GetConn());

            if (res.StatusCode == 200)
                return Ok(res);
            else
                return StatusCode(res.StatusCode, res);
        }

        [HttpGet]
        [Route("GetAllNotification")]

        public IActionResult GetAllNotifications()
        {
            try
            {
                DBConnection dbc = new DBConnection();
                var notificationList = _dal.GetAllNotifications(dbc.GetConn());

                if (notificationList != null && notificationList.Any())
                {
                    return Ok(notificationList);
                }
                else
                {
                    return NotFound(new Response { StatusCode = 404, StatusMessage = "Notifications not Found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { StatusCode = 500, StatusMessage = "Error: " + ex.Message });
            }
        }
    }
}
