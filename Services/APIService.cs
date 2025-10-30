using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BookVectorMVC.Services
{
    public class APIService
    {
        private static string _apiKey = "jina_2db4e931501d4a83a0fa469ffc715253CEFZ3J2fSkdHxIHnCRoe2dRiNbZe";
        private static string _url = "https://api.jina.ai/v1/embeddings";
        public int VectorDimension => 1024; // 固定維度，"jina-embeddings-v3" 是採用1024維

        /// <summary>
        /// 取得文字的語意向量（使用 Jina Embeddings v3）
        /// 同步呼叫方式，方便 .NET Framework MVC 使用
        /// </summary>
        /// <param name="text">輸入文字</param>
        /// <returns>浮點向量陣列</returns>
        public float[] GetEmbedding(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) {
                return new float[0]; 
            }

            try
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
            catch (Exception ex)
            {
                Console.WriteLine("[Jina Error] " + ex.Message);
                return new float[0];
            }
        }
    }
}
