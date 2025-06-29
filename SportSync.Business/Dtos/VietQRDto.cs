using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public record VietQrRequest(
      string accountNo,
      string accountName,
      string acqId,         // BIN ngân hàng, VD: 970415
      string? addInfo,
      string? amount,
      string template = "compact",   // compact | qr_only | ...
      string format = "text");     // text | image | ...

    public class VietQrResponse
    {
        public string Code { get; set; } = default!;   // "00" = OK
        public string Desc { get; set; } = default!;
        public VietQrData Data { get; set; } = default!;
    }

    public class VietQrData
    {
        public string QrCode { get; set; } = default!; // raw string
        public string QrDataURL { get; set; } = default!; // "data:image/png;base64,..."
        public string? Url { get; set; }             // chỉ khi format=text
    }
}
