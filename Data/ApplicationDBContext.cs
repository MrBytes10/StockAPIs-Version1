using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TestsAPiss.Models;

namespace TestsAPiss.Data
{
    public class ApplicationDBContext : DbContext
    {
        //public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        //{ 
        //    public DbSet<Stock> Stock{ get; set; }

        //}
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }
    
    }
}
    

