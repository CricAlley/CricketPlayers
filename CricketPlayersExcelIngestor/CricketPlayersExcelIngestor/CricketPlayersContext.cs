using Microsoft.EntityFrameworkCore;

namespace CricketPlayersExcelIngestor
{
    public class CricketPlayersContext : DbContext
    {
        public DbSet<Player> Player { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\Projects\CricketPlayers\CricketPlayers.Data\Player.mdf;Integrated Security=True;Connect Timeout=30");
        }
    }
}
