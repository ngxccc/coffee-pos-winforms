using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CoffeePOS.Shared.Helpers;

public static class ImgbbHelper
{
    private static readonly HttpClient HttpClient = new();

    private static string? _apiKey;

    public static void Initialize(IConfiguration config)
    {
        _apiKey = config["ThirdPartyServices:ImgBB_ApiKey"];
    }

    public static async Task<string> UploadImageAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Hệ thống chưa được nạp ImgBB API Key từ cấu hình!");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Không tìm thấy file ảnh local!");

        using var form = new MultipartFormDataContent();

        using var fileStream = File.OpenRead(filePath);
        using var streamContent = new StreamContent(fileStream);

        form.Add(streamContent, "image", Path.GetFileName(filePath));

        string url = $"https://api.imgbb.com/1/upload?key={_apiKey}";
        var response = await HttpClient.PostAsync(url, form);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"ImgBB API xịt. Status: {response.StatusCode}. Chi tiết: {errorBody}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);

        return jsonDoc.RootElement.GetProperty("data").GetProperty("url").GetString()
               ?? throw new InvalidOperationException("Không parse được URL từ ImgBB.");
    }
}
