# BookVectorMVC - 向量資料庫書籍管理系統

這是一個基於 ASP.NET MVC5 (.NET Framework) 的書籍管理系統，使用向量資料庫技術實現智能搜尋功能。

## 系統概述

本系統提供書籍的新增和搜尋功能，使用 MSSQL 資料庫儲存書籍資料，並將向量資料以 JSON 格式存儲在資料庫欄位中。系統採用 MVC 架構模式，提供簡潔易用的 Web 介面。

## 主要功能

### 1. 新增書籍
- 支援輸入書籍標題 (Title)
- 支援輸入書籍描述 (Description)  
- 支援輸入書籍位置 (Position) - 例如書架標籤 "A-12"
- 自動生成向量表示並儲存至資料庫

### 2. 搜尋書籍
- **關鍵字搜尋**: 使用向量相似度比對，找出最相關的書籍
- **位置搜尋**: 根據書架位置精確查找
- **書名搜尋**: 根據書名進行模糊查詢
- 搜尋結果包含相似度評分和排名

### 3. 書籍列表
- 顯示所有書籍的完整清單
- 包含 ID、標題、描述、位置等資訊

## 技術架構

### 資料庫設計 (MSSQL)
```sql
CREATE TABLE Books (
    BookId INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Position NVARCHAR(100) NULL, -- 實體位置或標籤
    Vector NVARCHAR(MAX) NOT NULL -- JSON 格式向量，例如 '[0.12,0.85,0.33]'
);
```

## 📊 欄位詳細說明

### 🏷️ Position 欄位 - 實體位置標籤系統

**Position** 欄位採用階層式編碼，記錄書籍的物理位置和分類資訊：

#### 位置編碼格式
```
格式：[書架代碼]-[位置編號]
範例：A-01, B-15, C-08, D-20, E-07
```

#### 書架分類系統
- **A 區** - 程式設計類 (Programming)
  - `A-01` ~ `A-25`: C#, Python, Java, JavaScript, Web開發
- **B 區** - 資料科學類 (Data Science) 
  - `B-01` ~ `B-20`: 資料結構、演算法、統計學、資料庫
- **C 區** - 人工智慧類 (AI/ML)
  - `C-01` ~ `C-20`: 機器學習、深度學習、NLP、電腦視覺
- **D 區** - 系統管理類 (System Management)
  - `D-01` ~ `D-25`: 雲端運算、DevOps、網路安全、系統架構
- **E 區** - 其他技術類 (Other Technologies)
  - `E-01` ~ `E-15`: 區塊鏈、物聯網、軟體測試、專案管理
- **F 區** - 文學小說類 (Literature)
  - `F-01` ~ `F-10`: 經典文學、科幻小說、推理小說
- **G 區** - 特殊收藏 (Special Collections)
  - `G-01` ~ `G-05`: 限量版、珍藏版、絕版書籍

#### Position 搜尋功能範例
```csharp
// 精確位置查詢
var book = bookService.SearchByPosition("A-01");

// 書架範圍查詢 (所有程式設計類書籍)
var programmingBooks = bookService.SearchByPositionPattern("A-%");

// SQL 查詢範例
SELECT * FROM Books WHERE Position LIKE 'C-%';  -- 查詢所有AI類書籍
SELECT * FROM Books WHERE Position BETWEEN 'A-01' AND 'A-25';  -- 範圍查詢
```

### 🧮 Vector 欄位 - 向量化數據分析

**Vector** 欄位以 JSON 陣列格式儲存書籍的數學向量表示，用於語義搜尋和相似度計算。

#### 向量生成演算法

