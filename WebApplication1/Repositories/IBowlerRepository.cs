using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IBowlerRepository
{
    Task<IEnumerable<BowlerDTO>> GetAllBowlersAsync();
    Task<IEnumerable<BowlerDTO>> GetBowlersByTeamAsync(string[] teamNames);
    Task<BowlerDTO?> GetBowlerByIdAsync(int id);
}
