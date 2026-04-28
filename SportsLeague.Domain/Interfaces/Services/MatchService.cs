using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentTeamRepository _tournamentTeamRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IRefereeRepository _refereeRepository;
        private readonly ILogger<MatchService> _logger;

        public MatchService(
       IMatchRepository matchRepository,
       ITournamentRepository tournamentRepository,
       ITournamentTeamRepository tournamentTeamRepository,
       ITeamRepository teamRepository,
       IRefereeRepository refereeRepository,
       ILogger<MatchService> logger)
        {
            _matchRepository = matchRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentTeamRepository = tournamentTeamRepository;
            _teamRepository = teamRepository;
            _refereeRepository = refereeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Match>> GetAllByTournamentAsync(int tournamentId)
        {
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException(
                    $"No se encontró el torneo con ID {tournamentId}");

            return await _matchRepository.GetByTournamentWithDetailsAsync(tournamentId);
        }

        public async Task<Match?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving match with ID: {MatchId}", id);
            return await _matchRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<Match> CreateAsync(Match match)
        {
            var tournament = await _tournamentRepository.GetByIdAsync(match.TournamentId);
            if (tournament == null)
                throw new KeyNotFoundException(
                    $"No se encontró el torneo con ID {match.TournamentId}");

            if (tournament.Status != TournamentStatus.InProgress)
                throw new InvalidOperationException(
                    "Only matches for tournaments in progress can be created");

            if (match.HomeTeamId == match.AwayTeamId)
                throw new InvalidOperationException(
                    "The home and away teams must be different");

            var homeTeamExists = await _teamRepository.ExistsAsync(match.HomeTeamId);
            if (!homeTeamExists)
                throw new KeyNotFoundException(
                    $"The home team with ID {match.HomeTeamId} does not exist");

            var awayTeamExists = await _teamRepository.ExistsAsync(match.AwayTeamId);
            if (!awayTeamExists)
                throw new KeyNotFoundException(
                    $"The away team with ID {match.AwayTeamId} does not exist");

            // 4. Validar que ambos equipos están inscritos en el torneo
            var homeEnrolled = await _tournamentTeamRepository
                .GetByTournamentAndTeamAsync(match.TournamentId, match.HomeTeamId);
            if (homeEnrolled == null)
                throw new InvalidOperationException(
                    "The home team is not registered in this tournament");

            var awayEnrolled = await _tournamentTeamRepository
                .GetByTournamentAndTeamAsync(match.TournamentId, match.AwayTeamId);
            if (awayEnrolled == null)
                throw new InvalidOperationException(
                    "The away team is not registered in this tournament");

            var refereeExists = await _refereeRepository.ExistsAsync(match.RefereeId);
            if (!refereeExists)
                throw new KeyNotFoundException(
                    $"The referee with ID {match.RefereeId} does not exist");

            match.Status = MatchStatus.Scheduled;

            _logger.LogInformation(
                "Creating match: Team {Home} vs Team {Away} in Tournament {Tournament}",
                match.HomeTeamId, match.AwayTeamId, match.TournamentId);
            return await _matchRepository.CreateAsync(match);
        }

        public async Task UpdateAsync(int id, Match match)
        {
            var existing = await _matchRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"The match with ID {id} was not found");

            if (existing.Status != MatchStatus.Scheduled)
                throw new InvalidOperationException(
                    "Only matches with status Scheduled can be updated");

            if (match.HomeTeamId == match.AwayTeamId)
                throw new InvalidOperationException(
                    "The home and away teams must be different");

            var homeEnrolled = await _tournamentTeamRepository
                .GetByTournamentAndTeamAsync(existing.TournamentId, match.HomeTeamId);
            if (homeEnrolled == null)
                throw new InvalidOperationException(
                    "The home team is not registered in this tournament");

            var awayEnrolled = await _tournamentTeamRepository
                .GetByTournamentAndTeamAsync(existing.TournamentId, match.AwayTeamId);
            if (awayEnrolled == null)
                throw new InvalidOperationException(
                    "The away team is not registered in this tournament");

            var refereeExists = await _refereeRepository.ExistsAsync(match.RefereeId);
            if (!refereeExists)
                throw new KeyNotFoundException(
                    $"The referee with ID {match.RefereeId} does not exist");

            existing.HomeTeamId = match.HomeTeamId;
            existing.AwayTeamId = match.AwayTeamId;
            existing.RefereeId = match.RefereeId;
            existing.MatchDate = match.MatchDate;
            existing.Venue = match.Venue;
            existing.Matchday = match.Matchday;

            _logger.LogInformation("Updating match with ID: {MatchId}", id);
            await _matchRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _matchRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"The match with ID {id} was not found");

            if (existing.Status != MatchStatus.Scheduled)
                throw new InvalidOperationException(
                    "Only matches with status Scheduled can be deleted");

            _logger.LogInformation("Deleting match with ID: {MatchId}", id);
            await _matchRepository.DeleteAsync(id);
        }

        public async Task UpdateStatusAsync(int id, MatchStatus newStatus)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null)
                throw new KeyNotFoundException($"The match with ID {id} was not found");

            var validTransition = (match.Status, newStatus) switch
            {
                (MatchStatus.Scheduled, MatchStatus.InProgress) => true,
                (MatchStatus.InProgress, MatchStatus.Finished) => true,
                (MatchStatus.Scheduled, MatchStatus.Suspended) => true,
                (MatchStatus.InProgress, MatchStatus.Suspended) => true,
                _ => false
            };

            if (!validTransition)
                throw new InvalidOperationException(
                    $"Cannot change status from {match.Status} to {newStatus}");

            match.Status = newStatus;

            _logger.LogInformation(
                "Updating match {MatchId} status to {NewStatus}", id, newStatus);
            await _matchRepository.UpdateAsync(match);
        }
    }
}