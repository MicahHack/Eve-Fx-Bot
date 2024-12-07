using AutoTrade.Models;

namespace AutoTrade.Services;

public interface IForexService
{
    public Task<List<string>> GetSymbols();
    public Task<Candle> GetLiveCandle(string pair);
    public Task<List<Candle>> GetHistorical(string pair, string timeframe);
    public Task<BollingerBands> AnalyzeBollingerBands(List<Candle> candles);
    public Task ExecTrade(string direction, string pair, double bid, double ask);
    public Task<PairSpecs> GetPairSpecs(Candle candle);
}