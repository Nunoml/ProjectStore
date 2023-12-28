using Microsoft.AspNetCore.Mvc;
using ProjectStore.FileService.Model.DBContext;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectStore.FileService.Controllers
{
    [Route("api/Files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileContext _fileContext;
        private readonly DirectoryContext _dirContext;

        public FilesController(FileContext fileContext, DirectoryContext dirContext)
        {
            _fileContext = fileContext;
            _dirContext = dirContext;
        }


        // GET: api/<FilesController>
        // List all files associated with a specified user id in a path.
        // User id grabbed from authorization headers?
        // NOTE: No need to read any files unless asked to do so
        [HttpGet("{path}")]
        public IEnumerable<string> Get(string path)
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FilesController>/5
        // Lists some information about the file, maybe check the metadata of uploaded file? or store the metadata in the db
        [HttpGet("{path}/{name}/{read:bool}")]
        public string Get(string path, string name, bool read)
        {
            return "value";
        }

        // POST api/<FilesController>
        // Upload or create a new file, depending on whats specified in the request.
        [HttpPost("{path}")]
        public async Task<IActionResult> Post([FromBody] IFormFile file)
        {
            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            //Upload dos ficheiros
            //Ideia basica de upload de ficheiros
            using (var _fileStream = new FileStream("./files/0/" + file.FileName, FileMode.Create))
            {
                await file.CopyToAsync(_fileStream);
            }
            
            return Ok();
        }

        // PUT api/<FilesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FilesController>/5
        // Delete a specified file.
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
