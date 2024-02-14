using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace aspmvc.Controllers
{
    class Kandinsky : Controller
    {
        private const string ApiUrl = "https://api-key.fusionbrain.ai/";
        private const string ApiKey = "C95419CEC0FD4EA4E00D68E41F29F1B8";
        private const string SecretKey = "3F2AC94569F35190ED87B432544A79FB";

        private static int fileIndex;
        private static string? filePath;

        private readonly HttpClient client;

        public Kandinsky()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Key", $"Key {ApiKey}");
            client.DefaultRequestHeaders.Add("X-Secret", $"Secret {SecretKey}");
        }

        private async Task<int> GetModelIdAsync()
        {
            var response = await client.GetAsync($"{ApiUrl}key/api/v1/models");
            response.EnsureSuccessStatusCode();

            var modelData = await response.Content.ReadFromJsonAsync<ModelData[]>();
            return modelData[0].Id;
        }

        private async Task<byte[]> GenerateImageAsync(string prompt, int modelId, int width = 1024, int height = 1024, int numImages = 1)
        {
            var uuid = await GenerateImageUuidAsync(prompt, modelId, width, height, numImages);
            if (uuid == null)
            {
                Console.WriteLine("Generation failed or timed out.");
                return null;
            }

            return await GetImageAsync(uuid);
        }

        private async Task<string> GenerateImageUuidAsync(string prompt, int modelId, int width = 1024, int height = 1024, int numImages = 1)
        {
            var generateParams = new
            {
                type = "GENERATE",
                numImages,
                width,
                height,
                generateParams = new
                {
                    query = prompt
                }
            };

            var requestData = new
            {
                model_id = modelId,
                @params = generateParams
            };

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(modelId.ToString()), "model_id");
            content.Add(new StringContent(JsonConvert.SerializeObject(generateParams), Encoding.UTF8, "application/json"), "params");

            var response = await client.PostAsync($"{ApiUrl}key/api/v1/text2image/run", content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<GenerateResponse>();
            return responseData.Uuid;
        }

        private async Task<byte[]> GetImageAsync(string uuid)
        {
            var attempts = 10;
            var delayInSeconds = 10;

            while (attempts > 0)
            {
                var response = await client.GetAsync($"{ApiUrl}key/api/v1/text2image/status/{uuid}");
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<StatusResponse>();
                if (data.Status == "DONE")
                {
                    return Convert.FromBase64String(data.Images[0]);
                }

                attempts--;
                await Task.Delay(delayInSeconds * 2000);
            }

            Console.WriteLine("Image retrieval failed or timed out.");
            return null;
        }

        public static async Task GetGenerateImage(string promt)
        {
            var kandinsky = new Kandinsky();

            var modelId = await kandinsky.GetModelIdAsync();
            var imageBytes = await kandinsky.GenerateImageAsync(promt, modelId);

            if (imageBytes != null)
            {
                fileIndex++;
                filePath = $"C:\\Users\\arter\\Desktop\\image\\{promt}.png";
                System.IO.File.WriteAllBytes(filePath, imageBytes);
                Console.WriteLine($"Image ({promt}) saved to {filePath}");

            }
        }

        public static string GetFilePath()
        {
            return filePath;
        }

        static int GetFileIndex(string folderPath)
        {
            int fileCount = Directory.GetFiles(folderPath, "*.png").Length;
            return fileCount;
        }
    }


    class ModelData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Version { get; set; }
        public string Type { get; set; } = "";
    }

    class GenerateResponse
    {
        public string Uuid { get; set; } = "";
        public string Status { get; set; } = "";
    }

    class StatusResponse
    {
        public string Uuid { get; set; } = "";
        public string Status { get; set; } = "";
        public string[] Images { get; set; } = { };
        public string ErrorDescription { get; set; } = "";
        public bool Censored { get; set; }
    }
}
