using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AberFitnessAuditLogger
{
    public interface IAuditLogger
    {
        Task log(string userId, string content);
    }
}
