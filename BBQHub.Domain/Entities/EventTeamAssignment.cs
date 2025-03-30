using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class EventTeamAssignment
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int TeamId { get; set; }

        public int Token { get; set; }

        public Event Event { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }

}