namespace SportsLeague.Domain.Entities
{
    public class TournamentTeam : AuditBase
    {
        public int TournamentId { get; set; }
        public int TeamId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Tournament Tournament { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }
}