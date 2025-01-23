using System.Text.Json;
using CodeBase.EFCore.Data.Model;
using CodeBase.EFCore.Data.Repository.Interface;

namespace CodeBase.Service;

public class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;

    public QuestService(IQuestRepository questRepository)
    {
        _questRepository = questRepository;
    }

    public async Task<bool> InitializeQuestsAsync()
    {
        var quests = LoadQuests();
        if (!quests.Any())
        {
            return false;
        }

        await _questRepository.InitializeQuests(quests);
        return true;
    }

    public async Task<List<Quest>> GetAllQuestsAsync()
    {
        return await _questRepository.GetAllQuestsAsync();
    }

    #region private functions

    private List<Quest> LoadQuests()
    {
        var json = File.ReadAllText("quests.json");
        var quests = JsonSerializer.Deserialize<List<Quest>>(json);
        return quests;
    }

    #endregion
}