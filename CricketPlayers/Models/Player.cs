using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CricketPlayers.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string PlayingRole { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string BattingStyle { get; set; }
        public string BowlingStyle { get; set; }
        public int CricInfoId { get; set; }
    }

    public enum PlayingRole
    {
        TopOrderBatsman,
        MiddleOrderBatsman,
        Finisher,
        Batsman,
        Bowler,
        DeathBowler,
        AllRounder,
        Wicketkeeper,
        WicketkeeperTopOrderBatsman,
        WicketkeeperMiddleOrderBatsman,
        WicketkeeperFinisher
    }
}
