using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public TestController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // GET: api/Test
    [HttpGet]
    public ActionResult<object> TestDatabase()
    {
        var result = new Dictionary<string, object>();
        
        try
        {
            // Get the connection string
            var connectionString = _configuration.GetConnectionString("BowlingLeagueConnection");
            result["connectionString"] = connectionString ?? "Connection string not found";
            
            if (connectionString == null)
            {
                result["error"] = "Connection string is null";
                return result;
            }
            
            // Check if the file exists
            var dbPath = connectionString.Replace("Data Source=", "").Split(';')[0];
            var fileExists = System.IO.File.Exists(dbPath);
            result["fileExists"] = fileExists;
            result["dbPath"] = dbPath;
            
            if (!fileExists)
            {
                return result;
            }
            
            // Try to open a direct connection to the database
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                result["connectionOpened"] = true;
                
                // Check if the Teams table exists and has data
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Teams";
                    var teamsCount = Convert.ToInt32(command.ExecuteScalar());
                    result["teamsCount"] = teamsCount;
                }
                
                // Check if the Bowlers table exists and has data
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Bowlers";
                    var bowlersCount = Convert.ToInt32(command.ExecuteScalar());
                    result["bowlersCount"] = bowlersCount;
                }
                
                // Check if there are Marlins and Sharks teams
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TeamID, TeamName FROM Teams WHERE TeamName IN ('Marlins', 'Sharks')";
                    using (var reader = command.ExecuteReader())
                    {
                        var teams = new List<object>();
                        while (reader.Read())
                        {
                            teams.Add(new
                            {
                                TeamId = reader.GetInt32(0),
                                TeamName = reader.GetString(1)
                            });
                        }
                        result["teams"] = teams;
                    }
                }
                
                // Check if there are bowlers in Marlins and Sharks teams
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM Bowlers b
                        JOIN Teams t ON b.TeamID = t.TeamID
                        WHERE t.TeamName IN ('Marlins', 'Sharks')
                    ";
                    var filteredBowlersCount = Convert.ToInt32(command.ExecuteScalar());
                    result["filteredBowlersCount"] = filteredBowlersCount;
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            result["error"] = ex.Message;
            result["stackTrace"] = ex.StackTrace ?? "No stack trace available";
            return result;
        }
    }
}
