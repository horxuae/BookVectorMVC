using System.Text;
using System.Text.Json;

namespace BookVectorMVC.Services;

/// <summary>
/// API 服務 - 處理 Jina AI 向量化服務
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;

    public int VectorDimension { get; }

    public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _apiKey = _configuration["JinaAI:ApiKey"] ?? throw new InvalidOperationException("JinaAI:ApiKey is not configured");
        _baseUrl = _configuration["JinaAI:BaseUrl"] ?? "https://api.jina.ai/v1/embeddings";
        _model = _configuration["JinaAI:Model"] ?? "jina-embeddings-v3";
        VectorDimension = _configuration.GetValue<int>("JinaAI:VectorDimension", 1024);

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <summary>
    /// 取得文字的語意向量（使用 Jina Embeddings v3）
    /// </summary>
    /// <param name="text">輸入文字</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>浮點向量陣列</returns>
    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Empty text provided for embedding");
            return Array.Empty<float>();
        }

        try
        {
            var requestBody = new
            {
                model = _model,
                task = "text-matching",
                input = new[] { text }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Requesting embedding for text: {Text}", text[..Math.Min(text.Length, 50)]);

            var response = await _httpClient.PostAsync(_baseUrl, content, cancellationToken);
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Jina API Error: {StatusCode} {Response}", response.StatusCode, jsonResponse);
                return Array.Empty<float>();
            }

            using var document = JsonDocument.Parse(jsonResponse);
            var embeddingArray = document.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            _logger.LogDebug("Successfully generated {Dimensions}D embedding", embeddingArray.Length);
            return embeddingArray;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text[..Math.Min(text.Length, 50)]);
            return Array.Empty<float>();
        }
    }
}