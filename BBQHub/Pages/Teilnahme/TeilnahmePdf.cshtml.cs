using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using System.Threading.Tasks;
using System.Linq;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Teilnahme
{
    public class TeilnahmePdfModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TeilnahmePdfModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int TeilnahmeId { get; set; }

        public TeilnahmePdfData PdfData { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var teilnahme = await _context.spontanTeilnahmen
                .Include(t => t.Durchgang)
                    .ThenInclude(d => d.Event)
                        .ThenInclude(e => e.Logos)
                .FirstOrDefaultAsync(t => t.Id == TeilnahmeId);

            if (teilnahme == null)
            {
                return NotFound();
            }

            var eventEntity = teilnahme.Durchgang.Event;

            var logoVeranstalter = eventEntity.Logos.FirstOrDefault(l => l.Type == Domain.Entities.LogoType.Veranstalter && l.IsActive)?.FilePath;
            var logoHauptsponsor = eventEntity.Logos.FirstOrDefault(l => l.Type == Domain.Entities.LogoType.Hauptsponsor && l.IsActive)?.FilePath;
            var logoNebensponsor = eventEntity.Logos.FirstOrDefault(l => l.Type == Domain.Entities.LogoType.Nebensponsor && l.IsActive)?.FilePath;

            PdfData = new TeilnahmePdfData
            {
                EventName = eventEntity.Name,
                EventStandort = $"{eventEntity.Street}, {eventEntity.ZipCode} {eventEntity.City}",
                EventDatum = eventEntity.StartDate.ToString("dd.MM.yyyy"),
                EventMenue = eventEntity.Menue ?? "",

                Vorname = teilnahme.Vorname ?? "",
                Nachname = teilnahme.Nachname ?? "",
                Adresse = teilnahme.Street,
                PLZ = teilnahme.ZipCode,
                Ort = teilnahme.City,
                Land = teilnahme.Country,
                Email = teilnahme.Email,
                Telefonnummer = teilnahme.Telefonnummer,

                DatenschutzerklaerungUnterschrift = teilnahme.DatenschutzUnterschriftBild ?? "",
                DatenschutzerklaerungDatum = teilnahme.DatenschutzDatum ?? eventEntity.StartDate,
                TeilnahmeUnterschrift = teilnahme.TeilnahmeUnterschriftBild ?? "",
                TeilnahmeDatum = teilnahme.TeilnahmeUnterschriftDatum ?? teilnahme.Anmeldezeit,

                LogoVeranstalter = logoVeranstalter,
                LogoHauptsponsor = logoHauptsponsor,
                LogoNebensponsor = logoNebensponsor
            };

            return Page();
        }
    }
}
