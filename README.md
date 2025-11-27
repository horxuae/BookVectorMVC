# BookVectorMVC - .NET Core 8 向量資料庫書籍管理系統

## 📚 系統概述

BookVectorMVC 是一個基於 .NET Core 8 開發的智能書籍管理系統，結合了向量數據庫技術和現代會員管理功能。系統支援語義搜索、書籍 CRUD 操作、使用者認證等功能，提供完整的圖書館或個人藏書管理解決方案。

## 📋 專案概述

這是一個基於 **ASP.NET Core 8** 框架的現代化智能書籍管理系統，採用 **Jina AI Embeddings v3** 技術實現向量化語義搜尋功能。系統使用最新的 .NET Core 架構，具備依賴注入、非同步處理、強型別配置等特性，可以根據書籍內容的語義相似度進行智能搜尋，而不僅僅是關鍵字匹配。

## ✨ 主要功能

### 🤖 AI 聊天機器人書籍助手
- **Perplexity AI 驅動**: 使用先進的 Perplexity AI 進行智能書籍搜尋
- **智能對話搜尋**: 支援自然語言查詢，如「推薦機器學習入門書籍」
- **多重搜尋來源**: Perplexity AI → Google Books API → 模擬資料的智能回退機制
- **優化聊天介面**: 520px×700px 大視窗，支援桌面與手機響應式設計
- **完整書籍資訊**: 自動抓取標題、作者、描述、ISBN、出版年份等完整資訊
- **批量選擇新增**: 可視化書籍卡片選擇，支援多選並即時顯示選中計數
- **流暢滾動體驗**: 優化的滾動條設計，確保所有搜尋結果都能完整顯示
- **智能去重檢查**: 避免新增重複書籍到資料庫
- **權限安全控制**: 只有管理員可透過機器人新增書籍

### 📖 書籍管理功能
- **完整 CRUD 操作**: 新增、查詢、編輯、刪除書籍資料
- **智慧語義搜尋**: 使用 Jina AI Embeddings v3 模型實現基於語義的書籍搜尋
- **多重搜尋方式**: 支援書名模糊搜尋、位置精確搜尋和關鍵字語義搜尋
- **向量相似度計算**: 採用餘弦相似度算法進行智能匹配
- **實體位置管理**: 追蹤書籍的實體存放位置
- **批量向量更新**: 一鍵更新所有書籍的向量表示

### 👤 會員系統功能
- **使用者註冊/登入**: 完整的身份驗證系統
- **角色權限管理**: 支援 Admin 和 Member 角色
- **密碼安全保護**: 使用 SHA256 加密儲存密碼
- **登入狀態管理**: Cookie 基礎的身份驗證，支援 "記住我" 功能
- **使用者資料管理**: 顯示名稱、電子郵件、最後登入時間等
- **帳戶安全**: 唯一使用者名稱和電子郵件驗證

### 🎨 使用者介面
- **雙版本介面**: 提供基礎版和增強版兩種使用體驗
- **現代化 UI**: 使用 Bootstrap 5 和 Font Awesome 打造響應式介面
- **直觀操作**: 清晰的編輯、刪除和查看按鈕
- **即時回饋**: 成功/錯誤訊息提示系統
- **響應式設計**: 支援桌面和移動設備
- **無障礙友善**: 符合網頁無障礙標準

### 🔒 安全性功能
- **CSRF 防護**: Anti-forgery token 保護
- **XSS 防護**: Razor 自動編碼輸出
- **SQL 注入防護**: Entity Framework 參數化查詢
- **身份驗證**: Cookie 基礎的安全認證
- **資料驗證**: 客戶端和伺服器端雙重驗證

## 🛠️ 技術架構

### 核心技術棧
- **後端框架**: ASP.NET Core 8
- **目標框架**: .NET 8.0
- **資料庫**: SQL Server (使用 Entity Framework Core 8)
- **向量服務**: Jina AI Embeddings v3 (1024維向量)
- **外部API**: Perplexity AI API (智能搜尋)、Google Books API (書籍搜尋)
- **前端技術**: Bootstrap 5 + jQuery + Font Awesome
- **JSON處理**: System.Text.Json
- **套件管理**: NuGet
- **設計模式**: 依賴注入、Repository Pattern、服務層分離
- **身份驗證**: ASP.NET Core Identity (Cookie Authentication)
- **安全加密**: SHA256 演算法
- **HTTP客戶端**: HttpClient (用於API呼叫)