##### 採用 Jina Embeddings API ，計算關鍵字向量值
````csharp
public float[] GetEmbedding(string text)
{
    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);

        // 建立 JSON 請求內容
        var body = new
        {
            model = "jina-embeddings-v3",
            task = "text-matching",
            input = new[] { text }
        };
        string jsonBody = JsonConvert.SerializeObject(body);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // 同步呼叫 POST
        var response = client.PostAsync(_url, content).Result;
        var jsonResponse = response.Content.ReadAsStringAsync().Result;

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Jina Error] {response.StatusCode} {jsonResponse}");
            return new float[0];
        }

        // 解析 JSON 結果
        var jobj = JObject.Parse(jsonResponse);
        var arr = jobj["data"]?[0]?["embedding"] as JArray;
        if (arr == null) return new float[0];

        return arr.Select(x => (float)x).ToArray();
    }
}
```
- **用途**: 語意搜尋 (Semantic Search)
- **說明**: 將文字轉為語意向量，比對語意相似度而非字面。
- **範例**: 搜尋「人性」，能找到描述「人道」、「靈魂」、「善良」的書。

// 範例
輸入: "人性"
輸出: [0.01817086, -0.14911059, 0.11055849, -0.01986349, 0.09199931, ...] (輸出維度:1024)

#### 🔍 相似度計算演算法

##### 餘弦相似度 (Cosine Similarity) ⭐
```csharp
// 計算兩向量間夾角的餘弦值
public double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
{
    double dotProduct = 0, normA = 0, normB = 0;
    
    for (int i = 0; i < Math.Min(vectorA.Length, vectorB.Length); i++)
    {
        dotProduct += vectorA[i] * vectorB[i];    // 向量點積
        normA += vectorA[i] * vectorA[i];         // 向量A模長平方
        normB += vectorB[i] * vectorB[i];         // 向量B模長平方  
    }
    
    return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)); // 餘弦值
}
```
- **數學公式**: `cos(θ) = (A·B) / (||A|| × ||B||)`
- **值域範圍**: [-1, 1]，1表示完全相同，0表示無關，-1表示完全相反
- **特點**: 不受向量長度影響，專注於方向相似性
- **最適用**: 文字語義相似度比較 (推薦使用)

#### 🚀 實際應用範例

##### 智能搜尋實作
```csharp
// 執行混合向量語義搜尋
var searchResults = enhancedBookService.SearchByVector(
    query: "深度學習 神經網路 TensorFlow",       // 複合關鍵字查詢
    vectorType: VectorType.Hybrid,             // 使用30維混合向量
    similarityMethod: SimilarityMethod.Cosine, // 餘弦相似度計算
    maxResults: 15                             // 返回前15個最相關結果
);

