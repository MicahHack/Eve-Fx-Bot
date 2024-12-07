using System.Text;
using Serilog;

namespace AutoTrade.Services.Implementation;

public class LoggingService : ILoggingService
{

    public async Task LogTrade(string pair)
    {
        // ToDo: Seems to be an error when creating new daily file
        string path = Path.Combine("/var/www/AutotradeLogs", "Trades");
        string fileName = Path.Combine(path, $"Trades_{DateTime.Now.ToString("dd-MM-yyyy")}.txt");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(fileName))
        {
            File.Create(fileName).Close();
        }

        try
        {
            StringBuilder finalContent = new StringBuilder();
            finalContent.Append($"{pair},{DateTime.Now}" + Environment.NewLine);
            await File.AppendAllTextAsync(fileName, finalContent.ToString());
        }
        catch (Exception ex)
        {
            string error = "Error found while running LogTrade: " + ex.Message;
            Log.Error(error);
        }


    }

    public async Task<string> GetDailyTradesByPair(string pair)
    {
        List<string> results = new List<string>();
        string path = Path.Combine("/var/www/AutotradeLogs", "Trades");
        string fileName = Path.Combine(path, $"Trades_{DateTime.Now.ToString("dd-MM-yyyy")}.txt");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(fileName))
        {
            File.Create(fileName).Close();
        }

        try
        {
            var content = await File.ReadAllLinesAsync(fileName);
            if (content.Length > 0)
            {
                foreach (var line in content)
                {
                    if (line.ToLower().Contains(pair.ToLower()))
                    {
                        results.Add(line);
                    }
                }

                return results.Last();
            }

            return "";
        }
        catch (Exception ex)
        {
            string error = "Failed at GetDailyTradesByPair: " + ex.Message;
            Log.Error(error);
            return null;
        }

    }

}