### 分層架構設計

```
📁 BookVectorMVC/
├── 📁 Controllers/          # MVC 控制器
│   ├── BookController.cs    # 基本書籍管理
│   ├── EnhancedBookController.cs # 增強功能
│   ├── ChatBotController.cs # AI聊天機器人控制器
│   └── AIAssistantController.cs # AI助手功能控制器
├── 📁 Models/              # 資料模型
│   ├── Book.cs             # 書籍實體
│   ├── SearchResult.cs     # 搜尋結果模型
│   └── ViewModels/         # 視圖模型
├── 📁 Services/            # 業務邏輯層
│   ├── Interfaces/         # 服務介面
│   │   ├── IBookSearchService.cs # 書籍搜尋服務介面
│   │   └── IBookRecommendationService.cs # AI推薦服務介面
│   ├── BookService.cs      # 書籍服務
│   ├── EnhancedBookService.cs # 增強服務
│   ├── BookSearchService.cs # 線上書籍搜尋服務 (Perplexity AI + Google Books)
│   ├── BookRecommendationService.cs # AI推薦服務
│   └── ApiService.cs       # API服務
├── 📁 Data/                # 資料存取層
│   └── BookDbContext.cs    # EF Core DbContext
├── 📁 Views/               # 視圖層
│   ├── Book/               # 書籍相關視圖
│   └── Shared/             # 共用視圖
│       └── _ChatBot.cshtml # AI聊天機器人UI組件
├── 📄 Program.cs           # 應用程式進入點
├── 📄 appsettings.json     # 配置文件
└── 📁 SQL/                 # 資料庫腳本
    ├── create_tables.sql
    └── reset_database.sql
```

## 🚀 快速開始

### 環境需求
- **Visual Studio 2022+** 或 **Visual Studio Code**
- **.NET 8.0 SDK**
- **SQL Server 2019+** (或 LocalDB)
- **Internet 連線** (用於 Jina AI API 呼叫)

### 安裝步驟

1. **clone專案**
   ```bash
   git clone https://github.com/your-repo/BookVectorMVC.git
   cd BookVectorMVC
   ```

2. **還原 NuGet 套件**
   ```bash
   dotnet restore
   ```

3. **設定資料庫連線**
   - 編輯 `appsettings.json` 中的連線字串
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookVectorMVC_Core;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

4. **設定 API 金鑰**

   **Jina AI API 金鑰** (語義搜尋必需):
   - 在 `appsettings.json` 中設定您的 API 金鑰
   ```json
   {
     "JinaAI": {
       "ApiKey": "your-jina-api-key",
       "BaseUrl": "https://api.jina.ai/v1/embeddings",
       "Model": "jina-embeddings-v3",
       "VectorDimension": 1024
     }
   }
   ```

   **Perplexity AI API 金鑰** (聊天機器人可選):
   ```powershell
   # Windows PowerShell
   $env:PERPLEXITY_API_KEY = "pplx-your-api-key-here"
   
   # 進階配置 (可選)
   $env:PERPLEXITY_MODEL = "llama-3.1-sonar-small-128k-online"
   $env:PERPLEXITY_MAX_RESULTS = "8"
   $env:PERPLEXITY_TEMPERATURE = "0.2"
   ```

   ```bash
   # macOS/Linux
   export PERPLEXITY_API_KEY="pplx-your-api-key-here"
   export PERPLEXITY_MAX_RESULTS="8"
   ```

   > 💡 **提示**: 如果沒有設置 Perplexity API，聊天機器人會自動回退到 Google Books API

5. **建立資料庫**
   ```bash
   dotnet ef database update
   ```
   或執行 SQL 腳本：
   ```sql
   -- 執行 SQL/create_tables.sql
   CREATE TABLE Books (
       BookId INT IDENTITY PRIMARY KEY,
       Title NVARCHAR(200) NOT NULL,
       Description NVARCHAR(MAX),
       Position NVARCHAR(100),
       Vector NVARCHAR(MAX) NOT NULL
   );
   ```

6. **執行應用程式**
   ```bash
   dotnet run
   ```
   或在 Visual Studio 中按 F5

### 瀏覽應用程式
- **簡易版**: `http://localhost:5000/Book/Index`
- **增強版**: `http://localhost:5000/EnhancedBook/Enhanced`
- **使用者註冊**: `http://localhost:5000/Account/Register`
- **使用者登入**: `http://localhost:5000/Account/Login`

