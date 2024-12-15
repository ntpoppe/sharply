using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sharply.Server.Data;
using Sharply.Server.Models;
using Sharply.Server.Services;
using Sharply.Shared.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

/// <summary>
/// API controller for handling user authentication and registration
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly SharplyDbContext _context;
    private readonly UserService _userService;
    private readonly ServerService _serverService;
    private readonly ChannelService _channelService;
    private readonly string _jwtKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="configuration">Contains the secret key for JWT signing.</param>
    public AuthController(
        SharplyDbContext context,
        UserService userService,
        ServerService serverService,
        ChannelService channelService,
        IConfiguration configuration)
    {
        _context = context;
        _userService = userService;
        _serverService = serverService;
        _channelService = channelService;
        _jwtKey = configuration["Jwt:Key"] ?? "DEFAULT";
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request containing username and password.</param>
    /// <returns>
    /// A response containing the registered user's username and a JWT token.
    /// </returns>
    /// <response code="200">The user was successfully registered.</response>
    /// <response code="400">The username already exists.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if the user exists
        if (_context.Users.Any(u => u.Username == request.Username))
        {
            return BadRequest("User already exists");
        }

        // Create the new user
        var user = new User { Username = request.Username };
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate a token for the new user
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, user.Username) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Join default server/channel(s)
        await EnsureDefaultServerAssignmentAsync(user.Id);

        // Send response
        return Ok(new RegisterResponse
        {
            Username = user.Username,
            Token = tokenString
        });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>
    /// A response containing the authenticated user's username and a JWT token.
    /// </returns>
    /// <response code="200">The user was successfully authenticated.</response>
    /// <response code="401">The username or password is invalid.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Find the user by username
        var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);
        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Verify password
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Generate a token for the user
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Send response
        return Ok(new LoginResponse
        {
            Username = user.Username,
            Token = tokenString
        });
    }

    /// <summary>
    /// Ensures a user logging in has access to the default global server.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task EnsureDefaultServerAssignmentAsync(int userId)
    {
        var defaultServer = await _context.Servers.FirstOrDefaultAsync(s => s.Id == 1);
        if (defaultServer == null) return;

        var userServer = await _context.UserServers.FirstOrDefaultAsync(us => us.UserId == userId && us.ServerId == defaultServer.Id);
        if (userServer == null)
        {
            await _userService.AddUserToServerAsync(userId, defaultServer.Id);
            await EnsureDefaultChannelAssignmentAsync(defaultServer.Id, userId);
        }
    }

    public async Task EnsureDefaultChannelAssignmentAsync(int serverId, int userId)
    {
        var defaultChannels = await _serverService.GetChannelsForServerAsync(serverId);
        if (defaultChannels == null) return;

        var userChannels = await _userService.GetChannelsForUserAsync(userId);

        var channelsToAdd = defaultChannels
            .Where(defaultChannel => !userChannels.Any(userChannel => userChannel.Id == defaultChannel.Id))
            .ToList();

        foreach (var channel in channelsToAdd)
            await _channelService.AddUserToChannel(userId, channel.Id);
    }
}
