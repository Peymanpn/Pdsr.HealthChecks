using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pdsr.HealthChecks.NpgSql;

public class NpgSqlHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _sql;
    private readonly Action<NpgsqlConnection> _connectionAction;
    public NpgSqlHealthCheck(string npgsqlConnectionString, string sql, Action<NpgsqlConnection> connectionAction = null)
    {
        _connectionString = npgsqlConnectionString ?? throw new ArgumentNullException(nameof(npgsqlConnectionString));
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        _connectionAction = connectionAction;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            _connectionAction?.Invoke(connection);

            await connection.OpenAsync(cancellationToken);

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = _sql;
                await command.ExecuteScalarAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}

public static class NpgSqlHealthCheckBuilderExtensions
{
    const string NAME = "npgsql";

    /// <summary>
    /// Add a health check for Postgres databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="npgsqlConnectionString">The Postgres connection string to be used.</param>
    /// /// <param name="healthQuery">The query to be used in check. Optional. If <c>null</c> SELECT 1 is used.</param>
    /// <param name="connectionAction">An optional action to allow additional Npgsql-specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'npgsql' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
    public static IHealthChecksBuilder AddNpgSql(this IHealthChecksBuilder builder, string npgsqlConnectionString, string healthQuery = "SELECT 1;", Action<NpgsqlConnection> connectionAction = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new NpgSqlHealthCheck(npgsqlConnectionString, healthQuery, connectionAction),
            failureStatus,
            tags,
            timeout));
    }
}
