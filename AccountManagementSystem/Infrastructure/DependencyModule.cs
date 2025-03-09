using AccountManagementSystem.Application.Interfaces;
using Autofac;
using Autofac.Core;


namespace AccountManagementSystem.Infrastructure
{
   public class DependencyModule :Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register services with interfaces
            builder.RegisterType<TransactionService>().SingleInstance();
            builder.RegisterType<ConsoleInputOutputHandler>().As<IInputOutputHandler>().SingleInstance();
            builder.RegisterType<TransactionService>().As<IHandler>().SingleInstance();
            builder.RegisterType<RuleService>().As<IHandler>().SingleInstance();
            builder.RegisterType<PrintAccountService>().As<IHandler>().SingleInstance();
            builder.RegisterType<AccountManagementService>().SingleInstance();
        }
    }
}
