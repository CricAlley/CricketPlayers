using CricketPlayers.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CricketPlayers.Data
{
    public class CricketPlayersContext : DbContext
    {
        public CricketPlayersContext(DbContextOptions<CricketPlayersContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Player { get; set; }
    }
}
