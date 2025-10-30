namespace BookVectorMVC.Models
{
    public class SearchResult
    {
        // 書籍
        public Book Book { get; set; }
        // 分數
        public double Score { get; set; }
        // 排名
        public int Rank { get; set; }
    }
}
