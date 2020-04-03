using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;

namespace PremierLeague.Core.Contracts
{
    public interface IGameRepository
    {
        void AddRange(IEnumerable<Game> games);
        IEnumerable<Game> GetAllWithTeams();
        void Add(Game game);
    }
}