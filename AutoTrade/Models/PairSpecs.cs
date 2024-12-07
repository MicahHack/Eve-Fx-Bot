namespace AutoTrade.Models;

public class PairSpecs
{
    public string symbol { get; set; }
    public double bid { get; set; }
    public double ask { get; set; }
    public double profitTickValue { get; set; }
    public double lossTickValue { get; set; }
    public string time { get; set; }
    public string brokerTime { get; set; }
}