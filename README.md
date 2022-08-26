# Pdsr HealthChecks Libraries

Contains HealthChecks for OpenId-Connect, RedisCacheManager, Postgres, and RabbitMQ.

## Getting Started

Health checks need to register in the DI container.

### Redis Healthcheck

In the minimal API, use `builder.Services` to add the extension method, `AddRedisCacheManager` to `IHealthChecksBuilder`.

```csharp
var degradedThreshold = 50;     // in milliseconds, response time higher than 50 will be considered degraded
var unhealthyThreshold = 100;   // in milliseconds, response time higher than 100 will be considered unhealthy
builder.Services.AddHealthChecks()
    .AddRedisCacheManager(degradedThreshold,unhealthyThreshold);
```

### Memory HealthCheck

You can define a memory usage limit for the `MemoryHealthCheck`

In the minimal API, use `builder.Services` to add the extension method, `AddMemoryHealthCheck` to `IHealthChecksBuilder`

```csharp
builder.Services.AddHealthChecks()
    .AddMemoryHealthCheck(
        degradedThreshold: 50 * 1024L * 1024L,      // shows degraded at this 50 MB
        unhealthyThreshold: 100 * 2048L * 2048L,    // threshold to show failureStatus (unhealthy), 100MB
        failureStatus: HealthStatus.Unhealthy,      // default failure status is HealthStatus.Unhealthy
        tags: new string[] { "memory", "ram" });    // optional tags
```

## Health Publisher

The interface `ISystemHealth` and the abstract class `SystemHealthBase` are available to be used as publisher.

you need to inherit from `SystemHealthBase`:

```csharp
public class MyHealthStatus : SystemHealthBase
{
    // some custom properties
}

```

Register in DI container

```csharp
builder.Services.RegisterReadinessPublisher<MyHealthStatus>();
```

The instance of `MyHealthStatus` is available through injection.

```csharp
public class SomeService : ISomeService
{
    private readonly MySystemHealth _mySystemHealth;
    public SomeService(MySystemHealth mySystemHealth)
    {
        _mySystemHealth = _mySystemHealth;
    }

    public void SomeMethod()
    {
        var health = _mySystemHealth.HealthReport;
        // do whatever you like with that health status.
    }
}
```

## Samples

Refer to the [samples](samples) folder.
