using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;

namespace BBQHub.Pages.Admin.Events
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Event Input { get; set; } = new();
        [BindProperty]
        public int SelectedTeamId { get; set; }

        public List<SelectListItem> Managers { get; set; } = new();
        public List<Durchgang> Durchgaenge { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
        public List<EventTeamAssignment> AssignedTeams { get; set; }
        public List<SelectListItem> AvailableTeams { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Durchgaenge)
                    .ThenInclude(d => d.Kriterien)
                .Include(e => e.Teams)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            Input = ev;
            Durchgaenge = ev.Durchgaenge.ToList();
            Teams = ev.Teams.ToList();

            Managers = await _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                }).ToListAsync();

            AssignedTeams = await _context.EventTeamAssignments
                .Include(a => a.Team)
                .Where(a => a.EventId == id)
                .ToListAsync();

            AvailableTeams = await _context.Teams
                .Where(t => !_context.EventTeamAssignments.Any(a => a.TeamId == t.Id && a.EventId == id))
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Events.FindAsync(Input.Id);
            if (existing == null)
                return NotFound();

            existing.Name = Input.Name;
            existing.Address = Input.Address;
            existing.Description = Input.Description;
            existing.ManagerId = Input.ManagerId;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/Index");
        }
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var existing = await _context.Events.FindAsync(Input.Id);
            if (existing == null)
                return NotFound();

            _context.Events.Remove(existing);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/Index");
        }

        public async Task<IActionResult> OnPostAssignTeamAsync(int eventId)
        {
            if (SelectedTeamId <= 0)
                return RedirectToPage(new { id = eventId });

            // Prüfen ob Team bereits zugewiesen wurde
            bool alreadyAssigned = await _context.EventTeamAssignments
                .AnyAsync(a => a.EventId == eventId && a.TeamId == SelectedTeamId);

            if (alreadyAssigned)
                return RedirectToPage(new { id = eventId });

            // Token generieren, der innerhalb dieses Events eindeutig ist
            int token;
            var rand = new Random();
            const int maxTries = 20;
            int tries = 0;

            do
            {
                token = rand.Next(100000, 999999); // 6-stellige Zahl
                tries++;

                if (tries > maxTries)
                    throw new Exception("Konnte keinen eindeutigen Token generieren.");
            }
            while (await _context.EventTeamAssignments
                .AnyAsync(a => a.EventId == eventId && a.Token == token));

            // Zuweisung speichern
            var assignment = new EventTeamAssignment
            {
                EventId = eventId,
                TeamId = SelectedTeamId,
                Token = token
            };

            _context.EventTeamAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = eventId });
        }

    }
}
