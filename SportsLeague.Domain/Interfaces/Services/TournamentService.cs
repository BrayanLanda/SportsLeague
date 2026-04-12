using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentTeamRepository _tournamentTeamRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<TournamentService> _logger;

        public TournamentService(
            ITournamentRepository tournamentRepository,
            ITournamentTeamRepository tournamentTeamRepository,
            ITeamRepository teamRepository,
            ILogger<TournamentService> logger)
        {
            _tournamentRepository = tournamentRepository;
            _tournamentTeamRepository = tournamentTeamRepository;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Tournament>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all tournaments");
            return await _tournamentRepository.GetAllAsync();
        }

        public async Task<Tournament?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving tournament with ID: {TournamentId}", id);
            var tournament = await _tournamentRepository.GetByIdWithTeamsAsync(id);
            if (tournament == null)
                _logger.LogWarning("Tournament with ID {TournamentId} not found", id);
            return tournament;
        }

        public async Task<Tournament> CreateAsync(Tournament tournament)
        {
            if (tournament.EndDate <= tournament.StartDate)
            {
                throw new InvalidOperationException("End date must be after start date");
            }

            tournament.Status = TournamentStatus.Pending;

            _logger.LogInformation("Creating tournament: {TournamentName}", tournament.Name);
            return await _tournamentRepository.CreateAsync(tournament);
        }

        public async Task UpdateAsync(int id, Tournament tournament)
        {
            var existing = await _tournamentRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Tournament with ID {id} not found");

            if (existing.Status != TournamentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only tournaments in Pending status can be edited");
            }

            if (tournament.EndDate <= tournament.StartDate)
            {
                throw new InvalidOperationException("Dates are invalid: End date must be after start date");
            }

            existing.Name = tournament.Name;
            existing.Season = tournament.Season;
            existing.StartDate = tournament.StartDate;
            existing.EndDate = tournament.EndDate;

            _logger.LogInformation("Updating tournament with ID: {TournamentId}", id);
            await _tournamentRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _tournamentRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Tournament with ID {id} not found");

            if (existing.Status != TournamentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only tournaments in Pending status can be deleted");
            }

            _logger.LogInformation("Deleting tournament with ID: {TournamentId}", id);
            await _tournamentRepository.DeleteAsync(id);
        }

        public async Task UpdateStatusAsync(int id, TournamentStatus newStatus)
        {
            var tournament = await _tournamentRepository.GetByIdAsync(id);
            if (tournament == null)
                throw new KeyNotFoundException($"Tournament with ID {id} not found");

            var validTransition = (tournament.Status, newStatus) switch
            {
                (TournamentStatus.Pending, TournamentStatus.InProgress) => true,
                (TournamentStatus.InProgress, TournamentStatus.Finished) => true,
                _ => false
            };

            if (!validTransition)
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from {tournament.Status} to {newStatus}");
            }

            tournament.Status = newStatus;

            _logger.LogInformation(
                "Updating tournament {TournamentId} status to {NewStatus}",
                id, newStatus);
            await _tournamentRepository.UpdateAsync(tournament);
        }

        public async Task RegisterTeamAsync(int tournamentId, int teamId)
        {
            // Validar que el torneo existe
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException(
                    $"No se encontró el torneo con ID {tournamentId}");

            // Solo se pueden inscribir equipos en torneos Pending
            if (tournament.Status != TournamentStatus.Pending)
            {
                throw new InvalidOperationException("Only tournaments in Pending status can register teams");
            }

            var teamExists = await _teamRepository.ExistsAsync(teamId);
            if (!teamExists)
                throw new KeyNotFoundException($"Team with ID {teamId} not found");

            var existing = await _tournamentTeamRepository
                .GetByTournamentAndTeamAsync(tournamentId, teamId);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"Team with ID {teamId} is already registered in tournament {tournamentId}");
            }

            var tournamentTeam = new TournamentTeam
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                RegisteredAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Registering team {TeamId} in tournament {TournamentId}",
                teamId, tournamentId);
            await _tournamentTeamRepository.CreateAsync(tournamentTeam);
        }

        public async Task<IEnumerable<Team>> GetTeamsByTournamentAsync(int tournamentId)
        {
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException(
                    $"The tournament with ID {tournamentId} was not found");

            var tournamentTeams = await _tournamentTeamRepository
                .GetByTournamentAsync(tournamentId);

            return tournamentTeams.Select(tt => tt.Team);
        }
    }
}