using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public enum BewertungsTyp
    {
        Integer = 0,
        Decimal = 1,
        Text = 2
    }
    public class Kriterium
    {
        public int Id { get; set; }

        public int DurchgangId { get; set; }
        public string Name { get; set; } = string.Empty;

        // Optional bei Text
        public int? MaxWert { get; set; } = 10;

        public double Gewichtung { get; set; } = 1.0;

        public BewertungsTyp BewertungsTyp { get; set; } = BewertungsTyp.Integer;
    }
}
