using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBQHub.Application.Common.Interfaces;

namespace BBQHub.Pages.Ranglisten
{
    public class ExportModel : PageModel
    {
        private readonly IExportService _exportService;

        public ExportModel(IExportService exportService)
        {
            _exportService = exportService;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DurchgangId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } = "gesamt"; // oder "durchgang"

        public async Task<IActionResult> OnGetAsync()
        {
            var pdfBytes = await _exportService.ExportRanglisteAsync(EventId, DurchgangId, Type);
            return File(pdfBytes, "application/pdf", $"Rangliste_{Type}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}
