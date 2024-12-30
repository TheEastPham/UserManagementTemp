namespace CodeBase.EFCore.Data.Model;

public class PlayerQuestState
{
    public string PlayerId { get; set; }
    public int QuestId { get; set; }
    public int TotalPoints { get; set; }
    public int LastMilestoneIndexCompleted { get; set; }
    public DateTime UpdateTime { get; set; }
}