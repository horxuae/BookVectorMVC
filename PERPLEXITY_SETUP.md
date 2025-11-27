# Perplexity AI 設置指南

## 🧠 關於 Perplexity AI

Perplexity AI 是一個強大的搜尋和問答AI服務，我們的聊天機器人使用它來提供更智能、更準確的書籍搜尋結果。

## 🔑 API 密鑰設置

### 1. 獲取 Perplexity API 密鑰

1. 前往 [Perplexity API](https://www.perplexity.ai/settings/api)
2. 註冊或登入您的帳號
3. 創建新的 API 密鑰
4. 複製您的 API 密鑰

### 2. 設置環境變數

#### Windows (PowerShell)
```powershell
# 臨時設置（僅當前會話有效）
$env:PERPLEXITY_API_KEY = "你的API密鑰"

# 永久設置（需要重啟終端）
[Environment]::SetEnvironmentVariable("PERPLEXITY_API_KEY", "你的API密鑰", "User")
```

#### Windows (命令提示字元)
```cmd
set PERPLEXITY_API_KEY=你的API密鑰
```

#### macOS/Linux
```bash
# 臨時設置
export PERPLEXITY_API_KEY="你的API密鑰"

# 永久設置（添加到 ~/.bashrc 或 ~/.zshrc）
echo 'export PERPLEXITY_API_KEY="你的API密鑰"' >> ~/.bashrc
source ~/.bashrc
```

### 3. 驗證設置

啟動應用程式後，查看日誌。如果看到以下訊息表示設置成功：
- ✅ 正常：機器人會使用 Perplexity AI 進行搜尋
- ⚠️ 未設置：會看到 "未設置 PERPLEXITY_API_KEY 環境變數，跳過 Perplexity 搜尋" 並自動回退到 Google Books API

## 🚀 功能優勢

### 使用 Perplexity AI 的好處：
- **智能理解**：能理解自然語言查詢
- **即時搜尋**：搜尋最新的書籍資訊
- **精準推薦**：基於語境提供相關書籍
- **多源整合**：結合多個書籍資料庫

### 回退機制：
如果 Perplexity API 不可用，系統會自動：
1. 回退到 Google Books API
2. 如果都失敗，提供模擬資料

## 💡 使用建議

### 最佳查詢方式：
- ✅ **推薦**：「我想找關於機器學習的入門書籍」
- ✅ **推薦**：「推薦一些歷史小說」
- ✅ **推薦**：「Python程式設計教材」
- ❌ **避免**：單個關鍵字如「書」

### 搜尋結果：
- 每次搜尋返回 5-8 本相關書籍
- 包含完整的書籍資訊（標題、作者、描述、ISBN等）
- 專注於中文書籍或有中文翻譯的書籍

## 🛠️ 故障排除

### 常見問題：

**Q: API 密鑰設置了但還是使用 Google Books？**
A: 請確認：
- 環境變數名稱正確：`PERPLEXITY_API_KEY`
- 重新啟動應用程式
- 檢查 API 密鑰是否有效

**Q: 搜尋結果品質不佳？**
A: 嘗試：
- 使用更具體的描述
- 包含書籍類型或主題
- 避免過於簡短的查詢

**Q: API 額度用完了？**
A: 系統會自動回退到 Google Books API，功能不受影響

## 📊 API 使用情況

- **模型**：llama-3.1-sonar-small-128k-online
- **搜尋域名**：books.google.com, amazon.com, goodreads.com
- **每次搜尋 Token**：約 1500-2000 tokens
- **回應格式**：結構化 JSON

---

💡 **提示**：如果不想使用 Perplexity API，系統會自動使用 Google Books API，功能完全正常運作！