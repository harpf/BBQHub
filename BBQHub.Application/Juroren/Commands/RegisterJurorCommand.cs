using BBQHub.Application.Common.Interfaces;
using BBQHub.Application.Juroren.Dtos;
using BBQHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Juroren.Commands
{
    public class RegisterJurorCommand
    {
        private readonly IApplicationDbContext _context;
        private readonly IAppLogger _logger;

        public RegisterJurorCommand(IApplicationDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task HandleAsync(JurorDto dto)
        {
            if (await _context.Juroren.AnyAsync(j => j.JuryId == dto.JuryId))
            {
                throw new Exception("Jury-ID bereits vergeben.");
            }

            var entity = new Juror
            {
                JuryId = dto.JuryId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Vereinslocation = dto.Vereinslocation,
                Telefonnummer = dto.Telefonnummer
            };

            _context.Juroren.Add(entity);
            await _context.SaveChangesAsync();
            _logger.Info($"Juror {dto.JuryId} erfolgreich erstellt.");
        }
    }
}
