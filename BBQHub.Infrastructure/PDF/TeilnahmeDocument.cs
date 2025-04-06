using BBQHub.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Text;

public class TeilnahmeDocument : IDocument
{
    private readonly TeilnahmePdfData _model;

    public TeilnahmeDocument(TeilnahmePdfData model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        // Seite 1: Teilnahmeformular
        container.Page(page =>
        {
            page.Margin(30);
            page.Content().Column(col =>
            {
                // Logos
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Height(60).Column(logoCol =>
                    {
                        if (!string.IsNullOrWhiteSpace(_model.LogoVeranstalter))
                        {
                            var logoPath = Path.Combine("wwwroot", _model.LogoVeranstalter.TrimStart('/'));
                            if (System.IO.File.Exists(logoPath))
                            {
                                var logo = System.IO.File.ReadAllBytes(logoPath);
                                logoCol.Item().Image(logo).FitHeight();
                            }
                        }
                    });

                    row.RelativeItem().AlignCenter().Height(60).Column(logoCol =>
                    {
                        if (!string.IsNullOrWhiteSpace(_model.LogoHauptsponsor))
                        {
                            var logoPath = Path.Combine("wwwroot", _model.LogoHauptsponsor.TrimStart('/'));
                            if (System.IO.File.Exists(logoPath))
                            {
                                var logo = System.IO.File.ReadAllBytes(logoPath);
                                logoCol.Item().Image(logo).FitHeight();
                            }
                        }
                    });

                    row.RelativeItem().AlignRight().Height(60).Column(logoCol =>
                    {
                        if (!string.IsNullOrWhiteSpace(_model.LogoNebensponsor))
                        {
                            var logoPath = Path.Combine("wwwroot", _model.LogoNebensponsor.TrimStart('/'));
                            if (System.IO.File.Exists(logoPath))
                            {
                                var logo = System.IO.File.ReadAllBytes(logoPath);
                                logoCol.Item().Image(logo).FitHeight();
                            }
                        }
                    });
                });

                col.Item().PaddingBottom(10);

                // Event Infos
                col.Item().Text(text =>
                {
                    text.Span("Teilnahmeformular – ").SemiBold().FontSize(16).FontColor(Colors.Orange.Medium);
                    text.Span(_model.EventName).SemiBold().FontSize(16);
                });

                col.Item().PaddingTop(10).Text(txt =>
                {
                    txt.Span("Vorausscheidung: ").Bold();
                    txt.Span(_model.EventStandort);
                });
                col.Item().Text($"Datum: {_model.EventDatum:dd.MM.yyyy}");
                col.Item().Text(txt =>
                {
                    txt.Span("Menü: ").Bold();
                    txt.Span(_model.EventMenue);
                });

                col.Item().PaddingTop(10);
                col.Item().Text("Teilnehmerdaten").Bold().FontSize(13);
                col.Item().Text($"{_model.Vorname} {_model.Nachname}");
                col.Item().Text($"{_model.Adresse}, {_model.PLZ} {_model.Ort}");
                col.Item().Text($"Land: {_model.Land}");
                col.Item().Text($"E-Mail: {_model.Email}");
                col.Item().Text($"Telefon: {_model.Telefonnummer}");

                col.Item().PaddingTop(10);
                col.Item().Text("✅ Datenschutzerklärung wurde bestätigt am " + _model.DatenschutzerklaerungDatum.ToString("dd.MM.yyyy")).FontSize(11);
                if (!string.IsNullOrEmpty(_model.DatenschutzerklaerungUnterschrift))
                {
                    var dsBytes = Convert.FromBase64String(_model.DatenschutzerklaerungUnterschrift.Replace("data:image/png;base64,", ""));
                    col.Item().Container().Height(100).Image(dsBytes);
                }

                col.Item().PaddingTop(10);
                col.Item().Text("Einverständniserklärung").Bold().FontSize(13);
                col.Item().Text("☑ Veröffentlichung der Ranglisten ist erlaubt.");
                col.Item().Text("☑ Datenschutzerklärung wurde gelesen.");
                col.Item().Text("☑ Einwilligung zu Foto- und Filmaufnahmen.");
                col.Item().Text("☑ Volljährigkeit oder Begleitung durch Erwachsene.");

                col.Item().PaddingTop(10);
                col.Item().Text("Unterschrift Teilnahme am " + _model.TeilnahmeDatum.ToString("dd.MM.yyyy")).FontSize(11).Bold();
                if (!string.IsNullOrEmpty(_model.TeilnahmeUnterschrift))
                {
                    var teilnahmeBytes = Convert.FromBase64String(_model.TeilnahmeUnterschrift.Replace("data:image/png;base64,", ""));
                    col.Item().Container().Height(100).Image(teilnahmeBytes);
                }
            });
        });

        // Seite 2: Datenschutzerklärung
        container.Page(page =>
        {
            page.Margin(30);
            page.Content().Column(col =>
            {
                col.Item().Text("Einwilligungserklärung zur Verwendung von Film- und Fotoaufnahmen")
                    .Bold().FontSize(14); //.PaddingBottom(10);

                col.Item().Text(text =>
                {
                    text.Span("Im Rahmen der Veranstaltung ").SemiBold();
                    text.Span(_model.EventName).Bold();
                    text.Span(" wird der Verantwortliche an allen Veranstaltungstagen Foto- und Filmaufnahmen anfertigen, auf denen auch die Teilnehmer zu sehen sind.");
                }); //.FontSize(10).PaddingBottom(5);

                col.Item().Text("Die Aufnahmen können für Präsentationen, Dokumentationen, Werbung u. v. m. genutzt werden. Die Nutzung ist geografisch und zeitlich unbegrenzt möglich.").FontSize(10);
                col.Item().Text("Die Aufnahmen dürfen außerdem Dritten (z. B. Sponsoren) zur Verfügung gestellt werden.").FontSize(10);
                col.Item().Text("Die Einwilligung kann jederzeit in Textform widerrufen werden.").FontSize(10).Italic(); //.PaddingBottom(10);

                col.Item().Text("Mit meiner Unterschrift erkläre ich mich mit der beschriebenen Nutzung einverstanden.").FontSize(11).Bold(); //.PaddingTop(10).Bold();

                // Unterschriftfeld
                col.Item().PaddingTop(10);
                if (!string.IsNullOrEmpty(_model.DatenschutzerklaerungUnterschrift))
                {
                    var dsBytes = Convert.FromBase64String(_model.DatenschutzerklaerungUnterschrift.Replace("data:image/png;base64,", ""));
                    col.Item().Container().Height(100).Image(dsBytes);
                }

                col.Item().Text($"{_model.Vorname} {_model.Nachname}, {_model.DatenschutzerklaerungDatum:dd.MM.yyyy}").FontSize(10);
            });
        });
    }

}