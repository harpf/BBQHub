using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public enum LogoType
    {
        Veranstalter,
        Hauptsponsor,
        Nebensponsor
    }

    public class EventLogo
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public LogoType Type { get; set; }

        public Event Event { get; set; } = null!;
        public bool IsActive { get; set; } = false;
    }
}
