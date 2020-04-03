using ConsoleTables;
using PremierLeague.Core;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.Entities;
using PremierLeague.Persistence;
using Serilog;
using System;
using System.Linq;
using Utils;

namespace PremierLeague.ImportConsole
{
    class Program
    {
        static void Main()
        {
            PrintHeader();
            InitData();
            AnalyzeData();

            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 60));

            Console.WriteLine(
                  @"
            _,...,_
          .'@/~~~\@'.          
         //~~\___/~~\\        P R E M I E R  L E A G U E 
        |@\__/@@@\__/@|             
        |@/  \@@@/  \@|            (inkl. Statistik)
         \\__/~~~\__//
          '.@\___/@.'
            `""""""
                ");

            Console.WriteLine(new String('-', 60));
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Importiert die Ergebnisse (csv-Datei >> Datenbank).
        /// </summary>
        private static void InitData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                Log.Information("Import der Spiele und Teams in die Datenbank");

                Log.Information("Datenbank löschen");
                // TODO: Datenbank löschen
                unitOfWork.DeleteDatabase();

                Log.Information("Datenbank migrieren");
                // TODO: Datenbank migrieren
                unitOfWork.MigrateDatabase();

                Log.Information("Spiele werden von premierleague.csv eingelesen");
                var games = ImportController.ReadFromCsv().ToArray();
                if (games.Length == 0)
                {
                    Log.Warning("!!! Es wurden keine Spiele eingelesen");
                }
                else
                {
                    Log.Debug($"  Es wurden {games.Count()} Spiele eingelesen!");

                    // TODO: Teams aus den Games ermitteln
                    var teams = games.Select(g => g.HomeTeam).Distinct();
                    Log.Debug($"  Es wurden {teams.Count()} Teams eingelesen!");

                    Log.Information("Daten werden in Datenbank gespeichert (in Context übertragen)");
                    unitOfWork.Games.AddRange(games);
                    unitOfWork.Teams.AddRange(teams);

                    // TODO: Teams/Games in der Datenbank speichern
                    unitOfWork.SaveChanges();
                    Log.Information("Daten wurden in DB gespeichert!");
                }
            }
        }

        private static void AnalyzeData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                var teamWithHighestGoals = unitOfWork.Teams.GetTeamWithBestTotalGoalCountAsNamedTuplet();
                PrintResult("Team mit den meisten geschossenen Toren:", $"{teamWithHighestGoals.Name}: {teamWithHighestGoals.GoalCount} Tore");
                Console.WriteLine();

                var teamWithMostGoalsAsGuest = unitOfWork.Teams.GetTeamsWithGoalStatisticsAsNamedTuplet().OrderByDescending(_ => _.TotalGoalsAsGuest).First();
                PrintResult("Team mit den meisten geschossenen Auswärtstoren:", $"{teamWithMostGoalsAsGuest.Name}: {teamWithMostGoalsAsGuest.TotalGoalsAsGuest} Auswärtstore");
                Console.WriteLine();

                var teamWithMostGoalsAtHome= unitOfWork.Teams.GetTeamsWithGoalStatisticsAsNamedTuplet().OrderByDescending(_ => _.TotalGoalsAtHome).First();
                PrintResult("Team mit den meisten geschossenen Heimtoren:", $"{teamWithMostGoalsAtHome.Name}: {teamWithMostGoalsAtHome.TotalGoalsAtHome} Heimtore");
                Console.WriteLine();

                var teamWithBestGoalDifference = unitOfWork.Teams.GetTeamsWithGoalStatisticsAsNamedTuplet().OrderByDescending(_ => _.OverallGoalDifference).First();
                PrintResult("Team mit dem besten Torverhältnis:", $"{teamWithBestGoalDifference.Name}: {teamWithBestGoalDifference.OverallGoalDifference} Torverhältnis");
                Console.WriteLine();

                var teamStatisticsAvg = unitOfWork.Teams.GetTeamStatisticDtos();
                string tmp = ConsoleTable
                    .From(teamStatisticsAvg)
                    .Configure(o => o.NumberAlignment = Alignment.Right)
                    .ToStringAlternative();
                PrintResult("Teamleistung im Durchschnitt (sortiert nach durchschnittlich geschossenen Toren pro Spiel):", tmp);
                Console.WriteLine();

                var teamTable = unitOfWork.Teams.GetTeamTableRowDtos();
                tmp = ConsoleTable
                    .From(teamTable)
                    .Configure(o => o.NumberAlignment = Alignment.Right)
                    .ToStringAlternative();
                PrintResult("Team Tabelle (sortiert nach Rang):", tmp);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Erstellt eine Konsolenausgabe
        /// </summary>
        /// <param name="caption">Enthält die Überschrift</param>
        /// <param name="result">Enthält das ermittelte Ergebnise</param>
        private static void PrintResult(string caption, string result)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(caption))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new String('=', caption.Length));
                Console.WriteLine(caption);
                Console.WriteLine(new String('=', caption.Length));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result);
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
