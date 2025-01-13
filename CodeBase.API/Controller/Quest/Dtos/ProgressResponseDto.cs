namespace CodeBase.API.Controller.Quest.Dtos;

public class ProgressResponseDto
{
    public int QuestPointsEarned { get; set; }
    public int TotalQuestPercentCompleted { get; set; }
    public MilestonesCompleted MilestonesCompleted { get; set; }
}