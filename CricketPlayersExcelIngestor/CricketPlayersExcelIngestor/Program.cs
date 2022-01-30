﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CsvHelper;
using IronXL;

namespace CricketPlayersExcelIngestor
{
    class Program
    {
        private const string peopleFile = "people.csv";
        private const string namesFile = "names.csv";
        public static DateTime minDate = new DateTime(1950,01,01);
        static void Main(string[] args)
        {
            // DumpDataInFile();

            StorePlayerRegistry();
        }

        private static void StorePlayerRegistry()
        {
            WebClient wc = new WebClient();
            using (var stream = new MemoryStream(wc.DownloadData("https://cricsheet.org/register/people.csv")))
            {
                using var reader = new StreamReader(stream);
                using var peopleCsv = new CsvReader(reader, CultureInfo.InvariantCulture);
                peopleCsv.Context.RegisterClassMap<PlayerRegistryMap>();

                using var ms = new MemoryStream(wc.DownloadData("https://cricsheet.org/register/names.csv"));

                using var r = new StreamReader(ms);
                using var nameCsv = new CsvReader(new StreamReader(ms), CultureInfo.InvariantCulture);
                nameCsv.Context.RegisterClassMap<NamesMap>();

                var names = nameCsv.GetRecords<Names>().ToList();
                var people = peopleCsv.GetRecords<PlayerRegistry>().ToList();

                Console.WriteLine("Excel Data Loaded. Generating Merge Script");
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("IF OBJECT_ID('tempdb..#playerRegister') IS NOT NULL");
                stringBuilder.AppendLine("  DROP TABLE #playerRegister");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("CREATE TABLE #playerRegister");
                stringBuilder.AppendLine("(");
                stringBuilder.AppendLine("  [Id]            INT             IDENTITY(1, 1) NOT NULL,");
                stringBuilder.AppendLine("  [Identifier]    VARCHAR(100)   NOT NULL,");
                stringBuilder.AppendLine("  [UniqueName]      VARCHAR(100)   NOT NULL,");
                stringBuilder.AppendLine("  [Names]   VARCHAR(1000)   NULL,");
                stringBuilder.AppendLine("  [CricInfoId]    INT             NOT NULL,");
                stringBuilder.AppendLine("  [IsActive]      BIT             DEFAULT((1)),");
                stringBuilder.AppendLine(");");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(
                    "INSERT INTO #playerRegister	([Identifier], [UniqueName], [Names], [CricInfoId], [IsActive])");


                var lastPlayerIndex = people.Count() - 1;

                for (var index = 0; index <= lastPlayerIndex; index++)
                {
                    var person = people[index];

                    var canSplitBatch = index % 1000 == 0;

                    if (string.IsNullOrEmpty(person.CricInfoId))
                    {
                        continue;
                    }

                    person.Names = string.Join(", ", names.Where(n => n.Identifier.Equals(person.Identifier)).Select(n => n.Name));
                    if (canSplitBatch)
                    {
                        stringBuilder.AppendLine(
                               $@"SELECT '{person.Identifier}' AS Identifier, '{person.UniqueName.Replace("'", "''")}' AS UniqueName, '{person.Names.Replace("'", "''")}' AS Names," +
                               $@" '{person.CricInfoId}' AS CricInfoId, 1 AS IsActive");

                        stringBuilder.AppendLine("Go");

                        stringBuilder.AppendLine();

                        stringBuilder.AppendLine(
                        "INSERT INTO #playerRegister	([Identifier], [UniqueName], [Names], [CricInfoId], [IsActive])");

                    }
                    else if (index == lastPlayerIndex)
                    {
                        stringBuilder.AppendLine(
                               $@"SELECT '{person.Identifier}' AS Identifier, '{person.UniqueName.Replace("'", "''")}' AS UniqueName, '{person.Names.Replace("'", "''")}' AS Names," +
                               $@" '{person.CricInfoId}' AS CricInfoId, 1 AS IsActive");
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                               $@"SELECT '{person.Identifier}' AS Identifier, '{person.UniqueName.Replace("'", "''")}' AS UniqueName, '{person.Names.Replace("'", "''")}' AS Names," +
                               $@" '{person.CricInfoId}' AS CricInfoId, 1 AS IsActive UNION ALL");
                    }

                }

                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("BEGIN TRY");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("  BEGIN TRANSACTION");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("      MERGE  dbo.PlayerRegister AS TARGET");
                stringBuilder.AppendLine("          USING #playerRegister AS SOURCE");
                stringBuilder.AppendLine("          ON(TARGET.Identifier = SOURCE.Identifier)");
                stringBuilder.AppendLine("              WHEN MATCHED AND TARGET.CricInfoId <> SOURCE.CricInfoId");
                stringBuilder.AppendLine("              THEN");
                stringBuilder.AppendLine("                  UPDATE");
                stringBuilder.AppendLine("                  SET TARGET.CricInfoId = SOURCE.CricInfoId");
                stringBuilder.AppendLine("              WHEN NOT MATCHED BY TARGET");
                stringBuilder.AppendLine("              THEN");
                stringBuilder.AppendLine("                  INSERT(Identifier");
                stringBuilder.AppendLine("                        , UniqueName");
                stringBuilder.AppendLine("                        , Names");
                stringBuilder.AppendLine("                        , CricInfoId");
                stringBuilder.AppendLine("                        , IsActive)");
                stringBuilder.AppendLine("                  VALUES(SOURCE.Identifier");
                stringBuilder.AppendLine("                        , SOURCE.UniqueName");
                stringBuilder.AppendLine("                        , SOURCE.Names");
                stringBuilder.AppendLine("                        , SOURCE.CricInfoId");
                stringBuilder.AppendLine("                        , SOURCE.IsActive);");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("  COMMIT TRANSACTION");
                stringBuilder.AppendLine("PRINT 'MERGE dbo.playerRegister - Done'");
                stringBuilder.AppendLine("END TRY");
                stringBuilder.AppendLine("BEGIN CATCH");
                stringBuilder.AppendLine("      ROLLBACK TRANSACTION;");
                stringBuilder.AppendLine("      THROW");
                stringBuilder.AppendLine("END CATCH");

                File.WriteAllText("PlayerRegister.sql", stringBuilder.ToString());

                Console.WriteLine("playerRegister script is generated.");
            }               
        }

