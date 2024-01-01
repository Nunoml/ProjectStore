﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProjectStore.FileService.Model;
using ProjectStore.FileService.Model.DBContext;
using ProjectStore.FileService.RequestObject;
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


        /// <summary>
        /// Obtem todos os ficheiros e diretorias guardadas do utilizador na diretoria principal.
        /// </summary>
        /// <returns>A lista de ficheiros e diretorias na pasta "root" do utilizador</returns>
        [HttpGet()]
        public async Task<IActionResult> GetRoot()
        {
            List<FileEntity> files = await _fileContext.Files.Where(q => q.Path == "root/" && q.UserId == 0).ToListAsync();
            List<DirectoryEntity> directories = await _fileContext.Directories.Where(q => q.Path == "root/" && q.UserId == 0).ToListAsync();
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
        public async Task<IActionResult> Get(string path)
        {
            List<FileEntity> files = await _fileContext.Files.Where(q => q.Path == "root/"+path && q.UserId == 0).ToListAsync();
            List<DirectoryEntity> directories = await _fileContext.Directories.Where(q => q.Path == "root/"+path && q.UserId == 0).ToListAsync();
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
        /// <param name="isdir">Especifica se o ficheiro a ler é uma diretoria. Como diretorias sao guardadas de uma forma diferente, é necessario logica diferente</param>
        /// <param name="path">O caminho para o ficheiro</param>
        /// <returns>Metadados do ficheiro, ou o ficheiro em si.</returns>
        [HttpGet("{path}/{name}&{isdir:bool}/{read:bool}")]
        public async Task<IActionResult> Get(string name, bool read,bool isdir, string path = "")
        {
            string userPath = "./files/0/" + path;
            if (name == null)
            {
                return BadRequest();
            }

            if(isdir)
            {
                //correr codigo diretorias.
                DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => ($"{q.Path}{q.DirName}") == ($"{userPath}{name}"));
                if (dir == null)
                    return NotFound();

                return Ok(new ReturnDirectoryMetadata(dir.DirName, dir.Path));
            }

            //codigo ficheiro
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

        /// <summary>
        /// Envia um ficheiro para o servidor
        /// </summary>
        /// <param name="file">O ficheiro a enviar</param>
        /// <param name="path">A diretoria onde guardar o ficheiro. Se não existir, é criado uma diretoria.</param>
        /// <returns>200 OK</returns>
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

        /// <summary>
        /// Elemina um ficheiro guardado no servidor.
        /// </summary>
        /// <param name="path">O caminho para o ficheiro</param>
        /// <param name="name">O nome do ficheiro</param>
        /// <returns>200 OK</returns>
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
            DirectoryEntity? dir = await _fileContext.Set<DirectoryEntity>().FirstOrDefaultAsync(q => q.Path + q.DirName == path + name && q.UserId == 0);
            //Encontrar o ficheiro e apagar
            return Ok();
        }
    }
}
