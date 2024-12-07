using System.Runtime.CompilerServices;
using AutoTrade.Services;
using System.Threading;
using AutoTrade.Services.Implementation;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

namespace AutoTrade;

class Program
{

    public Program()
    {
    }
    
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("./Logs/logs.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        ITaskQueue queue = new TaskQueue();
        await queue.PrepQueue();
        
    }

}