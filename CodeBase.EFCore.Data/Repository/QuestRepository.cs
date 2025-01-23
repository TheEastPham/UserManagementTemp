using CodeBase.EFCore.Data.DB;
using CodeBase.EFCore.Data.Model;
using CodeBase.EFCore.Data.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CodeBase.EFCore.Data.Repository;

public class QuestRepository : IQuestRepository
{
    private DatabaseContext _context;

    public QuestRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> InitializeQuests(IEnumerable<Quest> quests)
    {
        var questIds = quests.Select(q => q.Id).ToList();
        var milestonesIds = quests.SelectMany(q => q.Milestones).Select(m => m.Id).ToList();
        await _context.Milestones
            .Where(m => milestonesIds.Contains(m.Id))
            .ExecuteDeleteAsync();
        await _context.Quests
            .Where(q => questIds.Contains(q.Id))
            .ExecuteDeleteAsync();
        
        _context.Quests.AddRange(quests);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Quest>> GetAllQuestsAsync()
    {
        return await _context.Quests
            .Include(q => q.Milestones)
            .AsNoTracking()
            .ToListAsync();
    }
}