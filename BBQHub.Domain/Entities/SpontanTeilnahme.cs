using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class SpontanTeilnahme
    {
        public int Id { get; set; }
        public int Token { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Telefonnummer { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Adresse
        public string Street { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "CH";

        public int DurchgangId { get; set; }
        public DateTime Anmeldezeit { get; set; }

        public Durchgang? Durchgang { get; set; }
    }
}
