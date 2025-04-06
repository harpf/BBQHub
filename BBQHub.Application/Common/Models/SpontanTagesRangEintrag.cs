using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Common.Models
{
    public class SpontanTagesRangEintrag
    {
        public string Name { get; set; } = string.Empty;
        public int RunNumber { get; set; }
        public Dictionary<int, double> KriterienWerte { get; set; } = new(); // KriteriumId → Punkte
        public double Total { get; set; } = 0;
        public double GesamtGewichtung { get; set; } = 0;
        public double Sauce { get; set; } = 0;
    }

}
