using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceHost.Abstractions
{
    public interface IServiceHostBuilder
    {
        IConfigurationBuilder Configuration { get; }

        IServiceCollection Dependencies { get; }

        void AddService<TService>() where TService : IService;

        void AddService(Type serviceType);

        IServiceHost Build();
    }
}
