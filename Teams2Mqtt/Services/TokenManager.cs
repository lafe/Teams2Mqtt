using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace lafe.Teams2Mqtt.Services;

public interface ITokenManager
{
    /// <summary>
    /// The current token for communication with Teams
    /// </summary>
    string? Token { get; }

    /// <summary>
    /// Initializes the <see cref="TokenManager"/> and populates the <see cref="Token"/>
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the stored token with the new value
    /// </summary>
    /// <param name="newToken">The new token that has been received from Teams</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task UpdateToken(string newToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stores the token for communication with Teams and updates it when a new one is received. The token is persisted in an cache-file that is encrypted with the machine key. 
/// The location of the cache-file is %LOCALAPPDATA%\lafe\Teams2Mqtt\TokenCache.dat
/// </summary>
public class TokenManager : ITokenManager
{
    protected ILogger<TokenManager> Logger { get; }
    protected IDataProtectionProvider Provider { get; }
    protected IDataProtector DataProtector { get; }

    public TokenManager(ILogger<TokenManager> logger, IDataProtectionProvider provider)
    {
        Logger = logger;
        Provider = provider;
        DataProtector = provider.CreateProtector("TokenManager");
    }

    /// <summary>
    /// The current token for communication with Teams
    /// </summary>
    public string Token { get; protected set; } = string.Empty;

    protected string TokenCacheFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lafe", "Teams2Mqtt", "TokenCache.dat");

    /// <summary>
    /// Initializes the <see cref="TokenManager"/> and populates the <see cref="Token"/>
    /// </summary>
    public void Initialize()
    {
        // Load content of file %LOCALAPPDATA%\lafe\Teams2Mqtt\TokenCache.dat (if it is present) and decrypt it with the machine key
        using var scope = Logger.BeginScope($"{nameof(TokenManager)}:{nameof(Initialize)}");
        try
        {
            Logger.LogTrace(LogNumbers.TokenManager.Initialize, $"Loading token from \"{TokenCacheFile}\"");
            if (!File.Exists(TokenCacheFile))
            {
                Logger.LogWarning(LogNumbers.TokenManager.InitializeTokenMissing, $"The token cache file \"{TokenCacheFile}\" could not be found.");
                return;
            }
            Logger.LogTrace(LogNumbers.TokenManager.InitializeLoadingCacheFileContent, $"Loading content from file \"{TokenCacheFile}\"");
            var encryptedToken = File.ReadAllBytes(TokenCacheFile);
            Logger.LogTrace(LogNumbers.TokenManager.InitializeDecryptingContent, $"Decrypting content from file \"{TokenCacheFile}\"");
            var decryptedTokenBytes = DataProtector.Unprotect(encryptedToken);
            Logger.LogTrace(LogNumbers.TokenManager.InitializeDecodingByteArray, $"Decoding byte array");
            var decryptedToken = Encoding.UTF8.GetString(decryptedTokenBytes);
            Logger.LogTrace(LogNumbers.TokenManager.InitializeUsingNewToken, $"Using new token \"{decryptedToken}\"");
            Token = decryptedToken;
            Logger.LogTrace(LogNumbers.TokenManager.InitializeSuccess, $"Loaded token from cache file \"{TokenCacheFile}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TokenManager.InitializeException, ex, $"Failed to load token from cache file \"{TokenCacheFile}\": {ex}");
            throw;
        }
    }

    /// <summary>
    /// Updates the stored token with the new value
    /// </summary>
    /// <param name="newToken">The new token that has been received from Teams</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task UpdateToken(string newToken, CancellationToken cancellationToken = default)
    {
        using var scope = Logger.BeginScope($"{nameof(TokenManager)}:{nameof(UpdateToken)}");
        try
        {
            Logger.LogTrace(LogNumbers.TokenManager.UpdateToken, $"New token \"{newToken}\" received");
            lock (Token)
            {
                Token = newToken;
            }

            Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenUpdatedToken, $"Updated token internally");

            // Persist token to disk in file %LOCALAPPDATA%\lafe\Teams2Mqtt\TokenCache.dat. The file is encrypted with the machine key.
            Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenTokenCacheFile, $"Using file \"{TokenCacheFile}\" as token cache.");
            var tokenCacheDirectory = Path.GetDirectoryName(TokenCacheFile);
            if (!string.IsNullOrWhiteSpace(tokenCacheDirectory) && !Directory.Exists(tokenCacheDirectory))
            {
                Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenFolderMissing, $"Folder \"{tokenCacheDirectory}\" does not exist yet. Creating it.");
                Directory.CreateDirectory(tokenCacheDirectory!);
                Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenCreatedFolder, $"Created folder \"{tokenCacheDirectory}\" to store cache file.");
            }

            // Store token in file and encrypt it with the machine key
            var tokenBytes = Encoding.UTF8.GetBytes(newToken);
            Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenConvertedToken, $"Converted token to byte array");
            var encryptedToken = DataProtector.Protect(tokenBytes);
            Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenEncryptedToken, $"Encrypted token");
            await File.WriteAllBytesAsync(TokenCacheFile, encryptedToken, cancellationToken);
            Logger.LogTrace(LogNumbers.TokenManager.UpdateTokenSuccess, $"Updated cache file \"{TokenCacheFile}\" with new token");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TokenManager.UpdateTokenException, ex, $"An error occurred while trying to update token: {ex}");
            throw;
        }
    }
}