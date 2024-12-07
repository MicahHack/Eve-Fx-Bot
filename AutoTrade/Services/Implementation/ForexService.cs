using System.Text.Json;
using AutoTrade.Models;
using Serilog;
using talib = TicTacTec.TA.Library.Core;

namespace AutoTrade.Services.Implementation;

public class ForexService : IForexService
{
    private IHttpService _httpService;
    private ILoggingService _logging;
    private readonly string accountId = "Account ID Here";

    public ForexService()
    {
        this._httpService = new HttpService();
        this._logging = new LoggingService();
    }
    
    public async Task<List<string>> GetSymbols()
    {
        List<string> symbols = new List<string>();
        try
        {
            var request = await this._httpService.HttpGet($"users/current/accounts/{this.accountId}/symbols", false);
            if (request.IsSuccessStatusCode)
            {
                var response = await request.Content.ReadAsStringAsync();
                List<string> symbolsList = JsonSerializer.Deserialize<List<string>>(response);
                if (symbolsList != null && symbolsList.Count > 0)
                {
                    foreach (var pair in symbolsList)
                    {
                        symbols.Add(pair);
                    }
                }
            }
            return symbols;
        }
        catch (Exception ex)
        {
            string error = $"Error found while running GetSymbols: " + ex.Message;
            Log.Error(error);
            
            return symbols;
        }

    }

    public async Task<Candle> GetLiveCandle(string pair)
    {
        Candle candleData = new Candle();
        try
        {
            var request = await this._httpService.HttpGet($"users/current/accounts/{this.accountId}/historical-market-data/symbols/{pair}/timeframes/15m/candles?startTime={DateTime.Now}/&limit=2",
                true);
            string response = await request.Content.ReadAsStringAsync();
            List<Candle> candles = JsonSerializer.Deserialize<List<Candle>>(response);
            if (candles != null && candles.Count > 0)
            {
                foreach (var candle in candles)
                {
                    candleData = candle;
                }
            }
            return candleData;
            
        }
        catch (Exception ex)
        {
            string error = $"Error retrieving live candle for pair {pair}: " + ex.Message;
            Log.Error(error);
            return candleData;
        }
    }
    
    public async Task<List<Candle>> GetHistorical(string pair, string timeframe)
    {
        List<Candle> candleData = new List<Candle>();
        try
        {
            var request = await this._httpService.HttpGet(
                $"users/current/accounts/{this.accountId}/historical-market-data/symbols/{pair}/timeframes/{timeframe}/candles?startTime={DateTime.Now}/&limit=30",
                true);
            string response = await request.Content.ReadAsStringAsync();
            List<Candle> candles = JsonSerializer.Deserialize<List<Candle>>(response);
            if (candles != null && candles.Count > 0)
            {
                foreach (var candle in candles)
                {
                    candleData.Add(candle);
                }
            }

            return candleData;
        }
        catch (Exception ex)
        {
            string error = $"Error found while running GetHistorical: " + ex.Message;
            Log.Error(error);
            return candleData;
        }
    }

    public async Task<BollingerBands> AnalyzeBollingerBands(List<Candle> candles)
    {
        BollingerBands results = new BollingerBands();
        results.BullishBollinger = false;
        results.BearishBollinger = false;
        results.TimeFrame = candles[2].timeframe;
        try
        {
            double[] closePrices = candles.Select(c => c.close).ToArray();

            // Arrays to store the results
            int outIdx, outElements;
            double[] upperBand = new double[closePrices.Length];
            double[] middleBand = new double[closePrices.Length];
            double[] lowerBand = new double[closePrices.Length];
            talib.Bbands(0, closePrices.Length - 1, closePrices, 20, 2.0, 2.0, talib.MAType.Ema, out outIdx,
                out outElements, upperBand, middleBand, lowerBand);

            double[] upperBands = [];
            double[] lowerBands = [];
            upperBands = upperBand.Where(x => x != 0).TakeLast(3).ToArray();
            lowerBands = lowerBand.Where(x => x != 0).TakeLast(3).ToArray();
            candles = candles.TakeLast(3).ToList();

            try
            {
                var request = await this._httpService.HttpGet(
                    $"users/current/accounts/{this.accountId}/symbols/{candles[0].symbol.ToUpper()}/current-price",
                    false);
                var response = await request.Content.ReadAsStringAsync();
                PairSpecs? pairSpecs = JsonSerializer.Deserialize<PairSpecs>(response);
                if (!string.IsNullOrEmpty(pairSpecs.symbol))
                {
                    if (candles[2].timeframe == "4h")
                    {
                        var acceptableBearish = (candles[2].close + (candles[2].close * 0.0003));
                        var acceptableBullish = (candles[2].close - (candles[2].close * 0.0003));
                        if (acceptableBearish > upperBands[2])
                        {
                            results.BearishBollinger = true;
                        }
                        if (acceptableBullish < lowerBands[2])
                        {
                            results.BullishBollinger = true;
                        }
                        
                    }
                    if (candles[2].timeframe == "1h")
                    {
                        bool previousCloseAboveRange = candles[1].close > upperBands[1];
                        if (previousCloseAboveRange && candles[2].close > upperBands[2] && pairSpecs.bid > upperBands[2])
                        {
                            results.BearishBollinger = true;
                            // results.BullishBollinger = true;
                        }

                        bool previousCloseBelowRange = candles[1].close < lowerBands[1];
                        if (previousCloseBelowRange && candles[2].close < lowerBands[2] && pairSpecs.ask < upperBands[2])
                        {
                            results.BullishBollinger = true;
                            // results.BearishBollinger = true;
                        }
                    }
                    else
                    {
                        if (candles[1].close > upperBands[1] || candles[2].close > upperBands[2])
                        {
                            results.BearishBollinger = true;
                            // results.BullishBollinger = true;
                        }

                        if (candles[1].close < lowerBands[1] || candles[2].close < lowerBands[2])
                        {
                            results.BullishBollinger = true;
                            // results.BearishBollinger = true;
                        }
                    }        
                }
                
                return results;
            }
            catch (Exception ex)
            {
                string error = $"Error caught while attempting to get candle specification: " + ex.Message;
                Log.Error(error);
                return results;
            }
        }
        catch (Exception ex)
        {
            string error = $"Error found while running AnalyzeBollingerBands: " + ex.Message;
            Log.Error(error);
            return results;
        }
    }

