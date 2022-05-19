using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdminService m_AdminService;

        public AdminController(AdminService adminService)
        {
            m_AdminService = adminService;
        }

        [HttpGet("GetUser")]
        public async Task<ActionResult<GetUsersModel>> GetUser(string username, string email)
        {
            Console.WriteLine($"GetUser: {username}, {email}");

            string? _username = null, _email = null;
            if(username != "null")
            {
                _username = username;
            }
            if (email != "null")
            {
                _email = email;
            }

            var users = await m_AdminService.GetUser(_username, _email);
            return users;
        }

        [HttpPost("SaveUserChanges")]
        public async Task<ActionResult> SaveUserChanges([FromBody] SaveUserModel model)
        {
            Console.WriteLine($"SaveUserChanges: {model.NewEmail}, {model.NewEmail}");
            bool result = await m_AdminService.ModifyUser(model);
            return Ok(new
            {
                Modified = result
            });
        }

        [HttpPost("SendPasswordReset")]
        public async Task<ActionResult> SendPasswordReset([FromBody] SendPasswordResetModel model)
        {
            Console.WriteLine($"SendPasswordResetModel: {model.Id}");
            bool result = await m_AdminService.SendPasswordReset(model);
            return Ok(new
            {
                Modified = result
            });
        }

        [HttpPost("GiveAdmin")]
        public async Task<ActionResult> GiveAdmin([FromBody] GiveAdminModel model)
        {
            Console.WriteLine($"GiveAdminModel: {model.Id}");
            bool result = await m_AdminService.GiveAdmin(model);
            return Ok(new
            {
                Modified = result
            });
        }
    }
}
