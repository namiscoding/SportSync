using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum DeliveryMethodType // DEFAULT 0 trong DB design
    {
        InApp = 0,  // Thông báo trong ứng dụng/web (mặc định)
        Email = 1,
        SMS = 2
        // FirebaseCloudMessaging = 3 // Cho mobile app
    }
}
