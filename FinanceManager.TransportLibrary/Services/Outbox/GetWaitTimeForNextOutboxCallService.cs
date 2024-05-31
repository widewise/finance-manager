using FinanceManager.TransportLibrary.Models;
using NCrontab;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public interface IGetWaitTimeForNextOutboxCallService
{
    TimeSpan GetWaitTime();
}
public class GetWaitTimeForNextOutboxCallService : IGetWaitTimeForNextOutboxCallService
{
    private readonly CrontabSchedule _schedule;
    public GetWaitTimeForNextOutboxCallService(
        OutboxSettings settings)
    {
        _schedule = CrontabSchedule.Parse(settings.CronSchedule);
    }

    public TimeSpan GetWaitTime()
    {
        var now = DateTime.UtcNow;
        var nextRun = _schedule.GetNextOccurrence(now);
        return nextRun - now;
    }
}