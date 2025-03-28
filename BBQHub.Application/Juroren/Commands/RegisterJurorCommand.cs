using BBQHub.Application.Common.Interfaces;
using BBQHub.Application.Juroren.Dtos;
using BBQHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Juroren.Commands
{
    public class RegisterJurorCommand
    {
        private readonly IAppLogger _logger;
        private readonly IApplicationDbContext _db;

        public RegisterJurorCommand(IAppLogger logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task Handle(JurorDto input)
        {
            try
            {
                var entity = new Juror
                {
                    JuryId = input.JuryId,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    Vereinslocation = input.Vereinslocation
                };

                _db.Juroren.Add(entity);
                await _db.SaveChangesAsync();

                _logger.Info($"Neuer Juror registriert: {input.JuryId}");
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Registrieren des Jurors", ex);
                throw;
            }
        }
    }

}
