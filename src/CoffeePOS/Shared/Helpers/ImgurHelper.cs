using System.Text.Json;

namespace CoffeePOS.Shared.Helpers;

public static class ImgurHelper
{
    private static readonly HttpClient HttpClient = new();

    // HACK: Hardcoded anonymous Client-ID for Imgur API. Replace with your own from api.imgur.com if rate-limited.
    private const string ClientId = "Client-ID 1234567890abcdef";

    // PERF: Space Complexity O(N) where N is the image byte array size. Time Complexity is heavily Network I/O bound.
    public static async Task<string> UploadImageAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Không tìm thấy file ảnh local!");

        var imageData = await File.ReadAllBytesAsync(filePath);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image");
        request.Headers.Add("Authorization", ClientId);

        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(imageData), "image", Path.GetFileName(filePath) }
        };
        request.Content = content;

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);

        return jsonDoc.RootElement.GetProperty("data").GetProperty("link").GetString()
               ?? throw new Exception("Không parse được URL từ Imgur.");
    }
}
