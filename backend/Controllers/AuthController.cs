using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JurisIQ.Backend.Models.DTOs;
using JurisIQ.Backend.Services;
using JurisIQ.Backend.Models;
using System.Security.Claims;

namespace JurisIQ.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var (user, error) = await _authService.LoginAsync(
                    request.Email, 
                    request.Password, 
                    request.DeviceId,
                    request.DeviceName,
                    request.DeviceType,
                    userAgent,
                    ipAddress
                );

                if (user == null)
                {
                    return Unauthorized(ApiResponse<LoginResponse>.ErrorResult(error ?? "Login failed"));
                }

                var token = _jwtService.GenerateToken(user);
                var userDto = MapToUserDto(user);

                var response = new LoginResponse
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };

                return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Login successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var (user, error) = await _authService.RegisterAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName
                );

                if (user == null)
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult(error ?? "Registration failed"));
                }

                var userDto = MapToUserDto(user);
                return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "Registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<UserDto>.ErrorResult("Invalid token"));
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
                }

                var userDto = MapToUserDto(user);
                return Ok(ApiResponse<UserDto>.SuccessResult(userDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("health")]
        public ActionResult<ApiResponse<object>> HealthCheck()
        {
            return Ok(ApiResponse<object>.SuccessResult(new { status = "healthy", timestamp = DateTime.UtcNow }));
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SubscriptionType = user.SubscriptionType,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Devices = user.Devices.Select(d => new UserDeviceDto
                {
                    Id = d.Id,
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    DeviceType = d.DeviceType,
                    FirstLoginAt = d.FirstLoginAt,
                    LastLoginAt = d.LastLoginAt,
                    IsActive = d.IsActive
                }).ToList()
            };
        }
    }
}