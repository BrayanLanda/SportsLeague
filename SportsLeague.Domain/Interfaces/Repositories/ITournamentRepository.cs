using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface ITournamentRepository : IGenericRepository<Tournament>
    {
        Task<IEnumerable<Tournament>> GetByStatusAsync(TournamentStatus status);
        Task<Tournament?> GetByIdWithTeamsAsync(int id);
    }
}