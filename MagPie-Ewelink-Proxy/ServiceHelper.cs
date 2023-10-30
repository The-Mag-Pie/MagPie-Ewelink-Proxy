using MagPie_Ewelink_Proxy.Services;

namespace MagPie_Ewelink_Proxy
{
    public static class ServiceHelper
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider { set { _serviceProvider = value; } }

        public static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<EwelinkService>();
        }
    }
}
