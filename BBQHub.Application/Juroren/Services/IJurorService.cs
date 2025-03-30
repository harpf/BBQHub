using BBQHub.Application.Juroren.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Juroren.Services
{
    public interface IJurorService
    {
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(JurorDto dto);
    }
}
