using System.Text.Json;
using AutoTrade.Models;
using Serilog;

namespace AutoTrade.Services.Implementation;

public class TaskQueue : ITaskQueue
{
    private IForexService _fxService;
    private ILoggingService _logging;

    public TaskQueue()
    {
        this._fxService = new ForexService();
        this._logging = new LoggingService();
    }

    public async Task PrepQueue()
    {
        try
        {
            await RunAnalysis();
        }
        catch (Exception ex)
        {
            Log.Error("Application Failed with error: " + ex.Message);
        }

    }

    private async Task RunAnalysis()
    {
        Console.WriteLine($"Starting Analysis {DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss")}");
        
        // Filter through symbols
        List<string> allSymbols = await this._fxService.GetSymbols();
        List<string> symbols = new List<string>();
        foreach (var sym in allSymbols)
        {
            if ((sym.Contains("EUR") || sym.Contains("USD") || sym.Contains("JPY") || sym.Contains("CAD")) && !sym.Contains("#"))
            {
                // Check Spread
                Candle tempCandle = await this._fxService.GetLiveCandle(sym);
                if (tempCandle.spread <= 23 && tempCandle.spread > 0 && tempCandle.symbol.Length >= 3)
                {
                    symbols.Add(sym);
                }
            }
            if (sym.Contains("US100") || sym.Contains("XRPUSD") || sym.Contains("ETHUSD") || sym.Contains("USDZAR"))
            {
                symbols.Add(sym);
            }
            
        }

        // Get historic data for each pair and analyze.
        // List<string> timeFrames = ["15m", "30m", "1h", "4h"];
        List<string> timeFrames = ["15m", "30m", "1h"];
        foreach (var pair in symbols)
        {
            List<Candle> candles = new List<Candle>();
            List<BollingerBands> bollingerBands = new List<BollingerBands>();
            foreach (var time in timeFrames)
            {
                // Get Candle Data
                candles.Clear();
                candles = await this._fxService.GetHistorical(pair, time);
                
                // Analyze Bollinger Bands & add to bollinger bands list
                if (candles.Count >= 20)
                {
                    BollingerBands analysis = await this._fxService.AnalyzeBollingerBands(candles);
                    if (analysis.BullishBollinger || analysis.BearishBollinger)
                    {
                        bollingerBands.Add(analysis);
                    }
                }
            }
            
            // confirm if all timeframes have matching bollinger band areas.
            try
            {
                bool allTimesExist = timeFrames.All(timeframe => bollingerBands.Any(x => x.TimeFrame == timeframe));
                bool executeTrade = false;

                if (allTimesExist)
                {
                    // Check if trade has been made on currency pair recently.
                    string tradeData = await this._logging.GetDailyTradesByPair(pair);
                    if (tradeData == "")
                    {
                        executeTrade = true;
                    }
                    if (tradeData.Length > 0)
                    {
                        string[] splitLine = tradeData.Split(',');
                        DateTime tradeDate = Convert.ToDateTime(splitLine[1]);
                        DateTime futureTradeDate = tradeDate.AddHours(2);
                        DateTime comparisonDate = DateTime.Now;
                        if (splitLine[0].ToLower().Contains(pair.ToLower()) && comparisonDate > futureTradeDate)
                        {
                            executeTrade = true;
                        }
                    }

                    // If no trade is found for pair, or trade expired, make a buy/sell (Long/Short) here
                    if (executeTrade)
                    {
                        // Get pair specs for inspection of stoploss/takeprofit values
                        PairSpecs specs = await this._fxService.GetPairSpecs(candles.Last());
                        
                        // Determine trade direction
                        bool longTrade = bollingerBands.All(x => x.BullishBollinger);
                        bool shortTrade = bollingerBands.All(x => x.BearishBollinger);
                        if (longTrade)
                        {
                            await this._fxService.ExecTrade("Long", pair, specs.bid, specs.ask);
                        }
                        if (shortTrade)
                        {
                            await this._fxService.ExecTrade("Short", pair, specs.bid, specs.ask);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string error = $"Error caught while checking & executing trades: " + ex.Message;
                Log.Error(error);
            }

        }
        Console.WriteLine($"Ended Analysis {DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss")}");
    }
    
}