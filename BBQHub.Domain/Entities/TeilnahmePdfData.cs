using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class TeilnahmePdfData
    {
        public string Vorname { get; set; } = string.Empty;
        public string Nachname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefonnummer { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Ort { get; set; } = string.Empty;
        public string PLZ { get; set; } = string.Empty;
        public string Land { get; set; } = string.Empty;
        public string DatenschutzerklaerungUnterschrift { get; set; } = string.Empty; // base64 PNG
        public DateTime DatenschutzerklaerungDatum { get; set; }
        public string TeilnahmeUnterschrift { get; set; } = string.Empty; // base64 PNG
        public DateTime TeilnahmeDatum { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventDatum { get; set; } = string.Empty;
        public string EventMenue { get; set; } = string.Empty;
        public string EventStandort { get; set; } = string.Empty;
        public string? LogoVeranstalter { get; set; }
        public string? LogoHauptsponsor { get; set; }
        public string? LogoNebensponsor { get; set; }
    }

}