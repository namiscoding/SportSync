using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum PaymentMethodType // Default 0 trong DB design
    {
        PayAtCourt = 0,     // Thanh toán tại sân (MVP)
        VnPay = 1,          // Tương lai
        MoMo = 2,           // Tương lai
        BankTransfer = 3    // Tương lai
        //Thêm các loại khác nếu cần
    }
}