    public async Task ExecTrade(string direction, string pair, double bid, double ask)
    {
        ExecuteTrade tradeOne = new ExecuteTrade();
        ExecuteTrade tradeTwo = new ExecuteTrade();

        tradeOne.symbol = pair;
        tradeOne.volume = 0.01;
        tradeOne.stopLossUnits = "ABSOLUTE_PRICE";
        tradeOne.takeProfitUnits = "ABSOLUTE_PRICE";

        tradeTwo.symbol = pair;
        tradeTwo.volume = 0.01;
        tradeTwo.stopLossUnits = "ABSOLUTE_PRICE";
        tradeTwo.takeProfitUnits = "ABSOLUTE_PRICE";
        
        // Buy
        if (direction == "Long")
        {
            // Calculate takeprofit and stoploss prices
            double takePrice = (ask + (ask * 0.0009));
            double takePriceTwo = (ask + (ask * 0.0004));
            double stopPrice = (bid - (bid * 0.0009));

            tradeOne.takeProfit = takePrice;
            tradeOne.stopLoss = stopPrice;
            tradeOne.actionType = "ORDER_TYPE_BUY";

            tradeTwo.takeProfit = stopPrice;
            tradeTwo.stopLoss = takePriceTwo;
            tradeTwo.actionType = "ORDER_TYPE_SELL";

        }
        // Sell
        if (direction == "Short")
        {
            double takePrice = (bid - (bid * 0.0009));
            double takePriceTwo = (bid - (bid * 0.0004));
            double stopPrice = (ask + (ask * 0.0009));

            tradeOne.takeProfit = takePrice;
            tradeOne.stopLoss = stopPrice;
            tradeOne.actionType = "ORDER_TYPE_SELL";
            
            tradeTwo.takeProfit = stopPrice;
            tradeTwo.stopLoss = takePriceTwo;
            tradeTwo.actionType = "ORDER_TYPE_BUY";
            
        }

        try
        {
            var jsonTradeOne = JsonSerializer.Serialize(tradeOne);
            var requestOne = await this._httpService.HttpPost($"users/current/accounts/{this.accountId}/trade", jsonTradeOne);
            
            var jsonTradeTwo = JsonSerializer.Serialize(tradeTwo);
            var requestTwo = await this._httpService.HttpPost($"users/current/accounts/{this.accountId}/trade", jsonTradeTwo);
            await this._logging.LogTrade(pair);
        }
        catch (Exception ex)
        {
            string error = $"Error Submitting Trade: " + ex.Message;
            Log.Error(error);
        }


    }

    public async Task<PairSpecs> GetPairSpecs(Candle candle)
    {
        PairSpecs? pairSpecs = new PairSpecs();
        try
        {
            var request = await this._httpService.HttpGet(
                $"users/current/accounts/{this.accountId}/symbols/{candle.symbol.ToUpper()}/current-price",
                false);
            var response = await request.Content.ReadAsStringAsync();
            pairSpecs = JsonSerializer.Deserialize<PairSpecs>(response);
            return pairSpecs;
        }
        catch (Exception ex)
        {
            Log.Error("Error caught while getting pair specs: " + ex.Message);
            return pairSpecs;
        }
    }

}