using CsvHelper.Configuration;
using System.Collections.Generic;

namespace CricketPlayersExcelIngestor
{
    public class PlayerRegistry
    {
        public string Identifier { get; set; }
        public string Names { get; set; }        
        public string UniqueName { get; set; }
        public string CricInfoId { get; set; }
    }

    public class PlayerRegistryMap : ClassMap<PlayerRegistry>
    {
        public PlayerRegistryMap()
        {
            Map(m => m.Identifier).Index(0);
            Map(m => m.Names).Index(1);
            Map(m => m.UniqueName).Index(2);
            Map(m => m.CricInfoId).Index(6);
        }
    }
}