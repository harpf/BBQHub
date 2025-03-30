using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class DurchgangAuswertung
    {
        public string DurchgangName { get; set; } = string.Empty;
        public double GesamtGewichteteNote { get; set; }
        public List<KriteriumAuswertung> Kriterien { get; set; } = new();
    }
}
