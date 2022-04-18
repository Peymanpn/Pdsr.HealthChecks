using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pdsr.HealthChecks
{
    public abstract class SystemHealthBase : ISystemHealth
    {
        public virtual bool OverAll { get; set; }
        public virtual int MemoryUsedMb { get; set; }
        public HealthReport HealthReport { get; set; }
    }
}
