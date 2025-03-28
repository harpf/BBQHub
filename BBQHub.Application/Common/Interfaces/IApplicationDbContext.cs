using BBQHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace BBQHub.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Juror> Juroren { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
