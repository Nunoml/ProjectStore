using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProjectStore.FileService.Model;
using ProjectStore.FileService.Model.DBContext;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectStore.FileService.Controllers
{
    [Route("api/Files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileContext _fileContext;

        public FilesController(FileContext fileContext)
        {
            _fileContext = fileContext;
        }


        // GET: api/<FilesController>
        // List all files associated with a specified user id in a path.
        // User id grabbed from authorization headers?
        // NOTE: No need to read any files unless asked to do so
        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path)
        {
            //TODO: Implementar directories
            List<FileEntity> files = await _fileContext.Files.Where(q => q.Path == path && q.UserId == 0).ToListAsync();
            if (files.Count == 0)
            {
                return NotFound();
            }

            //TODO RETORNAR LISTA AQUI
            return Ok();
        }

        // GET api/<FilesController>/5
        // Lists some information about the file, store the metadata in the db
        [HttpGet("{path}/{name}/{read:bool}")]
        public async Task<IActionResult> Get(string name, bool read, string path = "")
        {
            string userPath = "./files/0/" + path;
            if (name == null)
            {
                return BadRequest();
            }
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => ($"{q.Path}{q.FileName}") == ($"{userPath}{name}"));
            if (file == null)
            {
                return NotFound();
            }

            if (read)
            {
                // Transferir o ficheiro
                string filePath = userPath + file.FileName;

                if (System.IO.File.Exists(filePath))
                {
                    return File(System.IO.File.OpenRead(filePath), "application/octet-stream", Path.GetFileName(filePath));
                }

                return StatusCode(500, "Could not find file when trying to download it.");
            }
            else
            {
                // Retornar DTO com metadata
                return Ok();
            }
        }

        // POST api/<FilesController>
        // Upload or create a new file, depending on whats specified in the request.
        [HttpPost()]
        [RequestSizeLimit(999_000_000)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            string filePath = "./files/0/" + file.FileName;
            Directory.CreateDirectory("./files/0/");
            //Upload dos ficheiros
            //Ideia basica de upload de ficheiros
            using (var _fileStream = new FileStream("./files/0/" + file.FileName, FileMode.Create))
            {
                await file.CopyToAsync(_fileStream);
            }
            //Criar entrada na DB TODO
            FileEntity newFile = new FileEntity
            {
                FileName = file.FileName,
                Path = "root",
                UserId = 0
            };

            _fileContext.Add(newFile);
            await _fileContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{path}")]
        [RequestSizeLimit(999_000_000)]
        public async Task<IActionResult> CreateFileInPath(IFormFile file, string path)
        {
            // Decoding
            path = path.Replace("%2F", "/");
            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            string filePath = "./files/0/" + path + "/" + file.FileName;
            Directory.CreateDirectory("./files/0/" + path);
            //Upload dos ficheiros
            //Ideia basica de upload de ficheiros
            using (var _fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(_fileStream);
            }

            //Criar representação de uma pasta.
            string currDir = "";
            foreach (var dirName in path.Split("/"))
            {
                DirectoryEntity? dirCheck = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path == "root/" + currDir && q.DirName == dirName && q.UserId == 0);
                if(dirCheck == null)
                {
                    DirectoryEntity dir = new DirectoryEntity
                    {
                        DirName = dirName,
                        Path = "root/" + currDir,
                        UserId = 0
                    };
                    currDir += dirName + "/";
                    _fileContext.Add(dir);
                }
            };
            
            //Criar representação de um ficheiro
            //TODO: Verificar se é duplicado.
            FileEntity newFile = new FileEntity
            {
                FileName = file.FileName,
                Path = "root/" + path,
                UserId = 0
            };

            _fileContext.Add(newFile);
            await _fileContext.SaveChangesAsync();

            return Ok();
        }

        // DELETE api/<FilesController>/5
        // Delete a specified file or directory.
        [HttpDelete("{path}/{name}")]
        public async Task<IActionResult> Delete(string path, string name)
        {
            // Usado para eleminar o ficheiro do servidor
            string userPath = "./files/0/" + path;
            if (name == null)
            {
                return BadRequest();
            }
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => q.Path + q.FileName == path + name && q.UserId == 0);

            //Encontrar o ficheiro e apagar
            return Ok();
        }
    }
}
