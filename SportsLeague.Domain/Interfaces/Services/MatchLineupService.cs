using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class MatchLineupService : IMatchLineupService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IMatchLineupRepository _matchLineupRepository;
        private readonly MatchValidationHelper _validationHelper;
        private readonly ILogger<MatchLineupService> _logger;

        public MatchLineupService(
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            IMatchLineupRepository matchLineupRepository,
            MatchValidationHelper validationHelper,
            ILogger<MatchLineupService> logger)
        {
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _matchLineupRepository = matchLineupRepository;
            _validationHelper = validationHelper;
            _logger = logger;
        }

        public async Task<MatchLineup> RegisterPlayerAsync(int matchId, MatchLineup lineup)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

            if (match.Status != MatchStatus.Scheduled)
                throw new InvalidOperationException(
                    "Solo se pueden registrar alineaciones en partidos Scheduled");

            var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);
            if (player == null)
                throw new KeyNotFoundException(
                    $"No se encontró el jugador con ID {lineup.PlayerId}");

            await _validationHelper.ValidatePlayerInMatchAsync(lineup.PlayerId, match);

            var alreadyExists = await _matchLineupRepository
                .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);
            if (alreadyExists)
                throw new InvalidOperationException(
                    "El jugador ya está registrado en la alineación de este partido");

            if (lineup.IsStarter)
            {
                var startersCount = await _matchLineupRepository
                    .CountStartersByMatchAndTeamAsync(matchId, player.TeamId);
                if (startersCount >= 11)
                    throw new InvalidOperationException(
                        "El equipo ya tiene 11 titulares registrados en este partido");
            }

            lineup.MatchId = matchId;

            _logger.LogInformation(
                "Registering lineup player {PlayerId} for match {MatchId}",
                lineup.PlayerId,
                matchId);

            var created = await _matchLineupRepository.CreateAsync(lineup);
            return (await _matchLineupRepository.GetByMatchAndIdAsync(matchId, created.Id))!;
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

            return await _matchLineupRepository.GetByMatchAsync(matchId);
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

            return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
        }

        public async Task DeleteAsync(int matchId, int id)
        {
            var existing = await _matchLineupRepository.GetByMatchAndIdAsync(matchId, id);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"No se encontró el registro de alineación con ID {id}");

            await _matchLineupRepository.DeleteAsync(id);
        }
    }
}
