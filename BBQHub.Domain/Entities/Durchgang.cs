using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BBQHub.Domain.Entities
{
    public class Durchgang
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int Durchgangsnummer { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Kriterium> Kriterien { get; set; } = new();
        public List<Bewertung> Bewertungen { get; set; } = new();
    }
}
