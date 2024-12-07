using System.Net.Http.Json;
using System.Text.Json;
using Serilog;

namespace AutoTrade.Services.Implementation;

public class HttpService : IHttpService
{
    public HttpService() { }
    
    public async Task<HttpResponseMessage> HttpGet(string urlPart, bool historicUrl)
    {
        var token = "API Key Here";
        
        var baseUrl = "https://mt-client-api-v1.london.agiliumtrade.ai/";
        if (historicUrl)
        {
            baseUrl = "https://mt-market-data-client-api-v1.london.agiliumtrade.ai/";
        }

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("auth-token", $"{token}");
            var newUrl = new Uri(baseUrl + urlPart);

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(newUrl);
                return response;

            }
            catch (Exception ex)
            {
                string error = $"Error sending http get: " + ex.Message;
                Log.Error(error);
                HttpResponseMessage responsemsg = new HttpResponseMessage();
                return responsemsg;
            }
        }

    }

    public async Task<HttpResponseMessage> HttpPost(string urlPart, dynamic content)
    {
        var token = "API Key Here";
        var baseUrl = "https://mt-client-api-v1.london.agiliumtrade.ai/";
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("auth-token", $"{token}");
                var newUrl = new Uri(baseUrl + urlPart);

                HttpResponseMessage response = await httpClient.PostAsync(newUrl, body);
                return response;
            }
        }
        catch (Exception ex)
        {
            string error = $"Error with Http Post: " + ex.Message;
            Log.Error(error);
            HttpResponseMessage responsemsg = new HttpResponseMessage();
            return responsemsg;
        }

    }
}