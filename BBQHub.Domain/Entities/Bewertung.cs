using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class Bewertung
    {
        public int Id { get; set; }
        public int JurorId { get; set; }
        public int DurchgangId { get; set; }
        public int KriteriumId { get; set; }
        public double Punkte { get; set; }
        public double GewichteteNote { get; set; }
        public string? TextBewertung { get; set; }
        public int? TeamId { get; set; }
        public int? SpontanTeilnahmeId { get; set; }

        public Juror Juror { get; set; } = default!;
        public Durchgang Durchgang { get; set; } = default!;
        public Kriterium Kriterium { get; set; } = default!;
        public Team? Team { get; set; }
        public SpontanTeilnahme? SpontanTeilnahme { get; set; }
    }
}
