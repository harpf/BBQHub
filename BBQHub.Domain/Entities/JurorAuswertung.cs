using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class JurorAuswertung
    {
        public int JurorId { get; set; }
        public string JurorName { get; set; } = string.Empty;
        public List<DurchgangAuswertung> Durchgaenge { get; set; } = new();
    }
}
