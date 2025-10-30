using BookVectorMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace BookVectorMVC.Services
{
    public class BookService
    {
        private string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private APIService apiService = new APIService();

        /// <summary>
        /// 獲取所有書籍記錄
        /// 從資料庫讀取所有書籍資料，包含向量資訊
        /// </summary>
        /// <returns>書籍清單</returns>
        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT BookId, Title, Description, Position, Vector FROM Books ORDER BY BookId",
                    conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookId = Convert.ToInt32(reader["BookId"]),
                            Title = reader["Title"] == DBNull.Value ? string.Empty : reader["Title"].ToString(),
                            Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                            Position = reader["Position"] == DBNull.Value ? string.Empty : reader["Position"].ToString(),
                            Vector = reader["Vector"] == DBNull.Value ? "[]" : reader["Vector"].ToString()
                        });
                    }
                }
            }

            return books;
        }

        /// <summary>
        /// 基於向量相似度的智能搜尋
        /// 演算法流程：
        /// 1. 將查詢文本轉換為向量
        /// 2. 計算與所有書籍向量的相似度
        /// 3. 按相似度降序排列並分配排名
        /// </summary>
        /// <param name="query">搜尋查詢文本</param>
        /// <returns>按相似度排序的搜尋結果</returns>
        public List<SearchResult> SearchByVector(string query, int maxResults = 10)
        {
            var list = new List<SearchResult>();
            var vector = apiService.GetEmbedding(query);  // 查詢向量化
            var books = GetAllBooks();

            foreach (var book in books)
            {
                var unVector = UnpackVector(book.Vector);  // 書籍向量反序列化
                double score = Cosine(vector, unVector);      // 計算餘弦相似度
                list.Add(new SearchResult { Book = book, Score = score });
            }

            return list.OrderByDescending(x => x.Score)
                       .Select((x, i) => { 
                           x.Rank = i + 1;
                           return x; 
                       })
                       .Take(maxResults)
                       .ToList();
        }

        /// <summary>
        /// 根據書籍位置進行精確搜尋
        /// 適用於已知書架位置或編號的直接查詢
        /// </summary>
        /// <param name="position">書籍位置（如：A-01, B-15）</param>
        /// <returns>匹配位置的書籍搜尋結果</returns>
        public List<SearchResult> SearchByPosition(string position)
        {
            var results = new List<SearchResult>();

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT BookId, Title, Description, Position, Vector FROM Books WHERE Position = @position", conn))
                {
                    cmd.Parameters.AddWithValue("@position", position);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book
                            {
                                BookId = Convert.ToInt32(reader["BookId"]),
                                Title = reader["Title"] == DBNull.Value ? string.Empty : reader["Title"].ToString(),
                                Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                                Position = reader["Position"] == DBNull.Value ? string.Empty : reader["Position"].ToString(),
                                Vector = reader["Vector"] == DBNull.Value ? "[]" : reader["Vector"].ToString()
                            };

                            results.Add(new SearchResult { Book = book, Score = 1.0, Rank = -1 });
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 根據書籍標題進行模糊搜尋
        /// 使用 SQL LIKE 查詢進行部分匹配
        /// </summary>
        /// <param name="title">書籍標題關鍵字</param>
        /// <returns>標題匹配的搜尋結果</returns>
        public List<SearchResult> SearchByTitle(string title)
        {
            var results = new List<SearchResult>();

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT BookId, Title, Description, Position, Vector FROM Books WHERE Title LIKE @title",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@title", $"%{title}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book
                            {
                                BookId = Convert.ToInt32(reader["BookId"]),
                                Title = reader["Title"] == DBNull.Value ? string.Empty : reader["Title"].ToString(),
                                Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                                Position = reader["Position"] == DBNull.Value ? string.Empty : reader["Position"].ToString(),
                                Vector = reader["Vector"] == DBNull.Value ? "[]" : reader["Vector"].ToString()
                            };

                            results.Add(new SearchResult { Book = book, Score = 1.0, Rank = -1 });
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 新增書籍到資料庫並自動生成向量表示
        /// 處理流程：
        /// 1. 合併書名和描述作為完整文本
        /// 2. 生成向量表示
        /// 3. 序列化向量為JSON
        /// 4. 存入資料庫
        /// </summary>
        /// <param name="title">書籍標題</param>
        /// <param name="desc">書籍描述</param>
        /// <param name="position">書籍位置（書架編號等）</param>
        public void AddBook(string title, string desc, string position)
        {
            var combinedText = $"{title} {desc}";
            var vector = apiService.GetEmbedding(combinedText);  // 查詢向量化
            var vectorJson = Serialize(vector); // 向量序列化

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("INSERT INTO Books (Title, Description, Position, Vector) VALUES (@t,@d,@p,@v)", conn))
                {
                    cmd.Parameters.AddWithValue("@t", title ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@d", desc ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p", position ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@v", vectorJson ?? "[]");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 更新所有書籍向量值
        /// </summary>
        public void UpdateAllVectors()
        {
            var books = GetAllBooks();

            foreach (var book in books)
            {
                var vector = apiService.GetEmbedding(book.Title + " " + book.Description); // 查詢向量化
                var vectorJson = Serialize(vector); // 向量序列化

                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand("UPDATE Books SET Vector = @v WHERE BookId = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@v", vectorJson);
                    cmd.Parameters.AddWithValue("@id", book.BookId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #region 演算法計算

        /// <summary>
        /// 將向量陣列序列化為JSON字符串
        /// 用於資料庫存儲向量數據
        /// </summary>
        /// <param name="v">向量陣列</param>
        /// <returns>JSON格式的字符串</returns>
        public string Serialize(float[] v) => JsonConvert.SerializeObject(v);

        /// <summary>
        /// 將JSON字符串反序列化為向量陣列
        /// 從資料庫讀取向量數據時使用
        /// </summary>
        /// <param name="j">JSON格式的字符串</param>
        /// <returns>向量陣列</returns>
        public float[] UnpackVector(string j)
        {
            if (string.IsNullOrWhiteSpace(j)) {
                return new float[0];
            }

            try { 
                return JsonConvert.DeserializeObject<float[]>(j) ?? new float[0]; 
            }
            catch {
                return new float[0]; 
            }
        }

        /// <summary>
        /// 計算兩個向量的餘弦相似度
        /// 餘弦相似度公式：cos(θ) = (A·B) / (|A| × |B|)
        /// 值域：0到1，越接近1表示越相似
        /// </summary>
        /// <param name="a">第一個向量</param>
        /// <param name="b">第二個向量</param>
        /// <returns>相似度分數 (0-1)</returns>
        private double Cosine(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0) {
                return 0.0;
            }

            int n = Math.Min(a.Length, b.Length);
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < n; i++)
            {
                dot += a[i] * b[i];  // 點積累加
                na += a[i] * a[i];   // 向量A的模長平方
                nb += b[i] * b[i];   // 向量B的模長平方
            }
            return dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-8);  // 加小數避免除零
        }

        #endregion 演算法計算
    }
}
