using DevHabit.Api.Dtos.Common;
using DevHabit.Api.Dtos.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[Route("api/[controller]")]
[ApiController]
public class GitHubController : ControllerBase
{
    private readonly GitHubAccessTokenService _gitHubAccessTokenService;
    private readonly GitHubService _gitHubApiService;
    private readonly UserContext _userContext;
    private readonly LinkService _linkService;

    public GitHubController(
        GitHubAccessTokenService gitHubAccessTokenService,
        GitHubService gitHubApiService,
        UserContext userContext,
        LinkService linkService)
    {
        _gitHubAccessTokenService = gitHubAccessTokenService;
        _gitHubApiService = gitHubApiService;
        _userContext = userContext;
        _linkService = linkService;
    }

    [HttpPut("personal-access-token")]
    public async Task<IActionResult> StoreAccessToken(StoreGitHubAccessTokenDto storeGitHubAccessTokenDto)
    {
        var userId = await _userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _gitHubAccessTokenService.StoreAsync(userId, storeGitHubAccessTokenDto);

        return NoContent();
    }

    [HttpDelete("personal-access-token")]
    public async Task<IActionResult> RevokeAccessToken()
    {
        var userId = await _userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _gitHubAccessTokenService.RevokeAsync(userId);

        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<ActionResult<GitHubUserProfileDto>> GetUserProfile([FromHeader] AcceptHeaderDto acceptHeader)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var accessToken = await _gitHubAccessTokenService.GetAsync(userId);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return NotFound("GitHub access token not found. Please store your personal access token first.");
        }

        var userProfile = await _gitHubApiService.GetUserProfileAsync(accessToken);
        if (userProfile is null)
        {
            return NotFound("Unable to retrieve GitHub user profile. Please check your access token and try again.");
        }

        if (acceptHeader.IncludeLinks)
        {
            userProfile.Links =
            [
                _linkService.Create(nameof(GetUserProfile), "self", HttpMethods.Get),
                _linkService.Create(nameof(StoreAccessToken), "store-token", HttpMethods.Put),
                _linkService.Create(nameof(RevokeAccessToken), "revoke-token", HttpMethods.Delete)
            ];
        }

        return Ok(userProfile);
    }
}
