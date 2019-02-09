using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.App
{
    public interface IReminderGrain : IGrainWithGuidKey
    {
        Task SetReminder();

        Task<ReminderState> GetState();
    }

    public class ReminderGrain : Grain<ReminderState>, IReminderGrain, IRemindable
    {
        public Task SetReminder()
        {
            return RegisterOrUpdateReminder(
                "test",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(1)
            );
        }

        public Task<ReminderState> GetState()
        {
            return Task.FromResult(State);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            State.NumReminderCalled++;
            return WriteStateAsync();
        }
    }
}