using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace HelloPlatform.Controllers
{
    /// <summary>
    /// Student Schedule Controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")] // Makes the default Content-Type for all responses "application/json". Put something different above your method if you want to override it.
    public class PhotoController : ControllerBase
    {
 
         public static IWebHostEnvironment _webHostEnvironment;

         public PhotoController(IWebHostEnvironment webHostEnvironment)
         {
             _webHostEnvironment = webHostEnvironment;
         }

         /// <summary>
         /// Update picture
         /// <summary>
         [HttpPost("{Name}")]
         public IActionResult Post([FromForm] FileUpload fileUpload, string Name)
         {
            List<Student> students = ScheduleController.Roster;
            Student checkName = Adapter.Check(Name, students);
            if (checkName != null)
            {
                try
                {
                    if (fileUpload.FileToUpload.Length > 0)
                    {
                        string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        using (FileStream fileStream = System.IO.File.Create(path + fileUpload.FileToUpload.FileName))
                        {
                            fileUpload.FileToUpload.CopyTo(fileStream);
                            fileStream.Flush();
                            string filename = fileUpload.FileToUpload.FileName;
                            checkName.Photo = filename;
                            SavingInfo.UpdateFile(ScheduleController.Roster);
                            return Ok(200);
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            return NotFound(Name + " is not a registered student.");
         }

         /// <summary>
         /// Get picture
         /// <summary>
         [HttpGet("{Name}")]
         public async Task<IActionResult> GetPhoto([FromRoute] string Name)
         {
            List<Student> students = ScheduleController.Roster;
            Student checkName = Adapter.Check(Name, students);
            if (checkName != null)
            {
                string fileName = checkName.Photo;
                string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
                var filePath = path + fileName;
                if (System.IO.File.Exists(filePath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                    return File(bytes, "image/png");
                }
            }
             return NotFound(Name + " is not a registered student.");
         }
    }
}
