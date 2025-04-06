using BBQHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Events
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }

        public string ManagerName { get; set; } = string.Empty; // UserName oder FullName
        public EventStatus Status { get; set; }

        // Optional für die View
        public string? Description { get; set; }
        public string? Location => $"{Street}, {ZipCode} {City}";

        public string Street { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string City { get; set; } = "";
    }

}
