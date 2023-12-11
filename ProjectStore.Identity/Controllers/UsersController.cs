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

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Edits a user
        /// </summary>
        /// <param name="editInfo">Parameters including the current password and new user information.</param>
        /// <returns>A HTTP Status code representing if the action was successful or not</returns>
        // PUT: api/Users/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> PutUser(EditUserRequest editInfo)
        {
            var currUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(currUser);

            if(user == null)
            {
                return BadRequest();
            }

            // Implement Editing logic here

            if(BCrypt.Net.BCrypt.Verify(editInfo.PasswordValidation, user.Password))
            {
                if (editInfo.NewPassword != "")
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(editInfo.NewPassword);
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
        /// <returns></returns>
        // DELETE: api/Users/
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var currUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(currUser);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