        private static void DumpDataInFile()
        {
            var fileName =
                @"D:\Users\NShekhwat\source\Repos\naru1790\CricketPlayers\CricketPlayersExcelIngestor\CricketPlayersExcelIngestor\CricketPlayers.xlsx";

            Console.WriteLine("Loading Data from Excel.");


            var workbook = WorkBook.Load(fileName);
            var sheet = workbook.WorkSheets.First();

            Console.WriteLine("Excel Data Loaded. Generating Merge Script");
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("IF OBJECT_ID('tempdb..#players') IS NOT NULL");
            stringBuilder.AppendLine("  DROP TABLE #players");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("CREATE TABLE #players");
            stringBuilder.AppendLine("(");
            stringBuilder.AppendLine("  [Id]            INT             IDENTITY(1, 1) NOT NULL,");
            stringBuilder.AppendLine("  [Name]          NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine("  [FullName]      NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine("  [PlayingRole]   NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine("  [DateOfBirth]   DATETIME        NULL,");
            stringBuilder.AppendLine("  [BattingStyle]  NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine("  [BowlingStyle]  NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine("  [CricInfoId]    INT             NOT NULL,");
            stringBuilder.AppendLine("  [IsActive]      BIT             DEFAULT((1)),");
            stringBuilder.AppendLine("  [CricsheetName] NVARCHAR(MAX)   NULL,");
            stringBuilder.AppendLine(");");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(
                "INSERT INTO #players	([Name], [FullName], [PlayingRole], [DateOfBirth], [BattingStyle], [BowlingStyle], [CricInfoId], [IsActive], [CricsheetName])");


            var players = sheet.Rows.Where(row =>
            {

                var birthDate = row.Columns[(int)ExcelColumns.BirthDate].DateTimeValue;
                var isDead = row.Columns[(int)ExcelColumns.Died].StringValue == "Dead";
                return GetPlayingRole(row) != PlayingRole.None && (!birthDate.HasValue || birthDate.Value > minDate) &&
                       !isDead;
            }).Select(row => new Player1
            {
                CricInfoId = row.Columns[(int)ExcelColumns.Id].Int32Value,
                BattingStyle = row.Columns[(int)ExcelColumns.BattingStyle].StringValue,
                BowlingStyle = row.Columns[(int)ExcelColumns.BowlingStyle].StringValue,
                DateOfBirth = row.Columns[(int)ExcelColumns.BirthDate].DateTimeValue,
                FullName = row.Columns[(int)ExcelColumns.FullName].StringValue,
                Name = row.Columns[(int)ExcelColumns.Name].StringValue,
                PlayingRole = GetPlayingRole(row).ToString()
            }).ToArray();

            Console.WriteLine($"Where condition executed. {players.Length} players will be added to the script.");

            var lastPlayerIndex = players.Length - 1;

            for (var index = 0; index < players.Length; index++)
            {
                var player = players[index];

                var canSplitBatch = index % 1000 == 0;
                if (canSplitBatch)
                {
                    if (player.DateOfBirth == null)
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, NULL AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName");
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, '{player.DateOfBirth}' AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName");
                    }

                    stringBuilder.AppendLine("Go");

                    stringBuilder.AppendLine();

                    stringBuilder.AppendLine(
                        "INSERT INTO #players	([Name], [FullName], [PlayingRole], [DateOfBirth], [BattingStyle], [BowlingStyle], [CricInfoId], [IsActive], [CricsheetName])");
                }
                else if (index == lastPlayerIndex)
                {
                    if (player.DateOfBirth == null)
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, NULL AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName");
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, '{player.DateOfBirth}' AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName");
                    }
                }
                else
                {
                    if (player.DateOfBirth == null)
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, NULL AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName UNION ALL");
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $@"SELECT '{player.Name.Replace("'", "''")}' AS Name, '{player.FullName.Replace("'", "''")}' AS FullName, '{player.PlayingRole}' AS PlayingRole, '{player.DateOfBirth}' AS DateofBirth," +
                            $@" '{player.BattingStyle}' AS BattingStyle, '{player.BowlingStyle}' AS BowlingStyle, '{player.CricInfoId}' AS CricInfoId, 1 AS IsActive, NULL AS CricsheetName UNION ALL");
                    }
                }
            }

            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("BEGIN TRY");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("  BEGIN TRANSACTION");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("      MERGE  dbo.players AS TARGET");
            stringBuilder.AppendLine("          USING #players AS SOURCE");
            stringBuilder.AppendLine("          ON(TARGET.CricInfoId = SOURCE.CricInfoId)");
            stringBuilder.AppendLine("              WHEN MATCHED AND TARGET.PlayingRole <> SOURCE.PlayingRole");
            stringBuilder.AppendLine("              THEN");
            stringBuilder.AppendLine("                  UPDATE");
            stringBuilder.AppendLine("                  SET TARGET.PlayingRole = SOURCE.PlayingRole");
            stringBuilder.AppendLine("              WHEN NOT MATCHED BY TARGET");
            stringBuilder.AppendLine("              THEN");
            stringBuilder.AppendLine("                  INSERT(Name");
            stringBuilder.AppendLine("                        , FullName");
            stringBuilder.AppendLine("                        , PlayingRole");
            stringBuilder.AppendLine("                        , DateOfBirth");
            stringBuilder.AppendLine("                        , BattingStyle");
            stringBuilder.AppendLine("                        , BowlingStyle");
            stringBuilder.AppendLine("                        , CricInfoId");
            stringBuilder.AppendLine("                        , IsActive");
            stringBuilder.AppendLine("                        , CricsheetName)");
            stringBuilder.AppendLine("                  VALUES(SOURCE.Name");
            stringBuilder.AppendLine("                        , SOURCE.FullName");
            stringBuilder.AppendLine("                        , SOURCE.PlayingRole");
            stringBuilder.AppendLine("                        , SOURCE.DateOfBirth");
            stringBuilder.AppendLine("                        , SOURCE.BattingStyle");
            stringBuilder.AppendLine("                        , SOURCE.BowlingStyle");
            stringBuilder.AppendLine("                        , SOURCE.CricInfoId");
            stringBuilder.AppendLine("                        , SOURCE.IsActive");
            stringBuilder.AppendLine("                        , SOURCE.CricsheetName);");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("  COMMIT TRANSACTION");
            stringBuilder.AppendLine("PRINT 'MERGE dbo.Player - Done'");
            stringBuilder.AppendLine("END TRY");
            stringBuilder.AppendLine("BEGIN CATCH");
            stringBuilder.AppendLine("      ROLLBACK TRANSACTION;");
            stringBuilder.AppendLine("      THROW");
            stringBuilder.AppendLine("END CATCH");