## 📝 使用指南

### 會員系統使用

1. **新使用者註冊**
   - 訪問註冊頁面：`/Account/Register`
   - 填寫使用者名稱、電子郵件、密碼等資訊
   - 系統自動驗證資料唯一性
   - 註冊成功後自動登入

2. **使用者登入**
   - 訪問登入頁面：`/Account/Login`
   - 輸入使用者名稱和密碼
   - 可選擇 "記住我" 保持登入狀態
   - 登入後可查看使用者資訊和角色

3. **權限管理**
   - **Member**: 基本的書籍查看和搜尋功能
   - **Admin**: 完整的書籍管理功能（新增、編輯、刪除）

### 🤖 AI聊天機器人使用

1. **開啟聊天機器人**
   - 在任何頁面右下角點擊 🤖 浮動按鈕（具有脈衝動畫效果）
   - 機器人會顯示歡迎訊息和使用說明
   - 聊天視窗支援現代化漸層設計和毛玻璃效果

2. **智能書籍搜尋**
   - 使用自然語言描述需求（如："我想找關於機器學習的入門書籍"）
   - Perplexity AI 會進行智能分析並搜尋相關書籍
   - 支援中文書籍優先搜尋，包含經典與新出版作品

3. **瀏覽搜尋結果**
   - 搜尋結果以美觀的卡片形式展示（520px 寬視窗）
   - 每本書顯示完整資訊：標題、作者、描述、ISBN、出版年份
   - 優化的滾動設計確保所有結果都能完整瀏覽

4. **選擇和新增書籍**
   - 點擊書籍卡片或勾選 checkbox 進行選擇
   - 即時顯示「已選擇 X / Y 本書籍」計數
   - 選中的書籍會有視覺回饋（藍色背景 + ✓ 標記）
   - 點擊「✅ 新增選中的書籍」批量加入圖書館

5. **進階功能特色**
   - **多重API來源**: Perplexity AI → Google Books → 備用資料
   - **響應式設計**: 完美支援桌面、平板、手機各種螢幕
   - **流暢動畫**: 所有操作都有平滑的視覺過渡效果
   - **智能去重**: 自動檢查標題避免重複新增
   - **安全權限**: 只有管理員能新增書籍，普通用戶僅可瀏覽

### 書籍管理使用

1. **新增書籍**
   - 填寫書名（必填）
   - 添加描述和實體位置（可選）
   - 系統自動生成 AI 向量表示

2. **編輯書籍**
   - 點擊書籍列表中的「編輯」按鈕
   - 修改書籍資訊
   - 系統自動更新向量表示

3. **刪除書籍**
   - 點擊書籍列表中的「刪除」按鈕
   - 確認刪除操作（不可恢復）
   - 資料永久移除

4. **搜尋功能**
   - **語義搜尋**: 輸入描述性關鍵字，AI 理解語義進行匹配
   - **書名搜尋**: 模糊匹配書籍標題
   - **位置搜尋**: 精確匹配實體存放位置

## 🔍 主要功能

### 📚 書籍管理
- ✅ **CRUD 操作**: 新增、查詢書籍
- ✅ **資料驗證**: 完整的前後端資料驗證機制
- ✅ **異常處理**: 錯誤處理和日誌記錄

### 🔍 智能搜尋引擎
- ✅ **語義搜尋**: 使用 1024 維向量進行語義相似度匹配
- ✅ **多模式搜尋**: 
  - 🎯 **向量搜尋**: 基於語義的智能搜尋
  - 📍 **位置搜尋**: 根據書架位置精確查找
  - 📖 **書名搜尋**: 支援模糊匹配的書名搜尋
- ✅ **搜尋結果排序**: 依據相似度分數自動排序
- ✅ **即時搜尋**: AJAX 技術實現無刷新搜尋

### 📊 系統統計與分析
- ✅ **基本統計**: 書籍總數、平均標題長度、位置分布
- ✅ **向量品質分析**: 向量維度一致性、數值分布統計
- ✅ **搜尋效能**: 搜尋時間、準確度分析
- ✅ **資料品質評分**: 自動評估資料完整性

## 🏗️ .NET Core 8 升級特性

