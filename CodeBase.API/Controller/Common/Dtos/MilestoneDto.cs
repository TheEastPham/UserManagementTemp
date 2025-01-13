namespace CodeBase.API.Controller.Dtos;

public class MilestoneDto
{
    public int Id { get; set; }
    public int QuestId { get; set; }
    public int Index { get; set; }
    public int PointsRequired { get; set; }
    public int ChipsAwarded { get; set; }
}