            File.WriteAllText("PlayersData.sql", stringBuilder.ToString());

            Console.WriteLine("Player Data script is generated.");
        }

        private static PlayingRole GetPlayingRole(RangeRow row)
        {
            var isBatsman = IsBatsman(row);

            var isBowler = IsBowler(row);

            var isWicketKeeper = IsWicketKeeper(row);

            PlayingRole role = PlayingRole.None;

            if (isBatsman)
            {
                role = PlayingRole.Batsman;

                if (isBowler)
                {
                    role = PlayingRole.AllRounder;
                }
            }

            if (isWicketKeeper && !isBowler)
            {
                role = PlayingRole.Wicketkeeper;
            }

            if (isBowler && !isBatsman  && !isWicketKeeper)
            {
                role = PlayingRole.Bowler;
            }

            if (isWicketKeeper && isBowler)
            {
                var OdiStumpings = row.Columns[(int)ExcelColumns.BATTING_ODIs_St].Int32Value;
                var T20iStumpings = row.Columns[(int)ExcelColumns.BATTING_T20Is_St].Int32Value;
                var T20Stumpings = row.Columns[(int)ExcelColumns.BATTING_T20s_St].Int32Value;

                var OdiWkts = row.Columns[(int)ExcelColumns.BOWLING_ODIs_Wkts].Int32Value;
                var T20IWkts = row.Columns[(int)ExcelColumns.BOWLING_T20Is_Wkts].Int32Value;
                var T20Wkts= row.Columns[(int)ExcelColumns.BOWLING_T20s_Wkts].Int32Value;

                isWicketKeeper = OdiStumpings + T20iStumpings + T20Stumpings > OdiWkts + T20IWkts + T20Wkts;

                role = isWicketKeeper ? PlayingRole.Wicketkeeper : PlayingRole.Bowler;
            }

            return role;
        }

        private static bool IsWicketKeeper(RangeRow row)
        {
            var OdiStumpings = row.Columns[(int)ExcelColumns.BATTING_ODIs_St].Int32Value;
            var T20iStumpings = row.Columns[(int)ExcelColumns.BATTING_T20Is_St].Int32Value;
            var T20Stumpings = row.Columns[(int)ExcelColumns.BATTING_T20s_St].Int32Value;

            return OdiStumpings > 0 || T20iStumpings > 0 || T20Stumpings > 0;
        }

        private static bool IsBowler(RangeRow row)
        {
            var OdiBalls = row.Columns[(int)ExcelColumns.BOWLING_ODIs_Balls].Int32Value;
            var T20IBalls = row.Columns[(int)ExcelColumns.BOWLING_T20Is_Balls].Int32Value;
            var T20Balls = row.Columns[(int)ExcelColumns.BOWLING_T20s_Balls].Int32Value;

            var OdiWkts = row.Columns[(int)ExcelColumns.BOWLING_ODIs_Wkts].Int32Value;
            var T20IWkts = row.Columns[(int)ExcelColumns.BOWLING_T20Is_Wkts].Int32Value;
            var T20Wkts = row.Columns[(int)ExcelColumns.BOWLING_T20s_Wkts].Int32Value;

            return OdiBalls > 0 || T20Balls > 0 || T20IBalls > 0 || OdiWkts > 0 || T20Wkts >0 || T20IWkts > 0;
        }

        private static bool IsBatsman(RangeRow row)
        {
            var OdiBattingInnings = row.Columns[(int) ExcelColumns.BATTING_ODIs_Inns].Int32Value;
            var T20BattingInnings = row.Columns[(int) ExcelColumns.BATTING_T20s_Inns].Int32Value;
            var T20IBattingInnings = row.Columns[(int) ExcelColumns.BATTING_T20Is_Inns].Int32Value;
            var Odi50s = row.Columns[(int)ExcelColumns.BATTING_ODIs_50].Int32Value;
            var T2050s = row.Columns[(int)ExcelColumns.BATTING_T20s_50].Int32Value;
            var T20I50s = row.Columns[(int)ExcelColumns.BATTING_T20Is_50].Int32Value;


            var isOdiBatsman = OdiBattingInnings > 0 || Odi50s > 0;
            var isT20Batsman = T20BattingInnings > 0 || T2050s > 0;
            var isT20IBatsman = T20IBattingInnings > 0 || T20I50s > 0;

            return isT20IBatsman || isT20Batsman || isOdiBatsman;
        }
    }

    public enum PlayingRole
    {
        None,
        Batsman,
        Bowler,
        AllRounder,
        Wicketkeeper
    }

    public enum ExcelColumns
    {
        RowNumber,
        Id,
        Name,
        Country,
        FullName,
        BirthDate,
        BirthPlace,
        Died,
        DateOfDeath,
        Age,
        MajorTeams,
        BattingStyle,
        BowlingStyle,
        Other,
        Awards,
        BATTING_Tests_Mat,
        BATTING_Tests_Inns,
        BATTING_Tests_NO,
        BATTING_Tests_Runs,
        BATTING_Tests_HS,
        BATTING_Tests_Ave,
        BATTING_Tests_BF,
        BATTING_Tests_SR,
        BATTING_Tests_100,
        BATTING_Tests_50,
        BATTING_Tests_4s,
        BATTING_Tests_6s,
        BATTING_Tests_Ct,
        BATTING_Tests_St,
        BATTING_ODIs_Mat,
        BATTING_ODIs_Inns,
        BATTING_ODIs_NO,
        BATTING_ODIs_Runs,
        BATTING_ODIs_HS,
        BATTING_ODIs_Ave,
        BATTING_ODIs_BF,
        BATTING_ODIs_SR,
        BATTING_ODIs_100,
        BATTING_ODIs_50,
        BATTING_ODIs_4s,
        BATTING_ODIs_6s,
        BATTING_ODIs_Ct,
        BATTING_ODIs_St,
        BATTING_T20Is_Mat,
        BATTING_T20Is_Inns,
        BATTING_T20Is_NO,
        BATTING_T20Is_Runs,
        BATTING_T20Is_HS,
        BATTING_T20Is_Ave,
        BATTING_T20Is_BF,
        BATTING_T20Is_SR,
        BATTING_T20Is_100,
        BATTING_T20Is_50,
        BATTING_T20Is_4s,
        BATTING_T20Is_6s,
        BATTING_T20Is_Ct,
        BATTING_T20Is_St,
        BATTING_First_class_Mat,
        BATTING_First_class_Inns,
        BATTING_First_class_NO,
        BATTING_First_class_Runs,
        BATTING_First_class_HS,
        BATTING_First_class_Ave,
        BATTING_First_class_BF,
        BATTING_First_class_SR,
        BATTING_First_class_100,
        BATTING_First_class_50,
        BATTING_First_class_4s,
        BATTING_First_class_6s,
        BATTING_First_class_Ct,
        BATTING_First_class_St,
        BATTING_List_A_Mat,
        BATTING_List_A_Inns,
        BATTING_List_A_NO,
        BATTING_List_A_Runs,
        BATTING_List_A_HS,
        BATTING_List_A_Ave,
        BATTING_List_A_BF,
        BATTING_List_A_SR,
        BATTING_List_A_100,
        BATTING_List_A_50,
        BATTING_List_A_4s,
        BATTING_List_A_6s,
        BATTING_List_A_Ct,
        BATTING_List_A_St,
        BATTING_T20s_Mat,
        BATTING_T20s_Inns,
        BATTING_T20s_NO,
        BATTING_T20s_Runs,
        BATTING_T20s_HS,
        BATTING_T20s_Ave,
        BATTING_T20s_BF,
        BATTING_T20s_SR,
        BATTING_T20s_100,
        BATTING_T20s_50,
        BATTING_T20s_4s,
        BATTING_T20s_6s,
        BATTING_T20s_Ct,
        BATTING_T20s_St,
        BOWLING_Tests_Mat,
        BOWLING_Tests_Inns,
        BOWLING_Tests_Balls,
        BOWLING_Tests_Runs,
        BOWLING_Tests_Wkts,
        BOWLING_Tests_BBI,
        BOWLING_Tests_BBM,
        BOWLING_Tests_Ave,
        BOWLING_Tests_Econ,
        BOWLING_Tests_SR,
        BOWLING_Tests_4w,
        BOWLING_Tests_5w,
        BOWLING_Tests_10,
        BOWLING_ODIs_Mat,
        BOWLING_ODIs_Inns,
        BOWLING_ODIs_Balls,
        BOWLING_ODIs_Runs,
        BOWLING_ODIs_Wkts,
        BOWLING_ODIs_BBI,
        BOWLING_ODIs_BBM,
        BOWLING_ODIs_Ave,
        BOWLING_ODIs_Econ,
        BOWLING_ODIs_SR,
        BOWLING_ODIs_4w,
        BOWLING_ODIs_5w,
        BOWLING_ODIs_10,
        BOWLING_T20Is_Mat,
        BOWLING_T20Is_Inns,
        BOWLING_T20Is_Balls,
        BOWLING_T20Is_Runs,
        BOWLING_T20Is_Wkts,
        BOWLING_T20Is_BBI,
        BOWLING_T20Is_BBM,
        BOWLING_T20Is_Ave,
        BOWLING_T20Is_Econ,
        BOWLING_T20Is_SR,
        BOWLING_T20Is_4w,
        BOWLING_T20Is_5w,
        BOWLING_T20Is_10,
        BOWLING_First_class_Mat,
        BOWLING_First_class_Inns,
        BOWLING_First_class_Balls,
        BOWLING_First_class_Runs,
        BOWLING_First_class_Wkts,
        BOWLING_First_class_BBI,
        BOWLING_First_class_BBM,
        BOWLING_First_class_Ave,
        BOWLING_First_class_Econ,
        BOWLING_First_class_SR,
        BOWLING_First_class_4w,
        BOWLING_First_class_5w,
        BOWLING_First_class_10,
        BOWLING_List_A_Mat,
        BOWLING_List_A_Inns,
        BOWLING_List_A_Balls,
        BOWLING_List_A_Runs,
        BOWLING_List_A_Wkts,
        BOWLING_List_A_BBI,
        BOWLING_List_A_BBM,
        BOWLING_List_A_Ave,
        BOWLING_List_A_Econ,
        BOWLING_List_A_SR,
        BOWLING_List_A_4w,
        BOWLING_List_A_5w,
        BOWLING_List_A_10,
        BOWLING_T20s_Mat,
        BOWLING_T20s_Inns,
        BOWLING_T20s_Balls,
        BOWLING_T20s_Runs,
        BOWLING_T20s_Wkts,
        BOWLING_T20s_BBI,
        BOWLING_T20s_BBM,
        BOWLING_T20s_Ave,
        BOWLING_T20s_Econ,
        BOWLING_T20s_SR,
        BOWLING_T20s_4w,
        BOWLING_T20s_5w,
        BOWLING_T20s_10
    }   
}
