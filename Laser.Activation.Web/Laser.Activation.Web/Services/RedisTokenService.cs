using FreeRedis;

namespace Laser.Activation.Web.Services;

public interface IRedisTokenService
{
    void StoreToken(string jti, int userId, TimeSpan expiry);
    bool IsTokenValid(string jti);
    void DeleteUserTokens(int userId);
    void ClearAllTokens();
}

public class RedisTokenService : IRedisTokenService
{
    private readonly RedisClient _redis;
    private const string TokenPrefix = "jwt:";
    private const string UserPrefix = "jwt_user:";

    public RedisTokenService(RedisClient redis)
    {
        _redis = redis;
    }

    public void StoreToken(string jti, int userId, TimeSpan expiry)
    {
        _redis.Set($"{TokenPrefix}{jti}", userId.ToString(), (int)expiry.TotalSeconds);
        _redis.SAdd($"{UserPrefix}{userId}", jti);
    }

    public bool IsTokenValid(string jti)
    {
        return _redis.Exists($"{TokenPrefix}{jti}");
    }

    public void DeleteUserTokens(int userId)
    {
        var userKey = $"{UserPrefix}{userId}";
        var jtis = _redis.SMembers(userKey);
        if (jtis is { Length: > 0 })
        {
            _redis.Del(jtis.Select(j => $"{TokenPrefix}{j}").ToArray());
        }
        _redis.Del(userKey);
    }

    public void ClearAllTokens()
    {
        var tokenKeys = _redis.Keys($"{TokenPrefix}*");
        var userKeys = _redis.Keys($"{UserPrefix}*");
        var allKeys = tokenKeys.Concat(userKeys).ToArray();
        if (allKeys.Length > 0)
        {
            _redis.Del(allKeys);
        }
    }
}
