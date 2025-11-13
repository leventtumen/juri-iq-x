using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        ILogger<AdminController> logger)
    {
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            // Note: This is a simplified version
            // In production, you would implement proper pagination and filtering
            return Ok(new { message = "Admin endpoint - implement user list query" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred while retrieving users." });
        }
    }

    [HttpGet("users/{userId}/devices")]
    public async Task<IActionResult> GetUserDevices(int userId)
    {
        try
        {
            var devices = await _deviceRepository.GetUserDevicesAsync(userId);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting devices for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user devices." });
        }
    }
}
