namespace AutoTrade.Services;

public interface IHttpService
{
    public Task<HttpResponseMessage> HttpGet(string urlPart, bool historicalData);
    public Task<HttpResponseMessage> HttpPost(string urlPart, dynamic content);

}