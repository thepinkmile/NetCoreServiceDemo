using System;
using System.Threading.Tasks;

namespace ServiceHost.Abstractions
{
    public interface IServiceHost
    {
        IServiceProvider Dependencies { get; }

        void AddTaskService(Type serviceType);

        void Run();

        void Start();

        void Stop();
    }
}
