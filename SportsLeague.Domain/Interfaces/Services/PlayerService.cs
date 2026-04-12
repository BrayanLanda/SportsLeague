using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(
        IPlayerRepository playerRepository,
        ITeamRepository teamRepository,
        ILogger<PlayerService> logger)
        {
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all players");
            return await _playerRepository.GetAllWithTeamAsync();
        }

        public async Task<Player?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving player with ID: {PlayerId}", id);
            var player = await _playerRepository.GetByIdWithTeamAsync(id);

            if (player == null)
                _logger.LogWarning("Player with ID {PlayerId} not found", id);

            return player;
        }

        public async Task<IEnumerable<Player>> GetByTeamAsync(int teamId)
        {
            var teamExists = await _teamRepository.ExistsAsync(teamId);
            if (!teamExists)
            {
                _logger.LogWarning("Team with ID {TeamId} not found", teamId);
                throw new KeyNotFoundException($"Team with ID {teamId} not found");
            }

            _logger.LogInformation("Retrieving players for team ID: {TeamId}", teamId);
            return await _playerRepository.GetByTeamAsync(teamId);
        }

        public async Task<Player> CreateAsync(Player player)
        {
            var teamExists = await _teamRepository.ExistsAsync(player.TeamId);
            if (!teamExists)
            {
                _logger.LogWarning("Team with ID {TeamId} not found", player.TeamId);
                throw new KeyNotFoundException($"Team with ID {player.TeamId} not found");
            }

            var existingPlayer = await _playerRepository.GetByTeamAndNumberAsync(player.TeamId, player.Number);
            if (existingPlayer != null)
            {
                _logger.LogWarning("Player with number {PlayerNumber} already exists in team ID {TeamId}", player.Number, player.TeamId);
                throw new InvalidOperationException($"Player with number {player.Number} already exists in team ID {player.TeamId}");
            }

            _logger.LogInformation("Creating new player: {PlayerName}", player.LastName);
            return await _playerRepository.CreateAsync(player);
        }

        public async Task UpdateAsync(int id, Player player)
        {
            var existingPlayer = await _playerRepository.GetByIdAsync(id);
            if (existingPlayer == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found", id);
                throw new KeyNotFoundException($"Player with ID {id} not found");
            }

            var teamExists = await _teamRepository.ExistsAsync(player.TeamId);
            if (!teamExists)
            {
                _logger.LogWarning("Team with ID {TeamId} not found", player.TeamId);
                throw new KeyNotFoundException($"Team with ID {player.TeamId} not found");
            }

            if (existingPlayer.Number != player.Number ||
            existingPlayer.TeamId != player.TeamId)
            {
                var conflict = await _playerRepository
                    .GetByTeamAndNumberAsync(player.TeamId, player.Number);
                if (conflict != null && conflict.Id != id)
                {
                    _logger.LogWarning("Player with number {PlayerNumber} already exists in team ID {TeamId}", player.Number, player.TeamId);
                    throw new InvalidOperationException($"Player with number {player.Number} already exists in team ID {player.TeamId}");
                }
            }

            existingPlayer.FirstName = player.FirstName;
            existingPlayer.LastName = player.LastName;
            existingPlayer.BirthDate = player.BirthDate;
            existingPlayer.Number = player.Number;
            existingPlayer.Position = player.Position;
            existingPlayer.TeamId = player.TeamId;

            _logger.LogInformation("Updating player with ID: {PlayerId}", id);
            await _playerRepository.UpdateAsync(existingPlayer);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _playerRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(
                    $"Player with ID {id} not found");
            }

            _logger.LogInformation("Deleting player with ID: {PlayerId}", id);
            await _playerRepository.DeleteAsync(id);
        }
    }
}