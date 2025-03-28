using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;
using System.Text;

namespace BBQHub.Pages.Admin
{
    public class ExportJurorenModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ExportJurorenModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var juroren = await _context.Juroren.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,JuryId,FirstName,LastName,Email,Vereinslocation");

            foreach (var j in juroren)
            {
                sb.AppendLine($"{j.Id},{j.JuryId},{j.FirstName},{j.LastName},{j.Email},{j.Vereinslocation}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "juroren_export.csv");
        }
    }
}
