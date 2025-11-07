# .NET Framework 至 .NET Core 8 遷移指南

## 📋 遷移概述

從 **ASP.NET MVC 5 (.NET Framework 4.7.2)** 升級至 **ASP.NET Core 8**，以下是詳細的遷移變更和改進。

## 🤔 ASP.NET MVC vs ASP.NET Core MVC - 真的差不多嗎？

### 表面相似性的迷思

很多開發者會認為 ASP.NET MVC 和 ASP.NET Core MVC "差不多"，因為它們：
- 都使用 MVC (Model-View-Controller) 設計模式
- 控制器語法看起來很相似
- 都使用 Razor 視圖引擎
- 路由配置方式類似

但實際上，這就像說 **汽油車和電動車差不多，因為都有四個輪子**！

### 🏗️ 底層架構根本不同

#### ASP.NET MVC (.NET Framework)
```csharp
// 基於古老的 System.Web 管道
public class MvcApplication : HttpApplication
{
    protected void Application_Start()
    {
        // 緊耦合的初始化
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}

// 手動管理依賴
public class BookController : Controller
{
    private BookService bookService = new BookService(); // 緊耦合！
}
```

#### ASP.NET Core MVC
```csharp
// 全新的中介軟體管道設計
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBookService, BookService>(); // 依賴注入

var app = builder.Build();
app.UseRouting();
app.MapControllerRoute(/*...*/);

// 現代化的依賴注入
public class BookController : Controller
{
    private readonly IBookService _bookService;
    
    public BookController(IBookService bookService) // 自動注入
    {
        _bookService = bookService;
    }
}
```

### 🔥 關鍵差異對比表

|      特性     |      ASP.NET MVC      | ASP.NET Core MVC |     實際影響     |
|---------------|-----------------------|------------------|------------------|
| **底層架構** | System.Web (古老)     | 全新輕量架構     | 效能差異 10x+    |
| **執行環境** | 只能 Windows + IIS    | 跨平台 + 容器    | 部署選擇天差地別 |
| **依賴注入** | 需手動或第三方        | 內建現代 DI      | 程式架構質的飛躍 |
| **非同步**   | 後加功能              | 原生設計         | 併發能力差異巨大 |
| **請求管道** | HTTP Modules/Handlers | 中介軟體管道     | 可擴展性完全不同 |
| **配置系統** | Web.config (XML)      | 強型別 + JSON    | 開發體驗天差地別 |
| **測試友善** | 困難                  | 易如反掌         | 開發效率差異顯著 |

### 🚀 實際程式碼差異示例

#### 資料存取方式
```csharp
// 舊版 (.NET Framework) - 古老的 ADO.NET
public List<Book> GetBooks()
{
    var books = new List<Book>();
    using (var conn = new SqlConnection(connectionString))
    {
        conn.Open(); // 阻塞執行緒！
        using (var cmd = new SqlCommand("SELECT * FROM Books", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read()) // 逐行讀取
            {
                books.Add(new Book 
                { 
                    BookId = Convert.ToInt32(reader["BookId"]), // 手動轉換
                    Title = reader["Title"].ToString()
                });
            }
        }
    }
    return books; // 同步返回
}

// 新版 (.NET Core 8) - 現代化 EF Core
public async Task<List<Book>> GetBooksAsync(CancellationToken cancellationToken = default)
{
    return await _context.Books // 強型別查詢
        .AsNoTracking() // 效能優化
        .ToListAsync(cancellationToken); // 非同步，可取消
}
```

#### 錯誤處理和日誌
```csharp
// 舊版 - 簡陋的錯誤處理
public ActionResult AddBook(Book book)
{
    try
    {
        bookService.AddBook(book);
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        // 基本錯誤處理
        ViewBag.Error = ex.Message;
        return View(book);
    }
}

// 新版 - 企業級錯誤處理
public async Task<IActionResult> AddBook(BookViewModel model, CancellationToken cancellationToken = default)
{
    if (!ModelState.IsValid) // 自動驗證
    {
        return View(model);
    }
    
    try
    {
        await _bookService.AddBookAsync(model.Title, model.Description, model.Position, cancellationToken);
        _logger.LogInformation("Successfully added book: {Title}", model.Title); // 結構化日誌
        TempData["Success"] = $"成功新增書籍：{model.Title}";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding book: {Title}", model.Title); // 完整錯誤記錄
        ModelState.AddModelError("", "新增書籍時發生錯誤");
        return View(model);
    }
}
```

