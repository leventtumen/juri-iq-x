using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JurisIQ.Backend.Models.DTOs;
using JurisIQ.Backend.Services;
using JurisIQ.Backend.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace JurisIQ.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IDocumentProcessingService _documentService;

        public AdminController(IAuthService authService, IDocumentProcessingService documentService)
        {
            _authService = authService;
            _documentService = documentService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<AdminUserListDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                
                var userDtos = users.Select(user => new AdminUserListDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SubscriptionType = user.SubscriptionType,
                    IsAdmin = user.IsAdmin,
                    IsBlocked = user.IsBlocked,
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
                }).ToList();

                return Ok(ApiResponse<List<AdminUserListDto>>.SuccessResult(userDtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<AdminUserListDto>>.ErrorResult($"Failed to get users: {ex.Message}"));
            }
        }

        [HttpPost("users/{userId}/block")]
        public async Task<ActionResult<ApiResponse<object>>> BlockUser(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == userId)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot block yourself"));
                }

                await _authService.BlockUserAsync(userId);
                return Ok(ApiResponse<object>.SuccessResult(null, "User blocked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to block user: {ex.Message}"));
            }
        }

        [HttpPost("users/{userId}/unblock")]
        public async Task<ActionResult<ApiResponse<object>>> UnblockUser(int userId)
        {
            try
            {
                await _authService.UnblockUserAsync(userId);
                return Ok(ApiResponse<object>.SuccessResult(null, "User unblocked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to unblock user: {ex.Message}"));
            }
        }

        [HttpPost("documents/process")]
        public async Task<ActionResult<ApiResponse<object>>> ProcessDocuments()
        {
            try
            {
                await _documentService.ProcessAllDocumentsAsync();
                return Ok(ApiResponse<object>.SuccessResult(null, "Document processing started"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to process documents: {ex.Message}"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}