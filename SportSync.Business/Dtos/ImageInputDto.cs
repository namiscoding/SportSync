using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class ImageInputDto
    {
        public Stream Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
