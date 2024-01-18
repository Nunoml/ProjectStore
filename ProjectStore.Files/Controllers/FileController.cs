using Microsoft.AspNetCore.Mvc;
using ProjectStore.Files.DataAccess;
using ProjectStore.Files.DataAccess.Model;
using ProjectStore.Files.DTO;
using System.IO;

namespace ProjectStore.Files.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private FileTable files = new FileTable("", "filedb");
        // GET: api/<ValuesController>
        [HttpGet("{dirId}")]
        public async Task<IActionResult> Get()
        {
            if (files.IsAvailable())
            {
                List<FileModel> val = files.GetAll("SELECT * FROM filedb WHERE UserId=0");
                return Ok(val);
            }
            return StatusCode(500, "Database server not available.");
        }

        // GET api/<ValuesController>/5
        // Download file/View metadata
        [HttpGet("{dirId}/{id}&{read}")]
        public async Task<IActionResult> Get(int dirId, int id, bool read)
        {
            
            if (files.IsAvailable())
            {
                
                FileModel val = files.Get($"SELECT TOP 1 * FROM filedb WHERE FileId={id} AND UserId=0 AND Inside={dirId}");
                string filePath = $"./files/0/{val.FileName}-{val.Inside}";

                if(read)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        return File(System.IO.File.OpenRead(filePath), "application/octet-stream", Path.GetFileName(filePath));
                    }
                    return StatusCode(500, "Could not find file in specified path, data might be corrupt.");
                }
                return StatusCode(500, "NOT IMPLEMENTED");
            }
            return StatusCode(500, "Database server not available.");
        }

        // POST api/<ValuesController>
        [HttpPost("{dirId}")]
        [RequestSizeLimit(999_000_000)]
        //Upload file
        public async Task<IActionResult> Post(int dirId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No File");

            // Nao vai ser criados diretorias novas, em vez disso, o ficheiro em si vai ter o ID da diretoria inserido.
            string filePath = $"./files/0/{file.FileName}-{dirId}";

            if(files.IsAvailable())
            {
                Directory.CreateDirectory($"./files/0/");
                using (var _fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync( _fileStream );
                }
                files.Insert(new FileModel { FileName = file.FileName, Inside = -1, UserId = 0 });
                return Ok();
            }
            return StatusCode(500, "Database server is not available.");
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{dirId}/{id}")]
        //Edit file metadata
        public async Task<IActionResult> Put(int id, EditFileMetadata value)
        {
            string filePath = $"./files/0/{value.OldFileName}";
            
            if(files.IsAvailable())
            {
                // TODO: Implementar directories
                if (System.IO.File.Exists(filePath) && value.NewFileName != "")
                {
                    // Rename usando File.Move();
                    System.IO.File.Move(filePath, $"./files/0/{value.NewFileName}");
                }
                // Requires validation
                files.Update(new FileModel { FileName = value.NewFileName, Inside=value.newFolder, Id = id });
                return Ok();
            }
            return StatusCode(500, "Database server is not available.");
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{dirId}/{id}")]
        //Delete file
        public async Task<IActionResult> Delete(int id)
        {
            if (files.IsAvailable())
            {
                FileModel val = files.Get($"SELECT TOP 1 * FROM filedb WHERE FileId={id} AND UserId=0");
                string userPath = $"./files/0/{val.FileName}-{val.Inside}";

                FileInfo f = new FileInfo(userPath);
                if (f.Exists)
                {
                    f.Delete();
                }

                files.Delete(val);
            }
            return StatusCode(500, "Database server is not available.");
        }
    }
}
