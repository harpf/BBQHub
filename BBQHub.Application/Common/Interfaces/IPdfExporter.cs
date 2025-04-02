using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Common.Interfaces
{
    public interface IPdfExporter
    {
        Task<byte[]> ExportRanglisteAsync(int eventId);
    }
}
