using System.Security;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;
using TsubaHaru.FleckyBot.VRChat.Models;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace TsubaHaru.FleckyBot.VRChat.Services;

public class VRChatService
{
    private readonly ILogger<VRChatService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VRChatSettings _settings;
    private readonly Configuration _configuration;
    private AuthenticationApi? _authApi;

    public VRChatService(ILogger<VRChatService> logger, IServiceScopeFactory scopeFactory, VRChatSettings settings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _settings = settings;

        _configuration = new Configuration
        {
            BasePath = "https://api.vrchat.cloud/api/1",
            Username = settings.Username,
            Password = settings.Password,
            UserAgent = "FleckyBot/2.1.0 helloATtsuyabashi.dev"
        };
    }

    private void Authenticate()
    {
        try
        {
            _authApi = new AuthenticationApi(_configuration);
            CurrentUser currentUser = _authApi.GetCurrentUser();

            if (currentUser == null)
            {
                // TODO: get the latest totp token
                Totp totpCode = new Totp(Base32Encoding.ToBytes(_settings.AuthSecret));

                var remainingSeconds = totpCode.RemainingSeconds();
                if (remainingSeconds < 5)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(remainingSeconds + 1));
                }

                var result = _authApi.Verify2FA(new TwoFactorAuthCode(totpCode.ComputeTotp()));

                if (result.Verified)
                {
                    _authApi = new AuthenticationApi(_configuration);
                    currentUser = _authApi.GetCurrentUser();

                    _logger.LogInformation("Successfully logged in as {vrChatUser}.", currentUser.DisplayName);
                }
            }
        }
        catch (ApiException e)
        {
            _logger.LogError("Exception when calling API: {errMsg}", e.Message);
        }
    }

    public Dictionary<string, string> SearchUser(string displayName)
    {
        if (_authApi?.GetCurrentUser() == null)
            Authenticate();

        try
        {
            UsersApi usersApi = new UsersApi(_configuration);

            var listOfUsers = usersApi.SearchUsers(displayName);

            if (listOfUsers != null && listOfUsers.Count > 0)
            {
                LimitedUser user = listOfUsers.First();

                var userDictionary = new Dictionary<string, string>
                {
                    { nameof(user.Id), user.Id },
                    { nameof(user.DisplayName), user.DisplayName }
                };

                return userDictionary;
            }
        }
        catch (ApiException e)
        {
            _logger.LogError("Exception when calling API: {errMsg}", e.Message); ;
        }

        return new Dictionary<string, string>();
    }

    public Dictionary<string, string> GetUser(string userId)
    {
        if (_authApi?.GetCurrentUser() == null)
            Authenticate();

        try
        {
            UsersApi usersApi = new UsersApi(_configuration);

            var user = usersApi.GetUser(userId);

            if (user != null)
            {
                var userDictionary = new Dictionary<string, string>
                {
                    { nameof(user.Id), user.Id },
                    { nameof(user.DisplayName), user.DisplayName },
                    { nameof(user.Bio), user.Bio },
                    { nameof(user.CurrentAvatarThumbnailImageUrl), user.ProfilePicOverride != string.Empty ? user.ProfilePicOverride : user.CurrentAvatarThumbnailImageUrl },
                    { nameof(user.CurrentAvatarImageUrl), user.CurrentAvatarImageUrl }
                };

                return userDictionary;
            }
        }
        catch (ApiException e)
        {
            _logger.LogError("Exception when calling API: {errMsg}", e.Message); ;
        }

        return new Dictionary<string, string>();
    }
}