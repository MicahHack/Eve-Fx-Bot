namespace AutoTrade.Models;

public class Candle
{
    public string symbol { get; set; }
    public string time { get; set; }
    public double open { get; set; }
    public double close { get; set; }
    public double high { get; set; }
    public double low { get; set; }
    public int tickVolume { get; set; }
    public int spread { get; set; }
    public string timeframe { get; set; }
    public string brokerTime { get; set; }
}