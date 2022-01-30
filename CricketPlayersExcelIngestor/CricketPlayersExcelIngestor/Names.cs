using CsvHelper.Configuration;

namespace CricketPlayersExcelIngestor
{
    public class Names
    {
        public string Identifier { get;  set; }
        public string Name { get;  set; }
    }

    public class NamesMap : ClassMap<Names>
    {
        public NamesMap()
        {
            Map(m => m.Identifier).Index(0);
            Map(m => m.Name).Index(1);
        }
    }
}