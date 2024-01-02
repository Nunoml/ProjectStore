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
        /// Edits a user
        /// </summary>
        /// <param name="edit">Parameters including the current password and new user information.</param>
        /// <returns>A HTTP Status code representing if the action was successful or not</returns>
        //TODO
        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> PutUser(EditUserRequest edit)
        {
            string? currUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? parsedUser = int.Parse(currUser);
            User? user = await _context.Users.Where(q => q.UserId == parsedUser).FirstAsync();

            if (user == null)
            {
                return BadRequest();
            }

            // Implement Editing logic here

            if(BCrypt.Net.BCrypt.Verify(edit.PasswordValidation, user.Password))
            {
                if (edit.NewPassword != "")
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(edit.NewPassword);
                }

                _context.Entry(user).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Problem("Error", statusCode: (int)HttpStatusCode.InternalServerError);
                }
            }
            return NoContent();
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
