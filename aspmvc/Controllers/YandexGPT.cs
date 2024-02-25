using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace aspmvc.Controllers
{
    class YandexGPT : Controller
    {
        public static async Task<string> SendCompletionRequest(string question)
        {
            var prompt = new
            {
                modelUri = "gpt://b1gt4nscm4mtcv853jvr/yandexgpt-lite",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.6,
                    maxTokens = "2000"
                },
                messages = new[]
                {
                new { role = "user", text = $"{question}" },
            }
            };

            var url = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";
            var apiKey = "AQVNzo4gWyPFQj-iipUvvndfCShl0uBbmUVpLxRv";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", apiKey);

                var content = new StringContent(JsonConvert.SerializeObject(prompt), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var resultDocument = JsonDocument.Parse(resultJson);
                    var textProperty = resultDocument.RootElement.GetProperty("result")
                        .GetProperty("alternatives")[0]
                        .GetProperty("message")
                        .GetProperty("text");

                    return textProperty.GetString();
                }
                else
                {
                    if (response.StatusCode.ToString() == "TooManyRequests")
                        return "Много одновременных запросов";
                    else
                        return $"Error: {response.StatusCode}";
                }
            }

            //SetView();
        }

        [ActionName("Index")]
        public IActionResult SetView()
        {
            return View();
        }
    }
}