using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Application.Juroren.Dtos;
using BBQHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Application.Juroren.Services
{
    public class JurorService : IJurorService
    {
        private readonly IApplicationDbContext _context;

        public JurorService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(JurorDto dto)
        {
            if (await _context.Juroren.AnyAsync(j => j.JuryId == dto.JuryId))
                return (false, "Diese Jury ID ist bereits vergeben.");

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                await _context.Juroren.AnyAsync(j => j.Email == dto.Email))
                return (false, "Diese E-Mail-Adresse ist bereits registriert.");

            if (string.IsNullOrWhiteSpace(dto.Vereinslocation))
                return (false, "Bitte ein Land auswählen.");

            var juror = new Juror
            {
                JuryId = dto.JuryId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Vereinslocation = dto.Vereinslocation,
                Telefonnummer = dto.Telefonnummer
            };

            _context.Juroren.Add(juror);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}
