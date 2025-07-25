using Base.UserManagement.Domain.DTOs;
using Base.UserManagement.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,ContentAdmin")]
    public async Task<ActionResult<GetUsersResponse>> GetUsers([FromQuery] GetUsersRequest request)
    {
        try
        {
            var response = await _userService.GetUsersAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            // Users can only view their own profile unless they're admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("SystemAdmin") || User.IsInRole("ContentAdmin");

            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest("Route ID does not match request ID");
            }

            // Users can only update their own profile unless they're admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("SystemAdmin") || User.IsInRole("ContentAdmin");

            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateUserAsync(request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{id}/roles")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
    {
        try
        {
            if (id != request.UserId)
            {
                return BadRequest("Route ID does not match request user ID");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _userService.AssignRoleAsync(request);
            if (!success)
            {
                return BadRequest("Failed to assign role");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", request.RoleName, id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("{id}/roles/{roleName}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult> RemoveRole(string id, string roleName)
    {
        try
        {
            var success = await _userService.RemoveRoleAsync(id, roleName);
            if (!success)
            {
                return BadRequest("Failed to remove role");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, id);
            return StatusCode(500, "Internal server error");
        }
    }
}
