using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectStore.Identity.Model;
using ProjectStore.Identity.Model.DBContext;
using System.Security.Cryptography;
using ProjectStore.Identity.RequestObject;
using ProjectStore.Identity.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

//TODO : Criar autenticação
namespace ProjectStore.Identity.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserContext _context;

        public UsersController(UserContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Busca o utilizador autenticado
        /// </summary>
        /// <returns>O utilizador autenticado</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<User>> GetUsers()
        {
            var user = await _context.Users.FindAsync(int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return user;
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <returns>204</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            string? currUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? parsedUser = int.Parse(currUser);
            User? user = await _context.Users.Where(q => q.UserId == parsedUser).FirstAsync();
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
