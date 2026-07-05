using Microsoft.AspNetCore.Mvc;
using TheGadgetHub.Models;
using TheGadgetHub.Util;

namespace TheGadgetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GadgetController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private Dal _dal;

        public GadgetController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        [HttpPost]
        [Route("AddGadget")]
        public IActionResult AddGadget([FromForm] Gadget gadget, IFormFile? imageFile)
        {
            if (gadget == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            try
            {
                string imagePath = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Gadgets");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStram = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStram);
                    }
                    imagePath = Path.Combine("Images", "Gadgets", uniqueFileName).Replace("\\", "/");
                }

                gadget.GImagePath = imagePath;

                DBConnection dbc = new DBConnection();
                Response response = _dal.AddGadget(gadget, dbc.GetConn());

                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }
                else if (response.StatusCode == 400)
                {
                    return BadRequest(response);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { StatusCode = 500, StatusMessage = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllGadget")]

        public IActionResult GetAllGadget()
        {
            try
            {
                DBConnection dbc = new DBConnection();
                var gadgetList = _dal.GetAllGadget(dbc.GetConn());

                if (gadgetList != null && gadgetList.Any())
                {
                    return Ok(gadgetList);
                }
                else
                {
                    return NotFound(new Response { StatusCode = 404, StatusMessage = "No Gadget Found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { StatusCode = 500, StatusMessage = "Error: " + ex.Message });
            }
        }
    }
}
