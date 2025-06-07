using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos.OwnerDashboard
{
    public class ComplexInfoSectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public int CourtCount { get; set; }
        public bool IsActive { get; set; }
    }
}
