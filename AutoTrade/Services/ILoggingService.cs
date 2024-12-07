namespace AutoTrade.Services;

public interface ILoggingService
{
    public Task LogTrade(string pair);
    public Task<string> GetDailyTradesByPair(string pair);
}