using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BowlersController : ControllerBase
{
    private readonly IBowlerRepository _bowlerRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BowlersController> _logger;
    private readonly string _logFilePath;
    
    public BowlersController(
        IBowlerRepository bowlerRepository,
        ITeamRepository teamRepository,
        IConfiguration configuration,
        ILogger<BowlersController> logger)
    {
        _bowlerRepository = bowlerRepository;
        _teamRepository = teamRepository;
        _configuration = configuration;
        _logger = logger;
        _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "bowlers_controller_log.txt");
        
        // Initialize log file
        using (var writer = new StreamWriter(_logFilePath, append: false))
        {
            writer.WriteLine($"BowlersController initialized at {DateTime.Now}");
        }
    }
    
    private void LogToFile(string message)
    {
        _logger.LogInformation(message);
        try
        {
            using (var writer = new StreamWriter(_logFilePath, append: true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to log file: {Message}", ex.Message);
        }
    }
    
    // GET: api/Bowlers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BowlerDTO>>> GetBowlers()
    {
        LogToFile("GetBowlers method called");
        
        try
        {
            // Get the connection string for validation
            var connectionString = _configuration.GetConnectionString("BowlingLeagueConnection");
            LogToFile($"Using connection string: {connectionString}");
            
            // Check if the database file exists
            var dbPath = connectionString?.Replace("Data Source=", "").Split(';')[0];
            if (!System.IO.File.Exists(dbPath))
            {
                LogToFile($"WARNING: Database file not found at {dbPath}");
                return Problem($"Database file not found at {dbPath}", statusCode: 500);
            }
            
            LogToFile($"Database file found at {dbPath}! Size: {new FileInfo(dbPath ?? string.Empty).Length} bytes");
            
            // Use the repository to get bowlers from Marlins and Sharks teams
            var bowlers = await _bowlerRepository.GetBowlersByTeamAsync(new[] { "Marlins", "Sharks" });
            var bowlersList = bowlers.ToList();
            
            LogToFile($"Found {bowlersList.Count} bowlers from Marlins and Sharks teams");
            
            if (bowlersList.Count > 0)
            {
                LogToFile("Sample bowlers:");
                foreach (var bowler in bowlersList.Take(5))
                {
                    LogToFile($"  BowlerID: {bowler.BowlerId}, Name: {bowler.BowlerFirstName} {bowler.BowlerLastName}, Team: {bowler.TeamName}");
                }
            }
            
            // Return as Ok result with the array to ensure proper serialization
            return Ok(bowlersList.ToArray());
        }
        catch (Exception ex)
        {
            LogToFile($"Error in GetBowlers: {ex.Message}");
            LogToFile($"Stack trace: {ex.StackTrace}");
            return Problem($"Error retrieving bowlers: {ex.Message}", statusCode: 500);
        }
    }
    
    // GET: api/Bowlers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BowlerDTO>> GetBowler(int id)
    {
        var bowler = await _bowlerRepository.GetBowlerByIdAsync(id);
            
        if (bowler == null)
        {
            return NotFound();
        }
        
        return bowler;
    }
    
    // GET: api/Bowlers/teams
    [HttpGet("teams")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeams()
    {
        // Get teams with names Marlins and Sharks
        var teams = await _teamRepository.GetTeamsByNameAsync(new[] { "Marlins", "Sharks" });
        
        // Map to anonymous objects to avoid circular references
        var teamDtos = teams.Select(t => new { t.TeamId, t.TeamName }).ToList();
            
        return teamDtos;
    }
}
