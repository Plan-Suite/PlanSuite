using System;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class LogViewModel : BaseViewModel
    {
        public Project Project { get; set; }
        public List<AuditLog> AuditLogs { get; set; }
    }
}

