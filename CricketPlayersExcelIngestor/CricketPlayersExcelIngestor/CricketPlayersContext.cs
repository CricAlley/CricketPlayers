using Microsoft.EntityFrameworkCore;

namespace CricketPlayersExcelIngestor
{
    public class CricketPlayersContext : DbContext
    {
        public DbSet<PlayerRegistry> Player { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data Source=LAPTOP-3AQTASAE\SQLEXPRESS;Initial Catalog=CricketPlayers;Integrated Security=True");
        }
    }
}
