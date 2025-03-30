using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public int? EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ansprechpartner { get; set; } = string.Empty;
        public string KontaktEmail { get; set; } = string.Empty;
        public Event? Event { get; set; }
        public List<Bewertung> Bewertungen { get; set; } = new();
    }
}
