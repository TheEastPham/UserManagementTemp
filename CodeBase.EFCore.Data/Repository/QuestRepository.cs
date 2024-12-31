using CodeBase.EFCore.Data.DB;
using CodeBase.EFCore.Data.Model;
using CodeBase.EFCore.Data.Repository.Interface;

namespace CodeBase.EFCore.Data.Repository;

public class QuestRepository : IQuestRepository
{
    private DatabaseContext _context;

    public QuestRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateQuests(IEnumerable<Quest> quests)
    {
        _context.Quests.AddRange(quests);
        return await _context.SaveChangesAsync() > 0;
    }
}