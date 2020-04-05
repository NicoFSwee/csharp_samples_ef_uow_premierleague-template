using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }

        public (Team Team, int GoalCount)[] GetTeamsWithTotalGoalCountAsNamedTuplets() =>
            _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                TotalGoalCount = t.AwayGames.Sum(_ => _.GuestGoals) + t.HomeGames.Sum(_ => _.HomeGoals)
            })
            .ToArray()
            .OrderByDescending(_ => _.TotalGoalCount)
            .Select(_ => (_.Team, _.TotalGoalCount))
            .ToArray();
        public (Team Team, int GoalCount) GetTeamWithBestTotalGoalCountAsNamedTuplet() => GetTeamsWithTotalGoalCountAsNamedTuplets().First();

        public (Team Team, int TotalGoalsAtHome, int TotalGoalsAsGuest, int OverallGoalDifference)[] GetTeamsWithGoalStatisticsAsNamedTuplet() =>
            _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                TotalGoalsAtHome = t.HomeGames.Sum(_ => _.HomeGoals),
                TotalGoalsAsGuests = t.AwayGames.Sum(_ => _.GuestGoals),
                OverallGoalDifference = t.HomeGames.Sum(_ => _.HomeGoals) + t.AwayGames.Sum(_ => _.GuestGoals) - t.HomeGames.Sum(_ => _.GuestGoals) - t.AwayGames.Sum(_ => _.HomeGoals)
            })
            .ToArray()
            .OrderByDescending(_ => _.Team.Name)
            .Select(_ => (_.Team, _.TotalGoalsAtHome, _.TotalGoalsAsGuests, _.OverallGoalDifference))
            .ToArray();

        public TeamStatisticDto[] GetTeamStatisticDtos() =>
            GetAllWithGames()
            .Select(t => new TeamStatisticDto
            {
                Name = t.Name,
                AvgGoalsShotAtHome = t.HomeGames.Average(_ => _.HomeGoals),
                AvgGoalsGotAtHome = t.HomeGames.Average(_ => _.GuestGoals),
                AvgGoalsShotOutwards = t.AwayGames.Average(_ => _.GuestGoals),
                AvgGoalsGotOutwards = t.AwayGames.Average(_ => _.HomeGoals),
                AvgGoalsShotInTotal = t.AwayGames.Select(g => g.GuestGoals).Concat(t.HomeGames.Select(g => g.HomeGoals)).Average(),
                AvgGoalsGotInTotal = t.AwayGames.Select(g => g.HomeGoals).Concat(t.HomeGames.Select(g => g.GuestGoals)).Average()
            })
            .OrderByDescending(_ => _.AvgGoalsShotInTotal)
            .ToArray();

        public TeamTableRowDto[] GetTeamTableRowDtos()
        {
            TeamTableRowDto[] result = GetAllWithGames()
                                        .Select(t => new TeamTableRowDto
                                        {
                                            Id = t.Id,
                                            Name = t.Name,
                                            Matches = t.HomeGames.Count() + t.AwayGames.Count(),
                                            Won = t.HomeGames.Where(g => g.HomeGoals > g.GuestGoals).Count() + t.AwayGames.Where(g => g.GuestGoals > g.HomeGoals).Count(),
                                            Lost = t.HomeGames.Where(g => g.HomeGoals < g.GuestGoals).Count() + t.AwayGames.Where(g => g.GuestGoals < g.HomeGoals).Count(),
                                            GoalsFor = t.AwayGames.Sum(_ => _.GuestGoals) + t.HomeGames.Sum(_ => _.HomeGoals),
                                            GoalsAgainst = t.AwayGames.Sum(_ => _.HomeGoals) + t.HomeGames.Sum(_ => _.GuestGoals)
                                        })
                                        .OrderByDescending(_ => _.Points)
                                        .ThenByDescending(_ => _.GoalDifference)
                                        .ToArray();

            foreach(var dto in result)
            {
                dto.Rank = result.ToList().IndexOf(dto) + 1;
            }

            return result;
        }
    }
}