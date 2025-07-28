using System.ComponentModel.DataAnnotations.Schema;

namespace TestsAPiss.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")] // prevents precision loss, limits to 2 decimal places
        public decimal Purchase { get; set; }
        [Column(TypeName = "decimal(18,2)")] 
        public decimal LastDividend { get; set; }
        public string Industry { get; set; } = string.Empty;
        //[Column(TypeName = "decimal(18,2)")]
        public long MarketCap { get; set; } // whole value of the company..can be in Trillions, so we set long..not decimal

        public List<Comment> Comments { get; set; } = new List<Comment>();// a blog post can have many comments, but a comment can only belog to one blog Post..one parent many children..

    }
}
