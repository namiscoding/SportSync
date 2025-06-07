using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Settings
{
    public class ESmsSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ApiEndpoint { get; set; } = "https://api.esms.vn/MainService.svc/json/SendMultipleMessage_V4_post_json/";

        // SmsType: "2" cho Brandname, "8" cho số cố định, "4" cho số ngẫu nhiên...
        // Bạn cần kiểm tra với eSMS để chọn loại phù hợp. Loại "2" hoặc "8" thường dùng cho OTP.
        public string SmsType { get; set; } = "8";

        // Điền Brandname của bạn nếu dùng SmsType 2
        public string? Brandname { get; set; }
    }
}
