using PrintMe.Storage;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintMe.Logic
{
    public class StartProcessing
    {
        private readonly MessageStorage _messageStorage;
        private readonly JobFactory _jobFactory;
        public StartProcessing(MessageStorage messageStorage, JobFactory jobFactory)
        {
            _messageStorage = messageStorage;
            _jobFactory = jobFactory;
        }

        public async Task Process()
        {
            var messages = await _messageStorage.GetAllMessages();

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            scheduler.JobFactory = _jobFactory;
            await scheduler.Start();

            foreach (var message in messages)
            {
                if (string.IsNullOrWhiteSpace(message.Id))
                    return;

                if (message.Ts > DateTime.UtcNow)
                {
                    IJobDetail job = JobBuilder.Create<PrintJob>()
                        .UsingJobData("Id", message.Id)
                        .Build();

                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), "PrintJob")
                        .StartAt(DateTime.UtcNow.AddMilliseconds((message.Ts - DateTime.UtcNow).TotalMilliseconds))
                        .WithPriority(1)
                        .Build();

                    await scheduler.ScheduleJob(job, trigger);
                }
                else
                {
                    Console.WriteLine(message.Text);

                    await _messageStorage.RemoveMessage(message.Id);
                }
            }
        }
    }
}