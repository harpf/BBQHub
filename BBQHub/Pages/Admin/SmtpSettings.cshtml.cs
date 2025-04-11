using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Application.Common.Interfaces;

namespace BBQHub.Pages.Admin
{
    public class SmtpSettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public SmtpSettingsModel(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [BindProperty]
        public SmtpSettings Input { get; set; } = new();
        [BindProperty]
        public string TestEmpfaengerEmail { get; set; }

        // Laden der aktuellen SMTP-Einstellungen
        public async Task<IActionResult> OnGetAsync()
        {
            var existing = await _context.SmtpSettings.FirstOrDefaultAsync();
            if (existing != null)
                Input = existing;

            return Page();
        }

        // Speichern der SMTP-Einstellungen
        public async Task<IActionResult> OnPostSaveAsync()
        {
            var existing = await _context.SmtpSettings.FirstOrDefaultAsync();

            if (existing == null)
                _context.SmtpSettings.Add(Input);
            else
            {
                existing.ClientId = Input.ClientId;
                existing.TenantId = Input.TenantId;
                existing.ClientSecret = Input.ClientSecret;
                existing.SenderEmail = Input.SenderEmail;
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "✅ Einstellungen gespeichert.";
            return RedirectToPage();
        }

        // Test-E-Mail versenden
        public async Task<IActionResult> OnPostSendTestAsync()
        {
            var settings = await _context.SmtpSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                TempData["Message"] = "⚠️ Es wurden noch keine SMTP-Einstellungen gespeichert.";
                return RedirectToPage();
            }

            try
            {
                var sender = settings.SenderEmail;

                // Verwende TestEmpfaengerEmail, falls angegeben, sonst SenderEmail als Fallback
                var empfaenger = string.IsNullOrWhiteSpace(TestEmpfaengerEmail) ? settings.SenderEmail : TestEmpfaengerEmail;

                // Überprüfen, ob eine gültige Empfängeradresse vorliegt
                if (string.IsNullOrWhiteSpace(empfaenger))
                {
                    TempData["Message"] = "⚠️ Keine gültige Test-E-Mail-Adresse angegeben.";
                    return RedirectToPage();
                }

                // Testmail versenden
                await _emailSender.SendMailAsync(
                    toEmail: empfaenger,
                    subject: "✅ Testmail von BBQHub",
                    bodyHtml: $"<p>Diese Nachricht wurde erfolgreich per Microsoft 365 (OAuth2) gesendet.</p><p><strong>Sender:</strong> {sender}</p>"
                );

                TempData["Message"] = $"✅ Test-E-Mail wurde an {empfaenger} gesendet.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"❌ Fehler beim Senden der Testmail: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}