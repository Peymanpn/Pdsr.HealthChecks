using Pdsr.HealthChecks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pdsr.HealthChecks
{
    public class ReadinessPublisher<TSysHealth> : IHealthCheckPublisher
        where TSysHealth : ISystemHealth
    {
        private readonly ILogger _logger;

        public ReadinessPublisher(
            TSysHealth sysHealth,
            ILoggerFactory logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger.CreateLogger<ReadinessPublisher<TSysHealth>>();
            SysHealth = sysHealth;
            ServiceProvider = serviceProvider;
        }

        public TSysHealth SysHealth { get; }
        public IServiceProvider ServiceProvider { get; }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {

            SysHealth.HealthReport = report;
            SysHealth.OverAll = report.Status == HealthStatus.Healthy;



            if (report.Entries.ContainsKey(MemoryHealthCheck.Name))
                SysHealth.MemoryUsedMb = report.GetHealtReportEntry(MemoryHealthCheck.Name)
                    .GetHealthReportData<int>("allocated_mb");

            if (report.Status == HealthStatus.Healthy)
            {
                _logger.LogInformation("Readiness Probe Status: {Result}", report.Status);
            }
            else
            {
                _logger.LogError("Readiness Probe Status: {Result}", report.Status);
            }

            return Task.CompletedTask;
        }
    }

    public static class ReadinessPublisherExtensions
    {
        public static void RegisterReadinessPublisher<TSysHealth>(this IServiceCollection services)
            where TSysHealth : class, ISystemHealth
        {
            services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher<TSysHealth>>();
            services.AddSingleton<TSysHealth>();
        }
    }
}
