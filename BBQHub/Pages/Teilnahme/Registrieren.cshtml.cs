using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Teilnahme
{
    public class RegistrierenModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegistrierenModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty]
        public string TeilnehmerName { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedDurchgangId { get; set; }
        [BindProperty] public string Street { get; set; } = string.Empty;
        [BindProperty] public string ZipCode { get; set; } = string.Empty;
        [BindProperty] public string City { get; set; } = string.Empty;
        [BindProperty] public string Country { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Telefonnummer { get; set; } = string.Empty;
        [BindProperty]
        public int Token { get; set; }



        public string EventName { get; set; } = string.Empty;
        public List<SelectListItem> Durchgaenge { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var ev = await _context.Events
                .Include(e => e.Durchgaenge)
                .FirstOrDefaultAsync(e => e.Id == EventId && e.Typ == EventType.SpontanTeilnahme);

            if (ev == null) return NotFound();

            EventName = ev.Name;

            Durchgaenge = ev.Durchgaenge
                .OrderBy(d => d.Durchgangsnummer)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"Durchgang {d.Durchgangsnummer}: {d.Name}"
                }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(TeilnehmerName) || SelectedDurchgangId <= 0)
                return RedirectToPage(new { eventId = EventId });

            var exists = await _context.spontanTeilnahmen
                .AnyAsync(t => t.Name == TeilnehmerName && t.DurchgangId == SelectedDurchgangId);

            var maxTries = 20;
            var tries = 0;
            var rand = new Random();
            int token;

            do
            {
                token = rand.Next(10000, 99999); // 5-stellige Zahl
                tries++;

                if (tries > maxTries)
                    throw new Exception("Konnte keinen eindeutigen Token generieren.");

            } while (await _context.spontanTeilnahmen
                .AnyAsync(t => t.DurchgangId == SelectedDurchgangId && t.Token == token));

            if (!exists)
            {
                var teilnahme = new SpontanTeilnahme
                {
                    Name = TeilnehmerName,
                    Street = Street,
                    ZipCode = ZipCode,
                    City = City,
                    Country = Country,
                    Email = Email,
                    Telefonnummer = Telefonnummer,
                    DurchgangId = SelectedDurchgangId,
                    Anmeldezeit = DateTime.Now,
                    Token = token
                };


                _context.spontanTeilnahmen.Add(teilnahme);
                await _context.SaveChangesAsync();
            }



            return RedirectToPage("/Teilnahme/Bestätigung");
        }
    }
}
