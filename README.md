# Summary

!! DO NOT USE IN PRODUCTION. THIS CODE IS EXPERIMENTAL !!

This package provides various experimental Microsoft Orleans providers backed by Entity Framework Core.

Using Entity Framework Core as the backing store for various Microsoft Orleans providers allows us to utilize any supported EF provider to back Orleans functionality (theoretically).

# Compatability

Should be compatable with Orleans 2.x

# Installation

Install from https://www.nuget.org/packages/Orleans.EntityFrameworkCore/

# Supported Orleans Providers

* IMembershipTable (used for orleans silo clustering rendezvous)
* IGatewayListProvider (used for client clustering rendezvous)
* IStorageProvider (used for grain state storage for any named provider)
* IReminderTable (used for grain reminders)

# Entity Framework Database Providers

Theoretically all entity framework core database providers listed [here](https://docs.microsoft.com/en-us/ef/core/providers/) should work.

This project explicitly tests the following providers and major database versions:

* `Microsoft.EntityFrameworkCore.InMemory`
* `Microsoft.EntityFrameworkCore.Sqlite`
* `Microsoft.EntityFrameworkCore.SqlServer`
    * SqlServer 2017 CU12
* `MySql.Data.EntityFrameworkCore`
    * MariaDB v05.5.63
    * MariaDB v10.4.2
    * Mysql v5.7.25
    * Mysql v8.0.15
* `Npgsql.EntityFrameworkCore.PostgreSQL`
    * CockroachDB v2.1.4
    * Postgres v9.6.11
    * Postgres v10.6.0
    * Postgres v11.1.0
* `Pomelo.EntityFrameworkCore.MySql`
    * MariaDB v05.5.63
    * MariaDB v10.4.2
    * Mysql v5.7.25
    * Mysql v8.0.15

See the `src/Orleans.EntityFrameworkCore.Tests` project for more details. Database versions are based on latest available docker images from Docker Hub and go back 3 major versions when availble.

If you have used `Orleans.EntityFrameworkCore` successfully with a provider not listed here, please open an issue and report your experience!

# Configuration Sample

Silo

```cs
new SiloHostBuilder()
    .AddEntityFrameworkStorage() // "Default" orleans storage provider
    .AddEntityFrameworkStorage("PubSub") // add storage for PubSub (streams)
    .AddEntityFrameworkStorage("MyCustomProvider") // your own custom storage provider name
    .Configure<ClusterOptions>(options =>
    {
        // make sure to configure cluster information
        options.ClusterId = <cluster_id>;
        options.ServiceId = <service_id>;
    })
    .ConfigureServices(services =>
    {
        // add the OrleansEFContext to the silo services.
        // be sure to configure this context with any EF provider
        // specific options (pgsql, sql server, in-memory etc)
        services.AddDbContext<OrleansEFContext>();
    })
    .UseEntityFrameworkClustering() // silo clustering
    .UseEntityFrameworkReminders() // silo reminders
    .Build();
```

Client

```cs
new ClientBuilder()
    .ConfigureServices(services =>
    {
        // add the OrleansEFContext to the silo services.
        // be sure to configure this context with any EF provider
        // specific options (pgsql, sql server, in-memory etc)
        services.AddDbContext<OrleansEFContext>();
    })
    .UseEntityFrameworkClustering() // client clustering
    .Build();
```

# Migrations

This project provides EF Core migrations for EF providers that support migrations.

You can invoke migrations by obtaining a instance of the ```OrleansEFContext``` and calling `AutoMigrate`

```cs
using (var scope = silo.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService(typeof(OrleansEFContext)) as OrleansEFContext;
    await context.AutoMigrate();
}
```

# License

This project is licensed under the terms of the [MIT license](LICENSE.md)