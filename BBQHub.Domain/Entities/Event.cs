using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public enum EventType
    {
        GeplantMitTeams = 0,
        SpontanTeilnahme = 1
    }
    public enum EventStatus
    {
        Inaktiv,
        Aktiv,
        Abgeschlossen
    }


    public class Event
    {
        public int Id { get; set; }
        public Guid EventId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Sponsors { get; set; }
        public string? LogoPath { get; set; }
        public string ManagerId { get; set; } = string.Empty;
        public EventType Typ { get; set; } = EventType.GeplantMitTeams;
        public EventStatus Status { get; set; } = EventStatus.Inaktiv;

        public List<Team> Teams { get; set; } = new();
        public List<Durchgang> Durchgaenge { get; set; } = new();
        public List<EventLogo> Logos { get; set; } = new();

    }
}
