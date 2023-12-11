using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectStore.Identity.Model;
using ProjectStore.Identity.Model.DBContext;
using ProjectStore.Identity.RequestObject;
using ProjectStore.Identity.Utility;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectStore.Identity.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _config;
        public AuthController(UserContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user">POST Parameters including user information</param>
        /// <returns>201 Created or 400 Bad request</returns>
        // POST: api/Users/create
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=212375
        // Criar utilizador aqui
        [HttpPost("new")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> CreateUser(RegisterUserRequest user)
        {
            // Verificar se alguns dados já estão a ser usados
            //TODO : Talvez criar uma forma de interagir com o entity framework numa forma mais simpatica aqui?
            User result = await _context.Set<User>().FirstOrDefaultAsync(query => query.Email == user.Email);
            if (result != null)
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

        /// <summary>
        /// Logins as user
        /// </summary>
        /// <param name="user">POST parameters containing email and password</param>
        /// <returns>200 OK if successful, 401 Unauthorized on invalid parameters or wrong information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> LoginUser(LoginUserRequest user)
        {
            // Verificar se o user existe.
            //TODO : Talvez criar uma forma de interagir com o entity framework numa forma mais simpatica aqui?
            User result = await _context.Set<User>().FirstOrDefaultAsync(query => query.Email == user.Email);
            // Mesmo se o utilizador não existe, melhor tentar fazer a API trabalhar para atrasar o processamento do pedido, evitando certo tipos de ataque.
            if (result == null)
            {
                result = new User
                {
                    Email = "UserDoesNot@Exist.No",
                    FirstName = "UserDoesNotExist",
                    LastName = "UserDoesNotExist",
                    Password = "ouhjghuasiugbsihtbiwfdsaf.saghiksadhbfiashbdff"
                };
            }

            // Hash
            bool isCorrect = BCrypt.Net.BCrypt.Verify(user.Password, result.Password);

            if (!isCorrect)
            {
                string message = string.Format("User {0} does not exist or password is incorrect!", user.Email);
                return Problem(message, statusCode: (int)HttpStatusCode.Unauthorized);
            }


            string token = Jwt.CreateJWTToken(result.UserId, result.Email, _config);

            return Ok(new ReturnUserToken(result.UserId, token));
        }
    }
}
