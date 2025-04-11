using Azure.Identity;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

public class EmailSender : IEmailSender
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ApplicationDbContext context, ILogger<EmailSender> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendTestMailAsync(string toEmail)
    {
        await SendMailAsync(toEmail, "✅ Testmail von BBQHub", "<p>Diese Nachricht wurde erfolgreich per Microsoft 365 (OAuth2) gesendet.</p>");
    }

    public async Task SendMailAsync(string toEmail, string subject, string bodyHtml, string? fromEmail = null)
    {
        var settings = await _context.SmtpSettings.FirstOrDefaultAsync();
        if (settings == null)
            throw new InvalidOperationException("Keine SMTP-Einstellungen gefunden.");

        var clientSecretCredential = new ClientSecretCredential(
            settings.TenantId,
            settings.ClientId,
            settings.ClientSecret
        );

        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = bodyHtml
            },
            ToRecipients = new List<Recipient>
        {
            new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = toEmail
                }
            }
        }
        };

        // Optional: Absender definieren, wenn über Delegation erlaubt
        if (!string.IsNullOrWhiteSpace(fromEmail))
        {
            message.From = new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = fromEmail
                }
            };
        }

        _logger.LogInformation("Versende E-Mail an: {Recipient}, Betreff: {Subject}", toEmail, subject);

        try
        {
            // E-Mail versenden
            await graphClient.Users[settings.SenderEmail]
                .SendMail
                .PostAsync(new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });

            // Erfolgreicher Versand loggen
            _logger.LogInformation("E-Mail erfolgreich versendet an: {Recipient}, Betreff: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            // Fehler loggen, falls Versand fehlschlägt
            _logger.LogError(ex, "Fehler beim Versenden der E-Mail an: {Recipient}, Betreff: {Subject}", toEmail, subject);
        }
    }
}
