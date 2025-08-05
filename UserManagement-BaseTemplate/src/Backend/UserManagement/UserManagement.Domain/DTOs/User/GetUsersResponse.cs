namespace UserManagement.Domain.DTOs.User;

public record GetUsersResponse(
    IEnumerable<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