### 🆕 現代化架構改進
- **依賴注入**: 使用內建 DI 容器管理服務生命週期
- **非同步程式設計**: 全面採用 async/await 模式
- **強型別配置**: 使用 IConfiguration 和 Options 模式
- **健康檢查**: 內建健康檢查和監控功能
- **日誌系統**: 結構化日誌記錄和效能監控

## 🧮 演算法實作

### 向量相似度計算
系統使用**餘弦相似度 (Cosine Similarity)** 演算法來計算書籍間的語義相似度：

```
相似度 = cos(θ) = (A·B) / (|A| × |B|)
```

- **A, B**: 兩個向量 (書籍的向量表示)
- **A·B**: 向量點積
- **|A|, |B|**: 向量的模長

### 搜尋流程
1. **文本向量化**: 將搜尋查詢轉換為 1024 維向量
2. **相似度計算**: 計算查詢向量與所有書籍向量的餘弦相似度
3. **結果排序**: 依據相似度分數降序排列
4. **結果過濾**: 返回前 N 個最相似的結果

## 📊 效能指標

### 系統效能
- **向量維度**: 1024 維 (Jina Embeddings v3)
- **搜尋時間**: 平均 < 100ms (100 本書籍)
- **向量生成**: 平均 200-500ms (取決於網路延遲)
- **記憶體使用**: 每個向量約 4KB

### 搜尋準確度
- **語義理解**: 支援同義詞和相關概念搜尋
- **多語言**: 支援中英文混合搜尋
- **模糊匹配**: 容忍拼寫錯誤和變體

## 🔧 核心檔案說明

### 後端核心
- **`Program.cs`**: 應用程式進入點和服務配置
- **`BookDbContext.cs`**: Entity Framework Core 資料庫上下文
- **`BookService.cs`**: 書籍管理核心業務邏輯
- **`ApiService.cs`**: Jina AI API 整合服務
- **`EnhancedBookService.cs`**: 進階統計和分析功能

### 前端介面
- **`Views/Book/Index.cshtml`**: 簡易版書籍管理介面
- **`Views/Book/Enhanced.cshtml`**: 增強版管理介面
- **`Views/Shared/_Layout.cshtml`**: 主要版面配置

### 配置檔案
- **`appsettings.json`**: 主要配置檔案
- **`BookVectorMVC.Core.csproj`**: 專案檔案和套件引用

## 🔧 進階配置

### AI 服務配置

#### Perplexity AI 完整配置
```powershell
# 基本設定
$env:PERPLEXITY_API_KEY = "pplx-your-api-key"

# 搜尋行為調整
$env:PERPLEXITY_MAX_RESULTS = "8"              # 搜尋結果數量 (1-10)
$env:PERPLEXITY_TEMPERATURE = "0.2"            # 創意程度 (0.0-1.0)
$env:PERPLEXITY_MAX_TOKENS = "2000"            # 回應長度限制

# 搜尋來源限制
$env:PERPLEXITY_SEARCH_DOMAINS = "books.google.com,amazon.com,goodreads.com,eslite.com"

# 模型選擇 (可選)
$env:PERPLEXITY_MODEL = "llama-3.1-sonar-small-128k-online"
```

#### 智能搜尋回退機制
1. **第一層**: Perplexity AI (主要) - 智能語義搜尋
2. **第二層**: Google Books API (備用) - 傳統關鍵字搜尋  
3. **第三層**: 模擬資料 (保底) - 確保功能不中斷

### 效能最佳化建議
- **API額度管理**: 監控 Perplexity API 使用量避免超額
- **快取策略**: 考慮實作搜尋結果快取減少 API 呼叫
- **回應時間**: 設定適當的 timeout 避免長時間等待

## 🔐 安全性考量

### API 金鑰管理
- ✅ 使用環境變數存放敏感的 API 金鑰
- ✅ Jina AI 金鑰可在 `appsettings.json` 中管理
- ✅ 支援環境變數覆寫配置檔案設定
- ✅ 建議在生產環境使用 Azure Key Vault 或類似服務

### 資料驗證
- ✅ 伺服器端資料驗證
- ✅ SQL 注入防護 (使用 EF Core 參數化查詢)
- ✅ XSS 防護 (ASP.NET Core 內建)
- ✅ CSRF 防護 (使用 AntiForgeryToken)

## 🚀 部署指南

### 本地開發
```bash
dotnet run --environment Development
```

### 生產部署
```bash
dotnet publish -c Release -o ./publish
```

### Docker 部署
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "BookVectorMVC.dll"]
```