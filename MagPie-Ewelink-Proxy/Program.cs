namespace MagPie_Ewelink_Proxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            ServiceHelper.RegisterServices(builder.Services);

            var app = builder.Build();

            ServiceHelper.ServiceProvider = app.Services;

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}