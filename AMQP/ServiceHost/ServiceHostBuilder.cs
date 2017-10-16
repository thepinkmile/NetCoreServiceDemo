using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceHost.Abstractions;

namespace ServiceHost
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        #region Constructors

        public ServiceHostBuilder()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            Dependencies = new ServiceCollection();
            _serviceTypes = new List<Type>();
        }

        #endregion

        #region Properties

        public IConfigurationBuilder Configuration { get; }

        public IServiceCollection Dependencies { get; }

        #endregion

        #region Methods

        public void AddService<TService>() where TService : IService
        {
            AddService(typeof(TService));
        }

        public void AddService(Type serviceType)
        {
            if (serviceType.IsInterface)
                throw new ArgumentException("Service Type is not a vaild IService.");
            _serviceTypes.Append(serviceType);
        }

        public IServiceHost Build()
        {
            var config = Configuration.Build();
            Dependencies.TryAddSingleton(config);
            Dependencies.TryAddSingleton<IConfiguration>(config);

            var host = new ServiceHost(Dependencies.BuildServiceProvider());
            foreach (var serviceType in _serviceTypes)
            {
                host.AddTaskService(serviceType);
            }
            return host;
        }

        #endregion

        #region Fields

        private readonly IEnumerable<Type> _serviceTypes;

        #endregion
    }
}