#### 請求處理管道
```bash
# .NET Framework 請求流程
HTTP 請求 → IIS → System.Web → Global.asax → HTTP Modules → HTTP Handlers → MVC Pipeline → 控制器
（每個環節都有額外負擔，效能沉重）

# .NET Core 請求流程  
HTTP 請求 → Kestrel → 中介軟體管道 → 路由 → 控制器
（輕量化，每個中介軟體都是可選的）
```

### 🎯 為什麼升級如此值得？

#### 效能提升示例
```csharp
// 舊版 - 阻塞式處理
public ActionResult Search(string query)
{
    var vector = apiService.GetEmbedding(query); // 阻塞 500ms
    var results = bookService.SearchByVector(vector); // 阻塞 100ms
    return PartialView("_SearchResult", results);
    // 總時間：600ms，執行緒被阻塞
}

// 新版 - 非同步處理
public async Task<IActionResult> Search(SearchViewModel model, CancellationToken cancellationToken = default)
{
    var vector = await _apiService.GetEmbeddingAsync(model.Query, cancellationToken); // 非阻塞
    var results = await _bookService.SearchByVectorAsync(vector, 10, cancellationToken); // 非阻塞
    return PartialView("_SearchResult", results);
    // 總時間：600ms，但執行緒可處理其他請求！併發能力提升 10x+
}
```

#### 部署靈活性
```bash
# .NET Framework - 限制重重
- 只能部署到 Windows Server
- 必須安裝 IIS
- 需要 .NET Framework 執行環境
- 無法容器化（或容器很大）

# .NET Core 8 - 自由自在
- Linux/Windows/macOS 隨意選擇
- Docker 容器化（< 100MB）
- 雲端原生支援
- 自包含部署（不需要執行環境）
```

### 🔍 架構演進的深層意義

這次升級不只是技術升級，而是**開發思維的進化**：

1. **從命令式到宣告式**: 更多使用配置而非程式碼
2. **從同步到非同步**: 更好的資源利用率
3. **從緊耦合到鬆耦合**: 更易測試和維護
4. **從單體到模組化**: 更好的可擴展性

### 💡 總結

ASP.NET MVC 和 ASP.NET Core MVC 的關係就像：
- **Steam 引擎 vs 電動馬達**: 表面都是動力系統，內在完全不同
- **膠卷相機 vs 數位相機**: 都能拍照，但技術代差巨大
- **DOS vs Windows**: 都是作業系統，但使用體驗天差地別

這也解釋了為什麼這次遷移需要**重寫而非升級** - 因為我們實際上是在**換引擎**，而不是簡單的**換機油**！

## 🔄 主要架構變更

### 1. 專案檔案系統
|         .NET Framework          |              .NET Core 8               |
|---------------------------------|----------------------------------------|
| `BookVectorMVC.csproj` (舊格式) | `BookVectorMVC.Core.csproj` (SDK 格式) |
| `Web.config`                    | `appsettings.json`                     |
| `Global.asax.cs`                | `Program.cs`                           |
| `packages.config`               | PackageReference (內嵌在 .csproj)      |

### 2. 依賴注入系統
```csharp
// 舊版 (.NET Framework)
private BookService bookService = new BookService();

// 新版 (.NET Core 8)
private readonly IBookService _bookService;
public BookController(IBookService bookService) 
{
    _bookService = bookService;
}
```

### 3. 資料存取層
```csharp
// 舊版 (ADO.NET)
using (var conn = new SqlConnection(connString))
{
    conn.Open();
    using (var cmd = new SqlCommand(sql, conn))
    {
        // 執行查詢
    }
}

// 新版 (Entity Framework Core)
var books = await _context.Books
    .Where(b => b.Title.Contains(title))
    .ToListAsync(cancellationToken);
```

