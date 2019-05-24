using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CricketPlayers.Models
{
    public class CricketPlayersContext : DbContext
    {
        public CricketPlayersContext (DbContextOptions<CricketPlayersContext> options)
            : base(options)
        {
        }

        public DbSet<CricketPlayers.Models.Player> Player { get; set; }
    }
}
