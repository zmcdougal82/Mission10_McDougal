using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using WebApplication1.Data;

// Set up file logging
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "app_log.txt");
using (var writer = new StreamWriter(logFilePath, append: false))
{
    writer.WriteLine($"Application started at {DateTime.Now}");
    writer.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
}

// Helper function to log to both console and file
void LogToFileAndConsole(string message)
{
    Console.WriteLine(message);
    try
    {
        using (var writer = new StreamWriter(logFilePath, append: true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error writing to log file: {ex.Message}");
    }
}

LogToFileAndConsole("Starting application...");

// Run the build-check.js script to ensure the React app is built
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build-check.js")))
{
    try
    {
        Console.WriteLine("Checking if React app needs to be built...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build-check.js"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.OutputDataReceived += (sender, data) => { if (data.Data != null) Console.WriteLine(data.Data); };
        process.ErrorDataReceived += (sender, data) => { if (data.Data != null) Console.WriteLine(data.Data); };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            Console.WriteLine("Failed to build React app. Please build it manually using the build-react script.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running build-check.js: {ex.Message}");
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Handle circular references in JSON serialization
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth if needed
});

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("BowlingLeagueConnection");
LogToFileAndConsole($"Using database connection string: {connectionString}");

// Check if the database file exists
var dbPath = connectionString?.Replace("Data Source=", "").Split(';')[0];
if (!File.Exists(dbPath))
{
    LogToFileAndConsole($"WARNING: Database file not found at {dbPath}");
    LogToFileAndConsole($"Current directory: {Directory.GetCurrentDirectory()}");
    LogToFileAndConsole($"Files in current directory: {string.Join(", ", Directory.GetFiles(Directory.GetCurrentDirectory()))}");
}
else
{
    LogToFileAndConsole($"Database file found at {dbPath}! Size: {new FileInfo(dbPath).Length} bytes");
    
    // Try to open the database file to check if it's valid
    try
    {
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
        {
            connection.Open();
            LogToFileAndConsole("Successfully opened database connection!");
            
            // Check if the Teams table exists
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Teams'";
                var result = command.ExecuteScalar();
                LogToFileAndConsole($"Teams table exists: {Convert.ToInt32(result) > 0}");
            }
            
            connection.Close();
        }
    }
    catch (Exception ex)
    {
        LogToFileAndConsole($"Error opening database: {ex.Message}");
        LogToFileAndConsole($"Stack trace: {ex.StackTrace}");
    }
}

builder.Services.AddDbContext<BowlingLeagueContext>(options =>
{
    options.UseSqlite(connectionString);
    // Enable sensitive data logging for debugging
    options.EnableSensitiveDataLogging();
    // Log all database commands
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

// Register repositories
builder.Services.AddScoped<WebApplication1.Repositories.IBowlerRepository, WebApplication1.Repositories.BowlerRepository>();
builder.Services.AddScoped<WebApplication1.Repositories.ITeamRepository, WebApplication1.Repositories.TeamRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add SPA services
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();
app.UseCors("ReactApp");

app.UseAuthorization();

app.MapControllers();

// Remove the default Razor Pages routing
// app.MapStaticAssets();
// app.MapRazorPages()
//    .WithStaticAssets();

// Configure the SPA - this should be the last middleware
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    
    // We're not using the proxy to a development server
    // Instead, we'll build the React app and serve it directly
});

app.Run();
