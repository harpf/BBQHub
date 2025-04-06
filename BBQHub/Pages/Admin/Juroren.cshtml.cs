using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Admin;

public class JurorenModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public JurorenModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<BBQHub.Domain.Entities.Juror> JurorenList { get; set; } = new List<BBQHub.Domain.Entities.Juror>();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortOrder { get; set; }

    public async Task OnGetAsync()
    {
        int pageSize = 10;
        var query = _context.Juroren.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(j =>
                j.FirstName.Contains(SearchTerm) ||
                j.LastName.Contains(SearchTerm) ||
                j.Email.Contains(SearchTerm) ||
                j.Vereinslocation.Contains(SearchTerm));
        }

        query = SortOrder switch
        {
            "firstname_desc" => query.OrderByDescending(j => j.FirstName),
            "lastname" => query.OrderBy(j => j.LastName),
            "lastname_desc" => query.OrderByDescending(j => j.LastName),
            _ => query.OrderBy(j => j.FirstName)
        };

        int totalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        JurorenList = await query
            .Skip((CurrentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
