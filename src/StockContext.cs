using Microsoft.EntityFrameworkCore;

namespace app
{
    using Models;

    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options) {}

        // 提供存取資料表的欄位
        public DbSet<Stock> Stocks { get;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // - 配置資料表結構在程式中對應的類別
            //   Entity<Stock>()
            // - 配置欄位與資料表的對應關係
            //   ToTable("stocks")
            modelBuilder.Entity<Stock>().ToTable("stocks");
        }
    }
}