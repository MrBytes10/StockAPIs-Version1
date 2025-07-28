namespace TestsAPiss.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public int? StockId { get; set; } // this is the foreign key to the Stock table..making the relationship// also called Navigation Property
        public Stock? Stock { get; set; }

    }
}
