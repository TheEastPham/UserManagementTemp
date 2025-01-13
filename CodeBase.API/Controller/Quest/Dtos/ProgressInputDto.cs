namespace CodeBase.API.Controller.Quest.Dtos;

public record ProgressInputDto
{
    public Guid PlayerId { get; set; }
    public int PlayerLevel { get; set; }
    public int ChipAmountBet { get; set; }
}