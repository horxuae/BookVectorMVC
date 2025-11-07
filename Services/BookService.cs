using BookVectorMVC.Data;
using BookVectorMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookVectorMVC.Services;

/// <summary>
/// 書籍服務實作
/// </summary>
public class BookService : IBookService
{
    private readonly BookDbContext _context;
    private readonly IApiService _apiService;
    private readonly ILogger<BookService> _logger;

    public BookService(BookDbContext context, IApiService apiService, ILogger<BookService> logger)
    {
        _context = context;
        _apiService = apiService;
        _logger = logger;
    }

    /// <summary>
    /// 獲取所有書籍
    /// </summary>
    public async Task<List<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .OrderBy(b => b.BookId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根據 ID 獲取書籍
    /// </summary>
    public async Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Books.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// 新增書籍並自動生成向量表示
    /// </summary>
    public async Task<Book> AddBookAsync(string title, string? description, string? position, CancellationToken cancellationToken = default)
    {
        var combinedText = $"{title} {description}";
        var vector = await _apiService.GetEmbeddingAsync(combinedText, cancellationToken);
        var vectorJson = SerializeVector(vector);

        var book = new Book
        {
            Title = title,
            Description = description,
            Position = position,
            Vector = vectorJson
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added new book: {Title} (ID: {BookId})", title, book.BookId);
        return book;
    }

    /// <summary>
    /// 更新書籍
    /// </summary>
    public async Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        // 重新生成向量
        var combinedText = $"{book.Title} {book.Description}";
        var vector = await _apiService.GetEmbeddingAsync(combinedText, cancellationToken);
        book.Vector = SerializeVector(vector);

        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated book: {Title} (ID: {BookId})", book.Title, book.BookId);
        return book;
    }

    /// <summary>
    /// 刪除書籍
    /// </summary>
    public async Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FindAsync(new object[] { id }, cancellationToken);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted book: {Title} (ID: {BookId})", book.Title, book.BookId);
        return true;
    }

    /// <summary>
    /// 基於向量相似度的智能搜尋
    /// </summary>
    public async Task<List<SearchResult>> SearchByVectorAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var queryVector = await _apiService.GetEmbeddingAsync(query, cancellationToken);
        if (queryVector.Length == 0)
        {
            _logger.LogWarning("Failed to generate vector for query: {Query}", query);
            return new List<SearchResult>();
        }

        var books = await GetAllBooksAsync(cancellationToken);
        var results = new List<SearchResult>();

        foreach (var book in books)
        {
            var bookVector = DeserializeVector(book.Vector);
            var score = CalculateCosineSimilarity(queryVector, bookVector);
            results.Add(new SearchResult { Book = book, Score = score });
        }

        return results
            .OrderByDescending(x => x.Score)
            .Select((x, i) => { x.Rank = i + 1; return x; })
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// 根據位置搜尋書籍
    /// </summary>
    public async Task<List<SearchResult>> SearchByPositionAsync(string position, CancellationToken cancellationToken = default)
    {
        var books = await _context.Books
            .Where(b => b.Position == position)
            .ToListAsync(cancellationToken);

        return books.Select((book, index) => new SearchResult
        {
            Book = book,
            Score = 1.0,
            Rank = index + 1
        }).ToList();
    }

    /// <summary>
    /// 根據書名搜尋書籍
    /// </summary>
    public async Task<List<SearchResult>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var books = await _context.Books
            .Where(b => b.Title.Contains(title))
            .ToListAsync(cancellationToken);

        return books.Select((book, index) => new SearchResult
        {
            Book = book,
            Score = 1.0,
            Rank = index + 1
        }).ToList();
    }

    /// <summary>
    /// 更新所有書籍的向量
    /// </summary>
    public async Task<int> UpdateAllVectorsAsync(CancellationToken cancellationToken = default)
    {
        var books = await GetAllBooksAsync(cancellationToken);
        var updatedCount = 0;

        foreach (var book in books)
        {
            var combinedText = $"{book.Title} {book.Description}";
            var vector = await _apiService.GetEmbeddingAsync(combinedText, cancellationToken);
            book.Vector = SerializeVector(vector);
            updatedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated vectors for {Count} books", updatedCount);
        return updatedCount;
    }

    /// <summary>
    /// 將向量陣列序列化為 JSON 字符串
    /// </summary>
    public string SerializeVector(float[] vector)
    {
        return JsonSerializer.Serialize(vector);
    }

    /// <summary>
    /// 將 JSON 字符串反序列化為向量陣列
    /// </summary>
    public float[] DeserializeVector(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<float>();
        }

        try
        {
            return JsonSerializer.Deserialize<float[]>(json) ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing vector: {Json}", json[..Math.Min(json.Length, 100)]);
            return Array.Empty<float>();
        }
    }

    /// <summary>
    /// 計算兩個向量的餘弦相似度
    /// </summary>
    private static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length == 0 || vectorB.Length == 0)
        {
            return 0.0;
        }

        var minLength = Math.Min(vectorA.Length, vectorB.Length);
        double dotProduct = 0, normA = 0, normB = 0;

        for (int i = 0; i < minLength; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator > 0 ? dotProduct / denominator : 0.0;
    }
}