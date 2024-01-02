using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProjectStore.FileService.Model;
using ProjectStore.FileService.Model.DBContext;
using ProjectStore.FileService.RequestObject;
using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

//Autorização. Se a assinatura do token JWT é valido, vai-se confiar na informação lá dentro em vez de pedir ao servidor identitade para verificar

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


        /// <summary>
        /// Obtem todos os ficheiros e diretorias guardadas do utilizador na diretoria principal.
        /// </summary>
        /// <returns>A lista de ficheiros e diretorias na pasta "root" do utilizador</returns>
        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetRoot()
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            List<FileEntity> files = await _fileContext.Files.Where(q => q.Path == "root/" && q.UserId == userId).ToListAsync();
            List<DirectoryEntity> directories = await _fileContext.Directories.Where(q => q.Path == "root/" && q.UserId == userId).ToListAsync();
            if (files.Count == 0 && directories.Count == 0)
            {
                return NotFound();
            }

            ReturnFilesListObject returnval = new ReturnFilesListObject(files, directories);
            return Ok(returnval);
        }


        /// <summary>
        /// Obtem todos os ficheiros e diretorias guardadas do utilizador na diretoria especificada
        /// </summary>
        /// <param name="path">A diretoria a analisar.</param>
        /// <returns>A lista de ficheiros e diretorias na pasta especificada no valor path.</returns>
        [HttpGet("{path}")]
        [Authorize]
        public async Task<IActionResult> Get(string path)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            path = path.Replace("%2F", "/");
            List<FileEntity> files = await _fileContext.Files.Where(q => q.Path == "root/"+path && q.UserId == userId).ToListAsync();
            List<DirectoryEntity> directories = await _fileContext.Directories.Where(q => q.Path == "root/"+path && q.UserId == userId).ToListAsync();
            if (files.Count == 0 && directories.Count == 0)
            {
                return NotFound();
            }

            ReturnFilesListObject returnval = new ReturnFilesListObject(files, directories);
            return Ok(returnval);
        }

        /// <summary>
        /// Retorna metadata sobre o ficheiro, ou transfere o ficheiro
        /// </summary>
        /// <param name="name">O nome do ficheiro</param>
        /// <param name="read">Especifica se é para ler o ficheiro. Atualmente transfere o ficheiro.</param>
        /// <param name="isdir">Especifica se o ficheiro a ler é uma diretoria.</param>
        /// <returns></returns>
        [HttpGet("{name}&{isdir:bool}/{read:bool}")]
        [Authorize]
        public async Task<IActionResult> GetFileInRoot(string name, bool read, bool isdir)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            string userPath = $"./files/{userClaim}/";
            if (name == null)
            {
                return BadRequest();
            }

            if (isdir)
            {
                //correr codigo diretorias.
                DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path+q.DirName == "root/"+name && q.UserId == userId);
                if (dir == null)
                    return NotFound();

                return Ok(new ReturnDirectoryMetadata(dir.DirName, dir.Path));
            }

            //codigo ficheiro
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => q.Path + q.FileName == "root/" + name && q.UserId == userId);
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

            // Retornar DTO com metadata
            return Ok(new ReturnFileMetadata(file.FileName, file.Path));
        }

        /// <summary>
        /// Retorna metadata sobre o ficheiro, ou transfere o ficheiro
        /// </summary>
        /// <param name="name">O nome do ficheiro</param>
        /// <param name="read">Especifica se é para ler o ficheiro. Atualmente transfere o ficheiro.</param>
        /// <param name="isdir">Especifica se o ficheiro a ler é uma diretoria. Como diretorias sao guardadas de uma forma diferente, é necessario logica diferente</param>
        /// <param name="path">O caminho para o ficheiro</param>
        /// <returns>Metadados do ficheiro, ou o ficheiro em si.</returns>
        [HttpGet("{path}/{name}&{isdir:bool}/{read:bool}")]
        [Authorize]
        public async Task<IActionResult> Get(string name, bool read,bool isdir, string path = "")
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            path = path.Replace("%2F", "/");
            string userPath = $"./files/{userClaim}/{path}";
            if (name == null)
            {
                return BadRequest();
            }

            if(isdir)
            {
                //correr codigo diretorias.
                DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path+q.DirName == "root/"+path+name);
                if (dir == null)
                    return NotFound();

                return Ok(new ReturnDirectoryMetadata(dir.DirName, dir.Path));
            }

            //codigo ficheiro
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => q.Path + q.FileName == "root/" + path+name);
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
            
            // Retornar DTO com metadata
            return Ok(new ReturnFileMetadata(file.FileName, file.Path));
        }

        /// <summary>
        /// Envia um ficheiro para o servidor na pasta principal.
        /// </summary>
        /// <param name="file">O ficheiro a enviar</param>
        /// <returns>200 OK</returns>
        [HttpPost("")]
        [RequestSizeLimit(999_000_000)]
        [Authorize]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);

            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            string filePath = $"./files/{userClaim}/{file.FileName}";
            Directory.CreateDirectory($"./files/{userClaim}/");
            //Upload dos ficheiros
            //Ideia basica de upload de ficheiros
            using (var _fileStream = new FileStream($"./files/{userClaim}/{file.FileName}" , FileMode.Create))
            {
                await file.CopyToAsync(_fileStream);
            }
            //Criar entrada na DB
            FileEntity newFile = new FileEntity
            {
                FileName = file.FileName,
                Path = "root/",
                UserId = userId
            };

            _fileContext.Add(newFile);
            await _fileContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Envia um ficheiro para o servidor
        /// </summary>
        /// <param name="file">O ficheiro a enviar</param>
        /// <param name="path">A diretoria onde guardar o ficheiro. Se não existir, é criado uma diretoria.</param>
        /// <returns>200 OK</returns>
        [HttpPost("{path}")]
        [RequestSizeLimit(999_000_000)]
        [Authorize]
        public async Task<IActionResult> CreateFileInPath(IFormFile file, string path)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            // Decoding
            path = path.Replace("%2F", "/");
            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            string filePath = $"./files/{userClaim}/{path}/{file.FileName}";
            Directory.CreateDirectory($"./files/{userClaim}/{path}");
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
                DirectoryEntity? dirCheck = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path == "root/" + currDir && q.DirName == dirName && q.UserId == userId);
                if(dirCheck == null)
                {
                    DirectoryEntity dir = new DirectoryEntity
                    {
                        DirName = dirName,
                        Path = "root/" + currDir,
                        UserId = userId
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
                Path = "root/" + path + "/",
                UserId = userId
            };

            _fileContext.Add(newFile);
            await _fileContext.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{name}&{isdir:bool}")]
        [Authorize]
        public async Task<IActionResult> DeleteOnRoot(string name, bool isdir)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            string userPath = $"./files/{userClaim}/";
            if (name == null)
            {
                return BadRequest();
            }
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => q.Path + q.FileName == "root/"+name && q.UserId == userId);
            DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path + q.DirName == "root/" + name && q.UserId == userId);
            //Encontrar o ficheiro e apagar
            if (dir != null && isdir)
            {
                //Obtem subdiretorias
                List<DirectoryEntity> visitedDirs = new List<DirectoryEntity>();
                List<DirectoryEntity> subdirs = await _fileContext.Directories.Where(q => q.Path == "root/" + name + "/" && q.UserId == userId).ToListAsync();
                while(subdirs.Count > 0)
                {
                    //Remover ultimo elemento
                    DirectoryEntity workingWith = subdirs.Last();
                    subdirs.Remove(workingWith);

                    subdirs.AddRange(await _fileContext.Directories.Where(q => q.Path == workingWith.Path && q.UserId == userId).ToListAsync());
                    
                    //Apagar todos os ficheiros na diretoria.
                    List<FileEntity> fileDelete = await _fileContext.Files.Where(q => q.Path == workingWith.Path && q.UserId == userId).ToListAsync();
                    foreach (FileEntity fileToDelete in fileDelete)
                    {
                        fileToDelete.Path.Replace("root/", "");
                        FileInfo fa = new FileInfo($"{userPath}/{fileToDelete.Path}{fileToDelete.FileName}");
                        fa.Delete();
                        _fileContext.Entry(fileToDelete).State = EntityState.Deleted;
                        _fileContext.SaveChanges();
                    }
                    visitedDirs.Add(workingWith);
                }
                
                foreach(DirectoryEntity dirs in visitedDirs)
                {
                    Directory.Delete($"{userPath}{dir.DirName}");
                    _fileContext.Entry(dirs).State = EntityState.Deleted;
                    _fileContext.SaveChanges();
                }
                
                return Ok();
            }

            if (file == null)
                return NotFound();

            FileInfo f = new FileInfo($"{userPath}{name}");
            if (f.Exists)
            {
                f.Delete();
            }

            _fileContext.Entry(file).State = EntityState.Deleted;
            _fileContext.SaveChanges();
            return Ok();
        }
        /// <summary>
        /// Elemina um ficheiro guardado no servidor.
        /// </summary>
        /// <param name="path">O caminho para o ficheiro</param>
        /// <param name="name">O nome do ficheiro</param>
        /// <returns>200 OK</returns>
        [HttpDelete("{path}/{name}&{isdir:bool}")]
        [Authorize]
        public async Task<IActionResult> Delete(string path, string name, bool isdir)
        {
            string userClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            path = path.Replace("%2F", "/");
            // Usado para eleminar o ficheiro do servidor
            string userPath = $"./files/{userClaim}/{path}";
            if (name == null)
            {
                return BadRequest();
            }
            FileEntity? file = await _fileContext.Set<FileEntity>().FirstOrDefaultAsync(q => q.Path + q.FileName == "root/"+path + name && q.UserId == userId);
            DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path + q.DirName == "root/"+path + name && q.UserId == userId);
            //Encontrar o ficheiro e apagar
            if (dir != null && isdir)
            {
                //Obtem subdiretorias
                List<DirectoryEntity> visitedDirs = new List<DirectoryEntity>();
                List<DirectoryEntity> subdirs = await _fileContext.Directories.Where(q => q.Path == "root/" + path + name + "/" && q.UserId == userId).ToListAsync();
                while (subdirs.Count > 0)
                {
                    //Remover ultimo elemento
                    DirectoryEntity workingWith = subdirs.Last();
                    subdirs.Remove(workingWith);

                    subdirs.AddRange(await _fileContext.Directories.Where(q => q.Path == workingWith.Path && q.UserId == userId).ToListAsync());

                    //Apagar todos os ficheiros na diretoria.
                    List<FileEntity> fileDelete = await _fileContext.Files.Where(q => q.Path == workingWith.Path && q.UserId == userId).ToListAsync();
                    foreach (FileEntity fileToDelete in fileDelete)
                    {
                        fileToDelete.Path.Replace("root/", "");
                        FileInfo fa = new FileInfo($"{userPath}/{fileToDelete.Path}{fileToDelete.FileName}");
                        fa.Delete();
                        _fileContext.Entry(fileToDelete).State = EntityState.Deleted;
                        _fileContext.SaveChanges();
                    }
                    visitedDirs.Add(workingWith);
                }

                foreach (DirectoryEntity dirs in visitedDirs)
                {
                    Directory.Delete($"{userPath}{dir.DirName}");
                    _fileContext.Entry(dirs).State = EntityState.Deleted;
                    _fileContext.SaveChanges();
                }

                return Ok();
            }

            if (file == null)
                return NotFound();

            FileInfo f = new FileInfo($"{userPath}{name}");
            if(f.Exists)
            {
                f.Delete();
            }

            _fileContext.Entry(file).State = EntityState.Deleted;
            _fileContext.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Retorna Ok se o utilizador está autenticado com um token valido
        /// </summary>
        /// <returns>Ok</returns>
        // Apenas testar se consegue validar tokens do serviço de identidade.
        [HttpGet("testingauth")]
        [Authorize]
        public async Task<IActionResult> AuthTest()
        {
            var id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            return Ok();
        }
    }
}
