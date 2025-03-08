using AccountManagementSystem.Infrastructure;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using AccountManagementSystem;


class Program
{
    static void Main()
    {
        var builder = new ContainerBuilder();

        // Register dependencies
        builder.RegisterModule<DependencyModule>();

        // Build the container
        var container = builder.Build();

        // Create Autofac service provider
        var serviceProvider = new AutofacServiceProvider(container);

        // Resolve the main service and run the application
        var accountManagementService = serviceProvider.GetRequiredService<AccountManagementService>();
        accountManagementService.Run();
    }
}