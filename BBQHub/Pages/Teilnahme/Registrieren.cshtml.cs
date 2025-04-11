using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.PDF;

namespace BBQHub.Pages.Teilnahme
{
    public class RegistrierenModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PDFRegistrationFormular _pdfGenerator;

        public RegistrierenModel(ApplicationDbContext context, PDFRegistrationFormular pdfGenerator)
        {
            _context = context;
            _pdfGenerator = pdfGenerator;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty]
        public string Vorname { get; set; } = string.Empty;

        [BindProperty]
        public string Nachname { get; set; } = string.Empty;

        [BindProperty]
        public string Street { get; set; } = string.Empty;

        [BindProperty]
        public string ZipCode { get; set; } = string.Empty;

        [BindProperty]
        public string City { get; set; } = string.Empty;

        [BindProperty]
        public string Country { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Telefonnummer { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedDurchgangId { get; set; }

        [BindProperty]
        public string DatenschutzUnterschrift { get; set; } = string.Empty;

        [BindProperty]
        public DateTime DatenschutzDatum { get; set; }

        [BindProperty]
        public string Unterschrift { get; set; } = string.Empty;

        [BindProperty]
        public DateTime Datum { get; set; }

        [BindProperty]
        public string? UnterschriftBild { get; set; }
        public string? LogoVeranstalter { get; set; }
        public string? LogoHauptsponsor { get; set; }
        public string? LogoNebensponsor { get; set; }

        public string EventName { get; set; } = string.Empty;
        public string EventStandort { get; set; } = string.Empty;
        public DateTime EventDatum { get; set; }
        public string EventMenue { get; set; } = string.Empty;

        public List<SelectListItem> Durchgaenge { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var ev = await _context.Events
                .Include(e => e.Durchgaenge)
                .Include(e => e.Logos)
                .FirstOrDefaultAsync(e => e.Id == EventId && e.Typ == EventType.SpontanTeilnahme);

            if (ev == null) return NotFound();

            EventName = ev.Name;
            EventStandort = $"{ev.Street}, {ev.ZipCode} {ev.City}, {ev.Country}";
            EventDatum = ev.StartDate;
            EventMenue = ev.Menue ?? "";

            LogoVeranstalter = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Veranstalter && l.IsActive)?.FilePath;
            LogoHauptsponsor = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Hauptsponsor && l.IsActive)?.FilePath;
            LogoNebensponsor = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Nebensponsor && l.IsActive)?.FilePath;

            Durchgaenge = ev.Durchgaenge
                .OrderBy(d => d.Durchgangsnummer)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"Durchgang {d.Durchgangsnummer}: {d.Name}"
                }).ToList();

            // Daten von Datenschutz-Seite übernehmen
            if (TempData.TryGetValue("Vorname", out var v)) Vorname = v?.ToString() ?? "";
            if (TempData.TryGetValue("Nachname", out var n)) Nachname = n?.ToString() ?? "";
            if (TempData.TryGetValue("DatenschutzUnterschrift", out var u)) DatenschutzUnterschrift = u?.ToString() ?? "";
            if (TempData.TryGetValue("Datum", out var d) && DateTime.TryParse(d?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate))
            {
                DatenschutzDatum = parsedDate;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (SelectedDurchgangId <= 0 || string.IsNullOrWhiteSpace(Vorname) || string.IsNullOrWhiteSpace(Nachname))
            {
                ModelState.AddModelError("", "Bitte alle Pflichtfelder ausfüllen.");
                return await OnGetAsync();
            }

            //if (string.IsNullOrWhiteSpace(DatenschutzUnterschrift))
            //{
            //    ModelState.AddModelError("", "Du musst zuerst die Datenschutzerklärung bestätigen.");
            //    return await OnGetAsync();
            //}

            // Kombinierter Teilnehmername
            var teilnehmerName = $"{Vorname} {Nachname}";

            var exists = await _context.spontanTeilnahmen
                .AnyAsync(t => t.Vorname == Vorname && t.Nachname == Nachname && t.DurchgangId == SelectedDurchgangId);

            // Token generieren
            var maxTries = 20;
            var tries = 0;
            var rand = new Random();
            int token;
            do
            {
                token = rand.Next(10000, 99999);
                tries++;
                if (tries > maxTries)
                    throw new Exception("Konnte keinen eindeutigen Token generieren.");
            }
            while (await _context.spontanTeilnahmen
                .AnyAsync(t => t.DurchgangId == SelectedDurchgangId && t.Token == token));

            if (!exists)
            {
                var teilnahme = new SpontanTeilnahme
                {
                    Vorname = Vorname,
                    Nachname = Nachname,
                    Street = Street,
                    ZipCode = ZipCode,
                    City = City,
                    Country = Country,
                    Email = Email,
                    Telefonnummer = Telefonnummer,
                    DurchgangId = SelectedDurchgangId,
                    Anmeldezeit = DateTime.Now,
                    DatenschutzUnterschriftBild = DatenschutzUnterschrift,
                    DatenschutzDatum = DatenschutzDatum,
                    TeilnahmeUnterschriftBild = UnterschriftBild,
                    TeilnahmeUnterschriftDatum = DateTime.Now,
                    Token = token
                };

                
                _context.spontanTeilnahmen.Add(teilnahme);
                await _context.SaveChangesAsync();

            }

            var durchgang = await _context.Durchgaenge
                    .Include(d => d.Event)
                    .ThenInclude(e => e.Logos)
                    .FirstOrDefaultAsync(d => d.Id == SelectedDurchgangId);

            if (durchgang == null || durchgang.Event == null)
                return NotFound();

            //var ev = durchgang.Event;

            //var pdfModel = new TeilnahmePdfData
            //{
            //    EventName = ev.Name,
            //    EventStandort = $"{ev.Street}, {ev.ZipCode} {ev.City}",
            //    EventDatum = ev.StartDate.ToString("dd.MM.yyyy"),
            //    EventMenue = ev.Menue ?? "",
            //    Vorname = Vorname,
            //    Nachname = Nachname,
            //    Adresse = Street,
            //    PLZ = ZipCode,
            //    Ort = City,
            //    Land = Country,
            //    Email = Email,
            //    Telefonnummer = Telefonnummer,
            //    DatenschutzerklaerungUnterschrift = DatenschutzUnterschrift,
            //    DatenschutzerklaerungDatum = DatenschutzDatum,
            //    TeilnahmeUnterschrift = UnterschriftBild,
            //    TeilnahmeDatum = DateTime.Now,
            //    LogoVeranstalter = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Veranstalter && l.IsActive)?.FilePath,
            //    LogoHauptsponsor = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Hauptsponsor && l.IsActive)?.FilePath,
            //    LogoNebensponsor = ev.Logos.FirstOrDefault(l => l.Type == LogoType.Nebensponsor && l.IsActive)?.FilePath
            //};

            if (!exists)
            {
                //var pdfBytes = _pdfGenerator.Generate(pdfModel);
                return RedirectToPage("/Teilnahme/Bestätigung");
                // return File(pdfBytes, "application/pdf", "Teilnahmeformular.pdf");
            }
            else
            {
                return RedirectToPage("/Teilnahme/Bestätigung");
            }
        }
    }
}
