using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class KriteriumAuswertung
    {
        public string KriteriumName { get; set; } = string.Empty;
        public int VergebenePunkte { get; set; }
        public double GewichteteNote { get; set; }
    }
}
