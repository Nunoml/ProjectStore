﻿using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectStore.FileService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // GET: api/<FilesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FilesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FilesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IFormFile file)
        {
            // A pasta root do utilizador seria o id do user?
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            //Upload dos ficheiros
            
            return Ok();
        }

        // PUT api/<FilesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FilesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
