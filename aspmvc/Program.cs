using Newtonsoft.Json;
using System.Text;

namespace aspmvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews().AddNewtonsoftJson();

            var app = builder.Build();

            Utilities.EnsureDirectoriesExist();
            SetWebhook();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "yandex",
                pattern: "yandex",
                defaults: new { controller = "YandexGPT", action = "Index" });

            app.MapControllerRoute(
                name: "kandinsky",
                pattern: "kandinsky",
                defaults: new { controller = "Kandinsky", action = "Index" });

            app.MapControllerRoute(
                name: "bot",
                pattern: "bot",
                defaults: new { controller = "BotController", action = "Index" });

            app.Run();
        }

        public static void SetWebhook()
        {
            string url = "";
            string apiUrl = $"https://api.telegram.org/bot" + "6791814675:AAFA9__aGx879v1y-1FtnZNr1LHgRU1i7sc" + "/setWebhook" + $"?url={url}";

            var message = new
            {
                url = apiUrl,
                drop_pending_updates = true,
            };

            HttpClient client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            client.PostAsync(apiUrl, content);
        }
    }
}
