using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BowlersController : ControllerBase
{
    private readonly BowlingLeagueContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _logFilePath;
    
    public BowlersController(BowlingLeagueContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "bowlers_controller_log.txt");
        
        // Initialize log file
        using (var writer = new StreamWriter(_logFilePath, append: false))
        {
            writer.WriteLine($"BowlersController initialized at {DateTime.Now}");
        }
    }
    
    private void LogToFile(string message)
    {
        Console.WriteLine(message);
        try
        {
            using (var writer = new StreamWriter(_logFilePath, append: true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
    
    // GET: api/Bowlers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BowlerDTO>>> GetBowlers()
    {
        LogToFile("GetBowlers method called");
        
        try
        {
            // Get the connection string
            var connectionString = _configuration.GetConnectionString("BowlingLeagueConnection");
            LogToFile($"Using connection string: {connectionString}");
            
            // Check if the database file exists
            var dbPath = connectionString?.Replace("Data Source=", "").Split(';')[0];
            if (!System.IO.File.Exists(dbPath))
            {
                LogToFile($"WARNING: Database file not found at {dbPath}");
                LogToFile($"Current directory: {Directory.GetCurrentDirectory()}");
                LogToFile($"Files in current directory: {string.Join(", ", Directory.GetFiles(Directory.GetCurrentDirectory()))}");
                return Problem($"Database file not found at {dbPath}", statusCode: 500);
            }
            
            LogToFile($"Database file found at {dbPath}! Size: {new FileInfo(dbPath ?? string.Empty).Length} bytes");
            
            // Try direct connection to the database
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    LogToFile("Successfully opened direct database connection!");
                    
                    // Check if the Teams table exists and has data
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Teams";
                        var teamsCount = Convert.ToInt32(command.ExecuteScalar());
                        LogToFile($"Teams count (direct): {teamsCount}");
                    }
                    
                    // Check if the Bowlers table exists and has data
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Bowlers";
                        var bowlersCount = Convert.ToInt32(command.ExecuteScalar());
                        LogToFile($"Bowlers count (direct): {bowlersCount}");
                    }
                    
                    // Check if there are Marlins and Sharks teams
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT TeamID, TeamName FROM Teams WHERE TeamName IN ('Marlins', 'Sharks')";
                        using (var reader = command.ExecuteReader())
                        {
                            LogToFile("Teams found (direct):");
                            while (reader.Read())
                            {
                                LogToFile($"  TeamID: {reader.GetInt32(0)}, TeamName: {reader.GetString(1)}");
                            }
                        }
                    }
                    
                    // Get bowlers from Marlins and Sharks teams
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT b.BowlerID, b.BowlerFirstName, b.BowlerLastName, t.TeamName
                            FROM Bowlers b
                            JOIN Teams t ON b.TeamID = t.TeamID
                            WHERE t.TeamName IN ('Marlins', 'Sharks')
                            LIMIT 5
                        ";
                        using (var reader = command.ExecuteReader())
                        {
                            LogToFile("Sample bowlers (direct):");
                            while (reader.Read())
                            {
                                LogToFile($"  BowlerID: {reader.GetInt32(0)}, Name: {reader.GetString(1)} {reader.GetString(2)}, Team: {reader.GetString(3)}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Error with direct database connection: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
            }
            
            // Now try using Entity Framework
            LogToFile("Trying to connect to the database using Entity Framework...");
            
            // First check if we can connect to the database
            if (_context.Database.CanConnect())
            {
                LogToFile("Successfully connected to the database using Entity Framework");
                
                // Check if Teams table has data
                var teamsCount = await _context.Teams.CountAsync();
                LogToFile($"Teams count (EF): {teamsCount}");
                
                // Check if Bowlers table has data
                var bowlersCount = await _context.Bowlers.CountAsync();
                LogToFile($"Bowlers count (EF): {bowlersCount}");
                
                // Get the bowlers from Marlins and Sharks teams and map to DTOs
                var bowlers = await _context.Bowlers
                    .Include(b => b.Team)
                    .Where(b => b.Team != null && (b.Team.TeamName == "Marlins" || b.Team.TeamName == "Sharks"))
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
                    
                LogToFile($"Found {bowlers.Count} bowlers from Marlins and Sharks teams (EF)");
                
                if (bowlers.Count > 0)
                {
                    LogToFile("Sample bowlers (EF):");
                    foreach (var bowler in bowlers.Take(5))
                    {
                        LogToFile($"  BowlerID: {bowler.BowlerId}, Name: {bowler.BowlerFirstName} {bowler.BowlerLastName}, Team: {bowler.TeamName}");
                    }
                }
                
                // Explicitly convert to a plain array to avoid any serialization issues
                LogToFile("Converting to array and returning...");
                var bowlersArray = bowlers.ToArray();
                LogToFile($"Array length: {bowlersArray.Length}");
                
                // Return as Ok result with the array to ensure proper serialization
                return Ok(bowlersArray);
            }
            else
            {
                LogToFile("Failed to connect to the database using Entity Framework");
                return Problem("Failed to connect to the database", statusCode: 500);
            }
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
        var bowler = await _context.Bowlers
            .Include(b => b.Team)
            .FirstOrDefaultAsync(b => b.BowlerId == id);
            
        if (bowler == null)
        {
            return NotFound();
        }
        
        // Map to DTO to avoid circular references
        var bowlerDto = new BowlerDTO
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
        
        return bowlerDto;
    }
    
    // GET: api/Bowlers/teams
    [HttpGet("teams")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeams()
    {
        // Return only the necessary team information to avoid circular references
        var teams = await _context.Teams
            .Where(t => t.TeamName == "Marlins" || t.TeamName == "Sharks")
            .Select(t => new { t.TeamId, t.TeamName })
            .ToListAsync();
            
        return teams;
    }
}
