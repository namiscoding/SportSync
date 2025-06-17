using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class SportType
    {
        public int SportTypeId { get; set; } 

        public string Name { get; set; } = default!; 

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<CourtComplex>? CourtComplexes { get; set; }

        public SportType()
        {
            CourtComplexes = new HashSet<CourtComplex>();
        }   
    }
}
