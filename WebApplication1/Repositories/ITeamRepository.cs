using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllTeamsAsync();
    Task<IEnumerable<Team>> GetTeamsByNameAsync(string[] teamNames);
    Task<Team?> GetTeamByIdAsync(int id);
}
