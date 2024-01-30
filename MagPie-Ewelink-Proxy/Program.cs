using MagPie_Ewelink_Proxy.Services;

namespace MagPie_Ewelink_Proxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<EwelinkService>();

            var app = builder.Build();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}