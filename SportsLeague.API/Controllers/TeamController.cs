using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamController> _logger;


        public TeamController(
           ITeamService teamService,
           IMapper mapper,
           ILogger<TeamController> logger)
        {
            _teamService = teamService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamResponseDTO>>> GetAll()
        {
            var teams = await _teamService.GetAllAsync();
            var teamsDTO = _mapper.Map<IEnumerable<TeamResponseDTO>>(teams);
            return Ok(teamsDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamResponseDTO>> GetById(int id)
        {
            var team = await _teamService.GetByIdAsync(id);
            if (team == null)
                return NotFound(new { Message = $"Team with ID {id} not found." });

            var teamDTO = _mapper.Map<TeamResponseDTO>(team);
            return Ok(teamDTO);
        }

        [HttpPost]
        public async Task<ActionResult<TeamResponseDTO>> Create(TeamRequestDTO dto)
        {
            try
            {
                var team = _mapper.Map<Team>(dto);
                var createdTeam = await _teamService.CreateAsync(team);
                var responseDTO = _mapper.Map<TeamResponseDTO>(createdTeam);
                return CreatedAtAction(nameof(GetById), new { id = responseDTO.Id }, responseDTO);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating team");
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, TeamRequestDTO dto)
        {
            try
            {
                var team = _mapper.Map<Team>(dto);
                await _teamService.UpdateAsync(id, team);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _teamService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}