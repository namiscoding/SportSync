using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum ProductType
    {
        Consumable = 0, // Sản phẩm tiêu dùng (nước uống, đồ ăn nhẹ, v.v.)
        Rental = 1, // Sản phẩm cho thuê (vợt, giày, v.v.)
        Service = 2, // Dịch vụ (hướng dẫn viên, huấn luyện viên, v.v.)
        Merchandise = 3 // Hàng hóa (áo thun, mũ, v.v.)
    }
}
