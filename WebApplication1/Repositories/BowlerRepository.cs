using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class BowlerRepository : IBowlerRepository
{
    private readonly BowlingLeagueContext _context;
    private readonly ILogger<BowlerRepository> _logger;

    public BowlerRepository(BowlingLeagueContext context, ILogger<BowlerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BowlerDTO>> GetAllBowlersAsync()
    {
        _logger.LogInformation("Getting all bowlers");
        
        return await _context.Bowlers
            .Include(b => b.Team)
            .Select(b => new BowlerDTO
            {
                BowlerId = b.BowlerId,
                BowlerFirstName = b.BowlerFirstName,
                BowlerMiddleInit = b.BowlerMiddleInit,
                BowlerLastName = b.BowlerLastName,
                BowlerAddress = b.BowlerAddress,
                BowlerCity = b.BowlerCity,
                BowlerState = b.BowlerState,
                BowlerZip = b.BowlerZip,
                BowlerPhoneNumber = b.BowlerPhoneNumber,
                TeamName = b.Team != null ? b.Team.TeamName : null
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<BowlerDTO>> GetBowlersByTeamAsync(string[] teamNames)
    {
        _logger.LogInformation("Getting bowlers for teams: {TeamNames}", string.Join(", ", teamNames));
        
        return await _context.Bowlers
            .Include(b => b.Team)
            .Where(b => b.Team != null && teamNames.Contains(b.Team.TeamName))
            .Select(b => new BowlerDTO
            {
                BowlerId = b.BowlerId,
                BowlerFirstName = b.BowlerFirstName,
                BowlerMiddleInit = b.BowlerMiddleInit,
                BowlerLastName = b.BowlerLastName,
                BowlerAddress = b.BowlerAddress,
                BowlerCity = b.BowlerCity,
                BowlerState = b.BowlerState,
                BowlerZip = b.BowlerZip,
                BowlerPhoneNumber = b.BowlerPhoneNumber,
                TeamName = b.Team != null ? b.Team.TeamName : null
            })
            .ToListAsync();
    }

    public async Task<BowlerDTO?> GetBowlerByIdAsync(int id)
    {
        _logger.LogInformation("Getting bowler with ID: {BowlerId}", id);
        
        var bowler = await _context.Bowlers
            .Include(b => b.Team)
            .FirstOrDefaultAsync(b => b.BowlerId == id);
            
        if (bowler == null)
        {
            _logger.LogWarning("Bowler with ID {BowlerId} not found", id);
            return null;
        }
        
        return new BowlerDTO
        {
            BowlerId = bowler.BowlerId,
            BowlerFirstName = bowler.BowlerFirstName,
            BowlerMiddleInit = bowler.BowlerMiddleInit,
            BowlerLastName = bowler.BowlerLastName,
            BowlerAddress = bowler.BowlerAddress,
            BowlerCity = bowler.BowlerCity,
            BowlerState = bowler.BowlerState,
            BowlerZip = bowler.BowlerZip,
            BowlerPhoneNumber = bowler.BowlerPhoneNumber,
            TeamName = bowler.Team?.TeamName
        };
    }
}
