using System;
using System.Collections.Generic;
using System.Text;

namespace AberFitnessAuditLogger
{
    class LogEntry
    {
        public virtual string Content { get; set; }

        public virtual string ServiceName { get; set; }

        public virtual string UserId { get; set; }

        public virtual string Timestamp { get; set; }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp}, UserId: {UserId}, ServiceName: {ServiceName}, Content: {Content}";
        }
    }
}
