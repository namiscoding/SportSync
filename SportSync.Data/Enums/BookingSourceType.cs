using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum BookingSourceType // Default 0 trong DB design
    {
        Website = 0,        // Đặt qua website
        ManualByOwner = 1,  // Chủ sân đặt thủ công
    }
}
