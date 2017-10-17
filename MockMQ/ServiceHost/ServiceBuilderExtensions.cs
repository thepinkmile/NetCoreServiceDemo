using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ServiceHost.Abstractions;

namespace ServiceHost
{
    public static class ServiceBuilderExtensions
    {
        #region ConfigurationMethods

        public static IServiceHostBuilder AddJsonConfiguration(this IServiceHostBuilder builder
            , string path
            , bool optional = false
            , bool reloadOnChange = false)
        {
            builder.Configuration.AddJsonFile(path, optional, reloadOnChange);
            return builder;
        }

        public static IServiceHostBuilder AddIniConfiguration(this IServiceHostBuilder builder
            , string path
            , bool optional = false
            , bool reloadOnChange = false)
        {
            builder.Configuration.AddIniFile(path, optional, reloadOnChange);
            return builder;
        }

        public static IServiceHostBuilder AddXmlConfiguration(this IServiceHostBuilder builder
            , string path
            , bool optional = false
            , bool reloadOnChange = false)
        {
            builder.Configuration.AddXmlFile(path, optional, reloadOnChange);
            return builder;
        }

        public static IServiceHostBuilder AddUserSecrets(this IServiceHostBuilder builder, string userSecretsId)
        {
            builder.Configuration.AddUserSecrets(userSecretsId);
            return builder;
        }

        public static IServiceHostBuilder AddUserSecrets(this IServiceHostBuilder builder, bool optional = true)
        {
            return builder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional);
        }

        public static IServiceHostBuilder AddUserSecrets(this IServiceHostBuilder builder, Assembly assembly, bool optional = true)
        {
            builder.Configuration.AddUserSecrets(assembly, optional);
            return builder;
        }

        public static IServiceHostBuilder AddEnvironmentConfiguration(this IServiceHostBuilder builder)
        {
            builder.Configuration.AddEnvironmentVariables("ASPNETCORE_SERVICES_");
            return builder;
        }

        public static IServiceHostBuilder AddCommandLine(this IServiceHostBuilder builder, string[] args)
        {
            builder.Configuration.AddCommandLine(args);
            return builder;
        }

        public static IServiceHostBuilder AddCommandLine(this IServiceHostBuilder builder, string[] args, IDictionary<string,string> switchMappings)
        {
            builder.Configuration.AddCommandLine(args, switchMappings);
            return builder;
        }

        public static IServiceHostBuilder AddCommandLine(this IServiceHostBuilder builder, Action<CommandLineConfigurationSource> configurationSource)
        {
            builder.Configuration.AddCommandLine(configurationSource);
            return builder;
        }

        #endregion

        #region DIMethods

        public static IServiceHostBuilder AddSingleton<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.AddSingleton<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder AddSingleton<TBase>(this IServiceHostBuilder builder, TBase singleton)
            where TBase : class
        {
            builder.Dependencies.AddSingleton(singleton);
            return builder;
        }

        public static IServiceHostBuilder AddScoped<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.AddScoped<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder AddScoped<TBase>(this IServiceHostBuilder builder, Func<IServiceProvider, TBase> scoped)
            where TBase : class
        {
            builder.Dependencies.AddScoped(scoped);
            return builder;
        }

        public static IServiceHostBuilder AddTransient<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.AddTransient<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder AddTransient<TBase>(this IServiceHostBuilder builder, Func<IServiceProvider, TBase> scoped)
            where TBase : class
        {
            builder.Dependencies.AddTransient(scoped);
            return builder;
        }
        
        public static IServiceHostBuilder TryAddSingleton<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.TryAddSingleton<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder TryAddSingleton<TBase>(this IServiceHostBuilder builder, TBase singleton)
            where TBase : class
        {
            builder.Dependencies.TryAddSingleton(singleton);
            return builder;
        }

        public static IServiceHostBuilder TryAddScoped<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.TryAddScoped<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder TryAddScoped<TBase>(this IServiceHostBuilder builder, Func<IServiceProvider, TBase> scoped)
            where TBase : class
        {
            builder.Dependencies.TryAddScoped(scoped);
            return builder;
        }

        public static IServiceHostBuilder TryAddTransient<TBase, TConcrete>(this IServiceHostBuilder builder)
            where TBase : class
            where TConcrete : class, TBase
        {
            builder.Dependencies.TryAddTransient<TBase, TConcrete>();
            return builder;
        }

        public static IServiceHostBuilder TryAddTransient<TBase>(this IServiceHostBuilder builder, Func<IServiceProvider, TBase> scoped)
            where TBase : class
        {
            builder.Dependencies.TryAddTransient(scoped);
            return builder;
        }

        public static IServiceHostBuilder Configure<T>(this IServiceHostBuilder builder, string configPath)
            where T : class, new()
        {
            builder.Dependencies.Configure<T>(builder.Configuration.Build().GetSection(configPath));
            return builder;
        }

        public static IServiceHostBuilder Configure<T>(this IServiceHostBuilder builder, IConfiguration configuration)
            where T : class, new()
        {
            builder.Dependencies.Configure<T>(configuration);
            return builder;
        }

        public static IServiceHostBuilder Configure<T>(this IServiceHostBuilder builder, Action<T> action)
            where T : class, new()
        {
            builder.Dependencies.Configure(action);
            return builder;
        }

        #endregion

        #region ServiceMethods

        public static IServiceHostBuilder IncludeService<TService>(this IServiceHostBuilder builder)
            where TService : IService
        {
            builder.AddService<TService>();
            return builder;
        }

        #endregion

        #region Logging Methods

        public static IServiceHostBuilder AddLogging(this IServiceHostBuilder builder, string configPath, Action<ILoggingBuilder> optionsAction = null)
        {
            builder.Dependencies.AddLogging(options =>
            {
                options.AddConfiguration(builder.Configuration.Build().GetSection(configPath));
                optionsAction?.Invoke(options);
            });
            return builder;
        }

        public static IServiceHostBuilder AddLogging(this IServiceHostBuilder builder, Action<ILoggingBuilder> optionsAction)
        {
            builder.Dependencies.AddLogging(optionsAction);
            return builder;
        }

        #endregion
    }
}
