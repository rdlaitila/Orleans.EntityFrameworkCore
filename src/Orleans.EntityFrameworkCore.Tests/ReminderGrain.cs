using System;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.EntityFrameworkCore.Tests
{
    public interface IReminderGrain : IGrainWithGuidKey
    {
        Task WakeUp();

        Task<bool> ReminderCalled();
    }

    public class ReminderGrain : Grain, IReminderGrain, IRemindable
    {
        private bool _reminderCalled = false;

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == "test")
                _reminderCalled = true;

            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            await RegisterOrUpdateReminder(
                "test",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(1)
            );
        }

        public Task WakeUp()
        {
            return Task.CompletedTask;
        }

        public Task<bool> ReminderCalled()
        {
            return Task.FromResult(_reminderCalled);
        }
    }
}