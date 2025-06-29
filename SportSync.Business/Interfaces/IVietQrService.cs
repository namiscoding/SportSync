using SportSync.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public interface IVietQrService
    {

        Task<string> GenerateAsync(CourtComplex complex, decimal amount, string? addInfo, CancellationToken ct = default);
    }

}
