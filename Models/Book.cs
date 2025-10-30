namespace BookVectorMVC.Models
{
    public class Book
    {
        // Identity
        public int BookId { get; set; }
        // 書名
        public string Title { get; set; }
        // 描述
        public string Description { get; set; }
        // 位置
        public string Position { get; set; }
        // 向量位置
        public string Vector { get; set; } // JSON string
    }
}
