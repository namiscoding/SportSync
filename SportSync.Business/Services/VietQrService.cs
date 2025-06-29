using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Business.Settings;
using SportSync.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class VietQrService : IVietQrService
    {
        private readonly HttpClient _http;
        private readonly VietQrOptions _opt;
        private readonly ILogger<VietQrService> _log;

        public VietQrService(HttpClient http, IOptions<VietQrOptions> opt,
                             ILogger<VietQrService> log)
        {
            _http = http;
            _opt = opt.Value;
            _log = log;
        }

        public async Task<string> GenerateAsync(CourtComplex complex,
                                                decimal amount,
                                                string? addInfo,
                                                CancellationToken ct = default)
        {
            // 1. Chuẩn bị request
            var req = new VietQrRequest(
                accountNo: complex.AccountNumber!,
                accountName: complex.AccountName!.ToUpperInvariant(),
                acqId: complex.BankCode!,         
                addInfo: addInfo,                   
                amount: amount.ToString("0"),       
                template: "compact",
                format: "text"                      
            );

            using var msg = new HttpRequestMessage(HttpMethod.Post,
                             $"{_opt.BaseUrl}/generate")
            {
                Content = JsonContent.Create(req)
            };

            msg.Headers.Add("x-client-id", _opt.ClientId);
            msg.Headers.Add("x-api-key", _opt.ApiKey);

            // 2. Gọi API
            using var res = await _http.SendAsync(msg, ct);

            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("VietQR trả về {Status}", res.StatusCode);
                throw new InvalidOperationException("Không tạo được QR thanh toán.");
            }

            var body = await res.Content.ReadFromJsonAsync<VietQrResponse>(cancellationToken: ct)
                       ?? throw new InvalidOperationException("Không đọc được phản hồi VietQR");

            if (body.Code != "00")
                throw new InvalidOperationException($"VietQR lỗi: {body.Desc}");

            // 3. Trả URL PNG (format=text) hoặc Data-URI (format=image)
            return body.Data.Url ?? body.Data.QrDataURL;
        }
    }
}
