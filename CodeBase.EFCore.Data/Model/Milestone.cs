namespace CodeBase.EFCore.Data.Model;

public class Milestone
{
    public int Id { get; set; }
    public int QuestId { get; set; }
    public int Index { get; set; }
    public int PointsRequired { get; set; }
    public int ChipsAwarded { get; set; }
    public virtual Quest Quest { get; set; }
}