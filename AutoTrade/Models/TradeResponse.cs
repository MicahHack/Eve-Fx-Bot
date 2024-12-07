namespace AutoTrade.Models;

public class TradeResponse
{
    public string numericCode { get; set; }
    public string stringCode { get; set; }
    public string message { get; set; }
    public int orderId { get; set; }
    public int positionId { get; set; }
}