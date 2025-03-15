using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly BowlingLeagueContext _context;
    private readonly ILogger<TeamRepository> _logger;

    public TeamRepository(BowlingLeagueContext context, ILogger<TeamRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        _logger.LogInformation("Getting all teams");
        
        return await _context.Teams.ToListAsync();
    }

    public async Task<IEnumerable<Team>> GetTeamsByNameAsync(string[] teamNames)
    {
        _logger.LogInformation("Getting teams with names: {TeamNames}", string.Join(", ", teamNames));
        
        return await _context.Teams
            .Where(t => teamNames.Contains(t.TeamName))
            .ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        _logger.LogInformation("Getting team with ID: {TeamId}", id);
        
        var team = await _context.Teams.FindAsync(id);
        
        if (team == null)
        {
            _logger.LogWarning("Team with ID {TeamId} not found", id);
        }
        
        return team;
    }
}