### 4. 非同步程式設計
```csharp
// 舊版 (同步)
public ActionResult Search(string query)
{
    var results = bookService.SearchByVector(query);
    return PartialView("_SearchResult", results);
}

// 新版 (非同步)
public async Task<IActionResult> Search(SearchViewModel model, CancellationToken cancellationToken = default)
{
    var results = await _bookService.SearchByVectorAsync(model.Query, 10, cancellationToken);
    return PartialView("_SearchResult", results);
}
```

## 🆕 新增功能

### 1. 強型別配置
```csharp
// appsettings.json
{
  "JinaAI": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.jina.ai/v1/embeddings",
    "Model": "jina-embeddings-v3",
    "VectorDimension": 1024
  }
}

// 程式碼中使用
var apiKey = _configuration["JinaAI:ApiKey"];
```

### 2. 健全的錯誤處理
```csharp
public async Task<IActionResult> AddBook(BookViewModel model, CancellationToken cancellationToken = default)
{
    try
    {
        var book = await _bookService.AddBookAsync(model.Title, model.Description, model.Position, cancellationToken);
        TempData["Success"] = $"成功新增書籍：{book.Title}";
        _logger.LogInformation("Successfully added book: {Title}", book.Title);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding book: {Title}", model.Title);
        TempData["Error"] = "新增書籍時發生錯誤";
    }
    
    return RedirectToAction(nameof(Index));
}
```

### 3. 現代化 UI 組件
- **Bootstrap 5**: 響應式設計
- **Font Awesome 6**: 豐富圖示系統
- **進度條**: 視覺化相似度分數
- **徽章系統**: 狀態指示器

## 🛠️ 套件升級對照

|   .NET Framework 套件   |           .NET Core 8 套件                |
|-------------------------|-------------------------------------------|
| `System.Data.SqlClient` | `Microsoft.EntityFrameworkCore.SqlServer` |
| `Newtonsoft.Json`       | `System.Text.Json`                        |
| `System.Web.Mvc`        | `Microsoft.AspNetCore.Mvc`                |
| `jQuery 3.4.1`          | `jQuery 3.6.0`                            |
| `Bootstrap 3.x`         | `Bootstrap 5.3.0`                         |

## 📊 效能改善

### 1. 記憶體使用
- **減少物件配置**: 使用 `Span<T>` 和 `Memory<T>`
- **連線池**: Entity Framework Core 自動管理
- **GC 壓力降低**: async/await 減少執行緒阻塞

### 2. 回應時間
- **非同步處理**: 不阻塞執行緒
- **更快的 JSON 序列化**: System.Text.Json
- **優化的路由**: ASP.NET Core 路由引擎

### 3. 擴充性
- **微服務友善**: 輕量級架構
- **容器化支援**: Docker 原生支援
- **雲端部署**: Azure、AWS 原生支援

## 🔐 安全性增強

### 1. 內建安全功能
```csharp
// CSRF 防護
[ValidateAntiForgeryToken]
public async Task<IActionResult> Add(BookViewModel model)

// 資料驗證
[Required(ErrorMessage = "書名不能為空")]
[StringLength(200, ErrorMessage = "書名長度不能超過 200 個字元")]
public string Title { get; set; } = string.Empty;
```

### 2. 設定管理
- **敏感資料隔離**: 使用 User Secrets
- **環境變數**: 支援 12-factor app
- **Azure Key Vault**: 生產環境金鑰管理

## 📝 遷移檢查清單

- [x] **專案檔案**: 轉換為 SDK 格式
- [x] **設定檔**: Web.config → appsettings.json
- [x] **依賴注入**: 註冊所有服務
- [x] **資料存取**: ADO.NET → Entity Framework Core
- [x] **非同步**: 所有 I/O 操作改為非同步
- [x] **錯誤處理**: 新增結構化日誌
- [x] **UI 更新**: Bootstrap 5 + Font Awesome
- [x] **安全性**: 啟用內建安全功能
- [x] **測試**: 單元測試和整合測試
- [x] **文件**: 更新 README 和 API 文件