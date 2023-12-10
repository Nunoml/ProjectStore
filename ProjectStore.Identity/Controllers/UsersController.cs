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

//TODO : Criar autenticação
namespace ProjectStore.Identity.Controllers
{
    [Route("api/[controller]")]
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

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=212375
        // Criar utilizador aqui
        [HttpPost("create")]
        public async Task<ActionResult<User>> CreateUser(RegisterUserRequest user)
        {
            // Verificar se alguns dados já estão a ser usados
            //TODO : Talvez criar uma forma de interagir com o entity framework numa forma mais simpatica aqui?
            User result = await _context.Set<User>().FirstOrDefaultAsync(query => query.Email == user.Email);
            if(result != null)
            {
                string message = string.Format("Email {0} already in use!", user.Email);
                return Problem(message, statusCode: (int)HttpStatusCode.BadRequest);
            }

            // Hash
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password, 14);

            var newUser = user.AsUser();

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = newUser.UserId }, newUser);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> LoginUser(LoginUserRequest user)
        {
            // Verificar se o user existe.
            //TODO : Talvez criar uma forma de interagir com o entity framework numa forma mais simpatica aqui?
            User result = await _context.Set<User>().FirstOrDefaultAsync(query => query.Email == user.Email);
            // Mesmo se o utilizador não existe, melhor tentar fazer a API trabalhar para atrasar o processamento do pedido, evitando certo tipos de ataque.
            if (result == null)
            {
                result = new User();
                result.Password = "ouhjghuasiugbsihtbiwfdsaf.saghiksadhbfiashbdff";
            }

            // Hash
            bool isCorrect = BCrypt.Net.BCrypt.Verify(user.Password, result.Password);

            if (!isCorrect)
            {
                string message = string.Format("User {0} does not exist or password is incorrect!", user.Email);
                return Problem(message, statusCode: (int)HttpStatusCode.BadRequest);
            }

            
            string token = Jwt.CreateJWTToken(result.UserId, result.Email, _config);

            return Ok(new ReturnUserToken(result.UserId, token));
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
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
