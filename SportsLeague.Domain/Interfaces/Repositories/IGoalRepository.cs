using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface IGoalRepository : IGenericRepository<Goal>
    {
        Task<IEnumerable<Goal>> GetByMatchIdAsync(int matchId);
        Task<IEnumerable<Goal>> GetByMatchWithDetailsAsync(int matchId);
    }
}