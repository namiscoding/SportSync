using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Settings
{
    public class VietQrOptions
    {
        public string BaseUrl { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
    }
}
