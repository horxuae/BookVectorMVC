# BookVectorMVC - .NET Core 8 向量資料庫書籍管理系統

## 📋 專案概述

這是一個基於 **ASP.NET Core 8** 框架的現代化智能書籍管理系統，採用 **Jina AI Embeddings v3** 技術實現向量化語義搜尋功能。系統使用最新的 .NET Core 架構，具備依賴注入、非同步處理、強型別配置等特性，可以根據書籍內容的語義相似度進行智能搜尋，而不僅僅是關鍵字匹配。

## 🛠️ 技術架構

### 核心技術棧
- **後端框架**: ASP.NET Core 8
- **目標框架**: .NET 8.0
- **資料庫**: SQL Server (使用 Entity Framework Core 8)
- **向量服務**: Jina AI Embeddings v3 (1024維向量)
- **前端技術**: Bootstrap 5 + jQuery + Font Awesome
- **JSON處理**: System.Text.Json
- **套件管理**: NuGet
- **設計模式**: 依賴注入、Repository Pattern、服務層分離

### 分層架構設計

```
📁 BookVectorMVC/
├── 📁 Controllers/          # MVC 控制器
│   ├── BookController.cs    # 基本書籍管理
│   └── EnhancedBookController.cs # 增強功能
├── 📁 Models/              # 資料模型
│   ├── Book.cs             # 書籍實體
│   ├── SearchResult.cs     # 搜尋結果模型
│   └── ViewModels/         # 視圖模型
├── 📁 Services/            # 業務邏輯層
│   ├── Interfaces/         # 服務介面
│   ├── BookService.cs      # 書籍服務
│   ├── EnhancedBookService.cs # 增強服務
│   └── ApiService.cs       # API服務
├── 📁 Data/                # 資料存取層
│   └── BookDbContext.cs    # EF Core DbContext
├── 📁 Views/               # 視圖層
│   ├── Book/               # 書籍相關視圖
│   └── Shared/             # 共用視圖
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

4. **設定 Jina AI API 金鑰**
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

## 🔐 安全性考量

### API 金鑰管理
- ✅ 使用 `appsettings.json` 管理敏感資訊
- ✅ 支援環境變數覆寫
- ✅ 建議在生產環境使用 Azure Key Vault

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