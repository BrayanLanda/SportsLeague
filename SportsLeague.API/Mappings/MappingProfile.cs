using AutoMapper;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;

namespace SportsLeague.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TeamRequestDTO, Team>();
            CreateMap<Team, TeamResponseDTO>();

            CreateMap<PlayerRequestDTO, Player>();
            CreateMap<Player, PlayerResponseDTO>()
                .ForMember(
                    dest => dest.TeamName,
                    opt => opt.MapFrom(src => src.Team.Name));

            CreateMap<RefereeRequestDTO, Referee>();
            CreateMap<Referee, RefereeResponseDTO>();

            CreateMap<TournamentRequestDTO, Tournament>();
            CreateMap<Tournament, TournamentResponseDTO>()
                .ForMember(
                    dest => dest.TeamsCount,
                    opt => opt.MapFrom(src =>
                        src.TournamentTeams != null ? src.TournamentTeams.Count : 0));

            CreateMap<SponsorRequestDTO, Sponsor>();
            CreateMap<Sponsor, SponsorResponseDTO>();

            CreateMap<TournamentSponsor, TournamentSponsorResponseDTO>()
                .ForMember(
                    dest => dest.TournamentName,
                    opt => opt.MapFrom(src => src.Tournament.Name))
                .ForMember(
                    dest => dest.Season,
                    opt => opt.MapFrom(src => src.Tournament.Season))
                .ForMember(
                    dest => dest.SponsorName,
                    opt => opt.MapFrom(src => src.Sponsor.Name));
        }
    }
}