// 結果處理與顯示
foreach (var result in searchResults)
{
    Console.WriteLine($"📚 排名: {result.Rank}");
    Console.WriteLine($"📖 書名: {result.Book.Title}");
    Console.WriteLine($"📍 位置: {result.Book.Position}");
    Console.WriteLine($"🎯 相似度: {result.Score:F4} ({result.Score * 100:F1}%)");
    Console.WriteLine($"📝 描述: {result.Book.Description.Substring(0, 100)}...");
    Console.WriteLine("---");
}
```

### MVC 架構組件

#### Models
- **Book.cs**: 書籍實體模型
  - BookId: 主鍵
  - Title: 書籍標題
  - Description: 書籍描述
  - Position: 書籍位置
  - Vector: JSON 格式的向量資料

- **SearchResult.cs**: 搜尋結果模型
  - Book: 書籍物件
  - Score: 相似度評分
  - Rank: 排名

#### Controllers
- **BookController.cs**: 書籍管理控制器
  - Index(): 顯示主頁面和書籍列表
  - Add(): 新增書籍
  - Search(): 搜尋書籍 (支援多種搜尋方式)

#### Services
- **BookService.cs**: 書籍業務邏輯服務
  - 向量生成: 將文字轉換為向量表示
  - 相似度計算: 使用餘弦相似度算法
  - 資料庫操作: CRUD 功能實現

#### Views
- **Index.cshtml**: 主頁面
  - 新增書籍表單
  - 搜尋介面
  - 書籍列表顯示
- **_SearchResult.cshtml**: 搜尋結果部分檢視

## 向量技術實現

### 向量生成方式
系統使用簡化的向量生成方法：
- 將書籍標題和描述合併為文字
- 按空白字元分割文字
- 使用每個單詞的長度作為向量分量
- 將向量序列化為 JSON 格式儲存

### 相似度計算
使用餘弦相似度 (Cosine Similarity) 算法：
```
similarity = (A · B) / (||A|| × ||B||)
```

### 搜尋演算法
1. **向量搜尋**: 
   - 將查詢文字轉換為向量
   - 計算與所有書籍向量的相似度
   - 按相似度降序排列
   
2. **位置搜尋**: 
   - 使用 SQL 精確匹配 Position 欄位
   
3. **標題搜尋**: 
   - 使用 SQL LIKE 進行模糊查詢

## 使用方式

### 新增書籍
1. 在主頁面填寫書籍資訊
2. 點擊「新增」按鈕
3. 系統自動生成向量並儲存

### 搜尋書籍
1. 選擇搜尋方式：
   - 輸入關鍵字進行向量搜尋
   - 輸入位置進行精確查找
   - 輸入書名進行模糊搜尋
2. 點擊「搜尋」按鈕
3. 查看搜尋結果和相似度評分

## 系統需求

- .NET Framework 4.7.2 或更高版本
- ASP.NET MVC 5
- Microsoft SQL Server
- Newtonsoft.Json (JSON 序列化)
- jQuery (前端 AJAX 功能)

## 設定說明

1. 建立 MSSQL 資料庫
2. 執行 `/SQL/create_tables.sql` 建立資料表
3. 設定 `Web.config` 中的資料庫連接字串：
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=your_server;Database=BookDB;Integrated Security=true;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

## 特色功能

- ✅ **智能搜尋**: 基於向量相似度的語義搜尋
- ✅ **多重搜尋模式**: 支援關鍵字、位置、書名三種搜尋方式
- ✅ **即時搜尋**: 使用 AJAX 技術實現無刷新搜尋
- ✅ **相似度評分**: 提供搜尋結果的相關性評分
- ✅ **簡潔介面**: 直觀易用的 Web 介面
- ✅ **向量儲存**: 將向量資料直接存儲在 MSSQL 資料庫中

## 未來擴展

- 整合更先進的向量生成模型 (如 Word2Vec、BERT)
- 支援更多搜尋條件組合
- 新增書籍編輯和刪除功能
- 實現分頁功能
- 新增搜尋歷史記錄

## 📦 專案檔案結構

```
BookVectorMVC/
├── 📁 Controllers/
│   ├── BookController.cs              # 簡易版控制器
│   └── EnhancedBookController.cs      # 一般版控制器 (推薦)
├── 📁 Models/
│   ├── Book.cs                        # 書籍實體模型
│   └── SearchResult.cs                # 搜尋結果模型
├── 📁 Services/
│   ├── BookService.cs                 # 基本書籍和向量算法服務
│   ├── EnhancedBookService.cs         # 一般版服務
├── 📁 Views/
│   ├── 📁 Book/
│   │   ├── Index.cshtml               # 簡易版主頁
│   │   ├── Enhanced.cshtml            # 一般版主頁 (推薦)
│   │   ├── _SearchResult.cshtml       # 基本搜尋結果
│   │   └── _EnhancedSearchResult.cshtml # 增強版搜尋結果
│   └── 📁 Shared/
│       └── _Layout.cshtml             # 版面配置
├── 📁 SQL/
│   └── create_tables.sql              # 資料庫建表腳本
├── 📁 Properties/
│   └── AssemblyInfo.cs                # 組件資訊
├── BookVectorMVC.csproj               # 專案檔案
├── Global.asax                        # 應用程式入口點
├── Global.asax.cs                     # 應用程式啟動邏輯
├── Web.config                         # 主要設定檔
├── packages.config                    # NuGet 套件設定
├── RouteConfig.cs                     # 路由設定
└── README.md                          # 專案文檔 (本檔案)
```

## 🚀 快速開始

### 1. 系統需求
- **Visual Studio 2017 或更新版本**
- **.NET Framework 4.7.2 或更高版本**
- **SQL Server 2016 或更新版本** (包含 LocalDB、Express 版本)
- **IIS Express** (Visual Studio 內建)

### 2. 專案檔案檢查清單

確保以下檔案存在：

#### 🔧 核心檔案
- ✅ `BookVectorMVC.csproj` - 專案檔案
- ✅ `Global.asax` - 應用程式入口點
- ✅ `Global.asax.cs` - 應用程式啟動邏輯
- ✅ `Web.config` - 設定檔
- ✅ `packages.config` - NuGet 套件設定
- ✅ `RouteConfig.cs` - 路由設定

#### 📁 MVC 架構
- ✅ `Models/Book.cs` - 書籍資料模型
- ✅ `Models/SearchResult.cs` - 搜尋結果模型
- ✅ `Controllers/BookController.cs` - 基本控制器
- ✅ `Controllers/EnhancedBookController.cs` - 一般版控制器
- ✅ `Services/BookService.cs` - 基本服務
- ✅ `Services/EnhancedBookService.cs` - 一般版服務

#### 🎨 視圖檔案
- ✅ `Views/_ViewStart.cshtml` - 視圖啟動檔
- ✅ `Views/Shared/_Layout.cshtml` - 版面配置
- ✅ `Views/web.config` - 視圖設定
- ✅ `Views/Book/Index.cshtml` - 基本主頁
- ✅ `Views/Book/Enhanced.cshtml` - 一般版主頁
- ✅ `Views/Book/_SearchResult.cshtml` - 基本搜尋結果
- ✅ `Views/Book/_EnhancedSearchResult.cshtml` - 一般版搜尋結果

#### 🗄️ 資料庫
- ✅ `SQL/create_tables.sql` - 建表腳本

## 🔧 詳細部署步驟

### 步驟 1: 開啟專案
1. 啟動 **Visual Studio**
2. 選擇 **「開啟專案或解決方案」**
3. 瀏覽到 `BookVectorMVC.csproj` 檔案並開啟

### 步驟 2: 還原 NuGet 套件
1. 在 Visual Studio 中，右鍵點擊解決方案
2. 選擇 **「還原 NuGet 套件」**
3. 等待套件下載完成

**主要套件：**
- `Microsoft.AspNet.Mvc` (5.2.7)
- `Newtonsoft.Json` (12.0.2)
- `Microsoft.AspNet.WebPages` (3.2.7)

### 步驟 3: 設定資料庫

#### 選項 A: 使用 LocalDB (推薦開發環境)
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BookDB.mdf;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### 選項 B: 使用 SQL Server Express
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=.\SQLEXPRESS;Database=BookDB;Trusted_Connection=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### 選項 C: 使用完整 SQL Server
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=your_server_name;Database=BookDB;Integrated Security=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 步驟 4: 建立資料庫
1. 開啟 **SQL Server Management Studio** 或 **Visual Studio 的 SQL Server 物件總管**
2. 連接到您的 SQL Server 實例
3. 建立新資料庫：
   ```sql
   CREATE DATABASE BookDB;
   ```
4. 執行 `SQL/create_tables.sql` 腳本：
   ```sql
   USE BookDB;
   
   CREATE TABLE Books (
       BookId INT IDENTITY PRIMARY KEY,
       Title NVARCHAR(200) NOT NULL,
       Description NVARCHAR(MAX) NULL,
       Position NVARCHAR(100) NULL,
       Vector NVARCHAR(MAX) NOT NULL
   );
   ```

### 步驟 5: 編譯專案
1. 在 Visual Studio 中按 **Ctrl+Shift+B** 或選擇 **「建置」→「建置方案」**
2. 確保沒有編譯錯誤

### 步驟 6: 執行應用程式
1. 按 **F5** 或點擊 **「開始偵錯」**
2. IIS Express 會自動啟動
3. 瀏覽器會開啟並導向應用程式

## 🌐 應用程式使用

### 基本版功能
- **首頁**: `http://localhost:port/Book/Index`
- **新增書籍**: 填寫表單並提交
- **搜尋書籍**: 使用關鍵字、位置或書名搜尋

