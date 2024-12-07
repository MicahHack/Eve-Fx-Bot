namespace AutoTrade.Models;

public class ExecuteTrade
{
    public string symbol { get; set; }
    public string actionType { get; set; }
    public double volume { get; set; }
    public double stopLoss { get; set; }
    public double takeProfit { get; set; }
    public string stopLossUnits { get; set; }
    public string takeProfitUnits { get; set; }
}