using System;
using Microsoft.Extensions.DependencyInjection;

namespace CSGOCheatDetector
{
    public static class ServiceLocator
    {
        private static readonly Lazy<IServiceProvider> _lazyProvider = new Lazy<IServiceProvider>(() =>
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        });

        public static IServiceProvider ServiceProvider => _lazyProvider.Value;

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ViewModels.FileCheckViewModel>();
            // Добавьте другие ViewModel и службы здесь
        }
    }
}