### 增強版功能 🚀
- **增強版首頁**: `http://localhost:port/EnhancedBook/Enhanced`
- **多種向量算法**: Simple、Semantic、TF-IDF、Hybrid
- **多種相似度計算**: Cosine、Euclidean、Manhattan

## 🔧 設定調整

### 資料庫連接字串範例

#### 開發環境 (LocalDB)
```xml
<add name="DefaultConnection" 
     connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BookDB.mdf;Integrated Security=True" 
     providerName="System.Data.SqlClient" />
```

## 🔍 功能測試

### 1. 搜尋功能驗證
- **關鍵字搜尋**: 輸入相關詞彙進行向量搜尋
- **位置搜尋**: 輸入書架位置進行精確匹配  
- **書名搜尋**: 輸入書名片段進行模糊搜尋

### 2. 系統狀態檢查
1. 前往一般版頁面
2. 檢查系統統計資訊
3. 確認各項功能運作正常

## 🛠️ 故障排除

### 常見問題

#### 1. 編譯錯誤
- 確保安裝了 .NET Framework 4.7.2
- 檢查 NuGet 套件是否正確還原
- 清理並重新建置解決方案

#### 2. 資料庫連接問題
- 檢查連接字串是否正確
- 確保 SQL Server 服務正在執行
- 驗證資料庫和資料表是否已建立

#### 3. 404 錯誤
- 檢查路由設定 `RouteConfig.cs`
- 確保控制器類別名稱正確
- 驗證視圖檔案路徑

#### 4. 向量搜尋無結果
- 確保書籍資料已包含向量資訊
- 檢查 `EnhancedBookService` 中的向量生成邏輯
- 驗證搜尋查詢格式

## 🎯 未來擴展計劃

### 短期目標
- [ ] 新增書籍編輯和刪除功能
- [ ] 實現搜尋結果分頁
- [ ] 新增書籍分類功能
- [ ] 實現搜尋歷史記錄

### 中期目標
- [ ] 整合 OpenAI Embeddings API
- [ ] 支援圖片和檔案上傳
- [ ] 新增用戶管理系統
- [ ] 實現推薦系統

### 長期目標
- [ ] 遷移到 .NET Core/.NET 6+
- [ ] 實現微服務架構
- [ ] 支援多語言和國際化
- [ ] 整合機器學習模型

## 🎉 完成！

BookVectorMVC 應用程式現在已準備就緒！
