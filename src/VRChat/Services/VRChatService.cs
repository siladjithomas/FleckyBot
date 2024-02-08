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

    public VRChatService(ILogger<VRChatService> logger, IServiceScopeFactory scopeFactory, VRChatSettings settings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _settings = settings;

        _configuration = new Configuration
        {
            Username = settings.Username,
            Password = settings.Password,
            UserAgent = "FleckyBot/2.1.0 hello@tsuyabashi.dev"
        };
    }

    public async Task Authenticate()
    {
        try
        {
            AuthenticationApi authApi = new AuthenticationApi(_configuration);
            CurrentUser currentUser = await authApi.GetCurrentUserAsync();

            if (currentUser == null)
            {
                // TODO: get the latest totp token
                Totp oneTime = new Totp(Base32Encoding.ToBytes(_settings.AuthSecret));

                var remainingSeconds = oneTime.RemainingSeconds();
                if (remainingSeconds < 5)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(remainingSeconds + 1));
                }

                var result = await authApi.Verify2FAAsync(new TwoFactorAuthCode(oneTime.ComputeTotp()));

                if (result.Verified)
                {
                    currentUser = await authApi.GetCurrentUserAsync();
                }
            }
        }
        catch (ApiException e)
        {
            _logger.LogError("Exception when calling API: {0}", e.Message);
            _logger.LogError("Status Code: {0}", e.ErrorCode);
            _logger.LogDebug(e.ToString());
        }
    }
}