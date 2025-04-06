using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HelloPlatform
{
    public class FileUpload
    {
        /// <summary>
        /// File to upload
        /// </summary>
        public IFormFile? FileToUpload { get; set; }
    }
}
