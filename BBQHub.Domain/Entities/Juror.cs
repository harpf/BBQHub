﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Domain.Entities
{
    public class Juror
    {
        public int Id { get; set; }
        public int JuryId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Vereinslocation { get; set; } = string.Empty;
    }
}
