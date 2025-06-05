using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;
using SportSync.Data.Entities;

namespace SportSync.Business.Interfaces
{
    public interface ICourtService
    {
        Task<Court> GetCourtByIdAsync(int courtId);
        Task UpdateCourtAsync(Court court);
      
    }
}   
