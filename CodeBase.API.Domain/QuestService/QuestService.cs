using System.Text.Json;
using CodeBase.EFCore.Data.Model;

namespace CodeBase.QuestService;

public class QuestService : IQuestService
{
    
    public async Task<bool> InitializeQuests()
    {
        var quests = LoadQuests();
        if (!quests.Any())
        {
            return false;
        }

        return true;
    }

    #region private functions

    private List<Quest> LoadQuests()
    {
        var json = File.ReadAllText("quests.json");
        var quests = JsonSerializer.Deserialize<List<Quest>>(json);
        return [..quests];
    }

    #endregion
}