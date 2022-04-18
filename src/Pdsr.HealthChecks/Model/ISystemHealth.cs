using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pdsr.HealthChecks
{
    public interface ISystemHealth
    {
        HealthReport HealthReport { get; set; }
        int MemoryUsedMb { get; set; }
        bool OverAll { get; set; }
    }
}