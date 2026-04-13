using System.Net.Mail;
using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            ValidateSponsor(sponsor);

            var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
            if (exists)
            {
                throw new InvalidOperationException(
                    $"Sponsor with name '{sponsor.Name}' already exists.");
            }

            _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Sponsor with ID {id} not found");
            }

            ValidateSponsor(sponsor);

            if (!string.Equals(existing.Name, sponsor.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicated = await _sponsorRepository.GetByNameAsync(sponsor.Name);
                if (duplicated != null && duplicated.Id != id)
                {
                    throw new InvalidOperationException(
                        $"Sponsor with name '{sponsor.Name}' already exists.");
                }
            }

            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException($"Sponsor with ID {id} not found");
            }

            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task<TournamentSponsor> LinkTournamentAsync(
            int sponsorId,
            int tournamentId,
            decimal contractAmount)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
            {
                throw new KeyNotFoundException($"Sponsor with ID {sponsorId} not found");
            }

            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
            {
                throw new KeyNotFoundException($"Tournament with ID {tournamentId} not found");
            }

            if (contractAmount <= 0)
            {
                throw new InvalidOperationException("Contract amount must be greater than 0");
            }

            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"Sponsor with ID {sponsorId} is already linked to tournament {tournamentId}");
            }

            var tournamentSponsor = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Linking sponsor {SponsorId} to tournament {TournamentId}",
                sponsorId,
                tournamentId);

            await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);

            return (await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId))!;
        }

        public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
            {
                throw new KeyNotFoundException($"Sponsor with ID {sponsorId} not found");
            }

            return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        }

        public async Task UnlinkTournamentAsync(int sponsorId, int tournamentId)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
            {
                throw new KeyNotFoundException($"Sponsor with ID {sponsorId} not found");
            }

            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
            {
                throw new KeyNotFoundException($"Tournament with ID {tournamentId} not found");
            }

            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing == null)
            {
                throw new KeyNotFoundException(
                    $"Sponsor with ID {sponsorId} is not linked to tournament {tournamentId}");
            }

            _logger.LogInformation(
                "Unlinking sponsor {SponsorId} from tournament {TournamentId}",
                sponsorId,
                tournamentId);
            await _tournamentSponsorRepository.DeleteAsync(existing.Id);
        }

        private static void ValidateSponsor(Sponsor sponsor)
        {
            if (string.IsNullOrWhiteSpace(sponsor.Name))
            {
                throw new InvalidOperationException("Sponsor name is required");
            }

            if (string.IsNullOrWhiteSpace(sponsor.ContactEmail))
            {
                throw new InvalidOperationException("Contact email is required");
            }

            try
            {
                _ = new MailAddress(sponsor.ContactEmail);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Contact email format is invalid");
            }
        }
    }
}
