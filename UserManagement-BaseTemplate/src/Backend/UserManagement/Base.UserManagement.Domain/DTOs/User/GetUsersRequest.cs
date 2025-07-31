namespace Base.UserManagement.Domain.DTOs.User;

public record GetUsersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Role = null,
    string? SearchTerm = null
);
