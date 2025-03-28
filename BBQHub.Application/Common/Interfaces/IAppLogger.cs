using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Common.Interfaces
{
    public interface IAppLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? exception = null);
    }
}
