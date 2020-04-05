using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;

namespace PremierLeague.Core.Contracts
{
    public interface ITeamRepository
    {
        IEnumerable<Team> GetAllWithGames();
        IEnumerable<Team> GetAll();
        void AddRange(IEnumerable<Team> teams);
        Team Get(int teamId);
        void Add(Team team);
        (Team Team, int GoalCount)[] GetTeamsWithTotalGoalCountAsNamedTuplets();
        (Team Team, int GoalCount) GetTeamWithBestTotalGoalCountAsNamedTuplet();
        (Team Team, int TotalGoalsAtHome, int TotalGoalsAsGuest, int OverallGoalDifference)[] GetTeamsWithGoalStatisticsAsNamedTuplet();
        TeamStatisticDto[] GetTeamStatisticDtos();
        TeamTableRowDto[] GetTeamTableRowDtos();
    }
}