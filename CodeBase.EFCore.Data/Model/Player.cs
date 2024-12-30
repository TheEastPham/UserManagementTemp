namespace CodeBase.EFCore.Data.Model;

public class Player
{
    public Guid UserId { get; set; }
    public long PlayerId { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Chips { get; set; }
    public bool IsDelete { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}