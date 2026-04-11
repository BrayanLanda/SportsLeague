using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<TeamService> _logger;

        public TeamService(ITeamRepository teamRepository, ILogger<TeamService> logger)
        {
            _teamRepository = teamRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Team>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all teams");
            return await _teamRepository.GetAllAsync();
        }

        public async Task<Team?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving team with ID: {TeamId}", id);
            var team = await _teamRepository.GetByIdAsync(id);

            if (team == null)
                _logger.LogWarning("Team with ID {TeamId} not found", id);

            return team;
        }

        public async Task<Team> CreateAsync(Team team)
        {
            var existingTeam = await _teamRepository.GetByNameAsync(team.Name);
            if (existingTeam != null)
            {
                _logger.LogWarning("Team with name {TeamName} already exists", team.Name);
                throw new InvalidOperationException($"Team with name '{team.Name}' already exists.");
            }
            _logger.LogInformation("Creating new team with name: {TeamName}", team.Name);
            return await _teamRepository.CreateAsync(team);
        }

        public async Task UpdateAsync(int id, Team team)
        {
            var existingTeam = await _teamRepository.GetByIdAsync(id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Team with ID {TeamId} not found for update", id);
                throw new KeyNotFoundException($"Team with ID '{id}' not found.");
            }

            if (existingTeam.Name != team.Name)
            {
                var teamWithSameName = await _teamRepository.GetByNameAsync(team.Name);
                if (teamWithSameName != null && teamWithSameName.Id != id)
                {
                    _logger.LogWarning("Another team with name {TeamName} already exists", team.Name);
                    throw new InvalidOperationException($"Another team with name '{team.Name}' already exists.");
                }
            }

            existingTeam.Name = team.Name;
            existingTeam.City = team.City;
            existingTeam.Stadium = team.Stadium;
            existingTeam.LogoUrl = team.LogoUrl;
            existingTeam.FoundedDate = team.FoundedDate;

            _logger.LogInformation("Updating team with ID: {TeamId}", id);
            await _teamRepository.UpdateAsync(existingTeam);
        }

        public async Task DeleteAsync(int id)
        {
            var existingTeam = await _teamRepository.GetByIdAsync(id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Team with ID {TeamId} not found for deletion", id);
                throw new KeyNotFoundException($"Team with ID '{id}' not found.");
            }

            _logger.LogInformation("Deleting team with ID: {TeamId}", id);
            await _teamRepository.DeleteAsync(id);
        }
    }
}