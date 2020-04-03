using System;
using System.Collections.Generic;
using System.Linq;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        const string Filename = "PremierLeague.csv";
        const int ROUND_IDX = 0;
        const int HOMETEAM_IDX = 1;
        const int GUESTTEAM_IDX = 2;
        const int HOMETEAM_GOALS_IDX = 3;
        const int GUESTTEAM_GOALS_IDX = 4;
        public static IEnumerable<Game> ReadFromCsv()
        {
            string[][] data = MyFile.ReadStringMatrixFromCsv(Filename, false);

            var teams = data.GroupBy(d => d[HOMETEAM_IDX])
                            .Select(d => new Team
                            {
                                Name = d.Key,
                                HomeGames = new List<Game>(),
                                AwayGames = new List<Game>()
                            })
                            .ToDictionary(_ => _.Name);

            var games = data.Select(d => new Game
                            {
                                Round = int.Parse(d[ROUND_IDX]),
                                HomeTeam = teams[d[HOMETEAM_IDX]],
                                GuestTeam = teams[d[GUESTTEAM_IDX]],
                                HomeGoals = int.Parse(d[HOMETEAM_GOALS_IDX]),
                                GuestGoals = int.Parse(d[GUESTTEAM_GOALS_IDX])
                            })
                            .ToArray();

            return games;
        }

    }
}
