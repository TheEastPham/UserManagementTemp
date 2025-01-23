namespace CodeBase.Model.TransferObject.UserData;

public class UserDataDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public IEnumerable<RoleDataDto>? Roles { get; set; }

}