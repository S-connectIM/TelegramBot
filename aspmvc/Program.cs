using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace aspmvc
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddControllersWithViews();
            var app = builder.Build();
            Console.WriteLine(4);
            Utilities.EnsureDirectoriesExist();
            Console.WriteLine(5);

            string url = "https://3ae7-178-75-82-202.ngrok-free.app";
            string apiUrl = $"https://api.telegram.org/bot" + "6791814675:AAFA9__aGx879v1y-1FtnZNr1LHgRU1i7sc" + "/setWebhook" + $"?url={url}";

            var message = new
            {
                url = apiUrl,
                drop_pending_updates = true,
            };

            HttpClient client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            client.PostAsync(apiUrl, content);

            app.MapControllerRoute(name: "default", pattern: "{controller=Bot}/{action=Index}/{ad?}");
            app.Run();
        }
    }
}
