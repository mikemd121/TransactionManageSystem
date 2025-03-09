using AccountManagementSystem.Infrastructure;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using AccountManagementSystem;


class Program
{
    static void Main()
    {
        try
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<DependencyModule>();
            var container = builder.Build();
            var serviceProvider = new AutofacServiceProvider(container);
            var accountManagementService = serviceProvider.GetRequiredService<AccountManagementService>();
            accountManagementService.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

    }
}