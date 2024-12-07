using AutoTrade.Models;

namespace AutoTrade.Services;

public interface ITaskQueue
{
    public Task PrepQueue();
}