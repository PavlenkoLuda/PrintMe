using Microsoft.AspNetCore.Mvc;
using PrintMe.Logic;
using PrintMe.Storage;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintMe.Controllers
{
    [ApiController]
    public class PrintMeController : ControllerBase
    {
        private readonly MessageStorage _messageStorage;
        private readonly JobFactory _jobFactory;
        public PrintMeController(MessageStorage messageStorage, JobFactory jobFactory)
        {
            _messageStorage = messageStorage;
            _jobFactory = jobFactory;
        }

        [HttpPost]
        [Route("printMeAt")]
        public async Task PrintMeAt(PrintMeContract contract)
        {
            if (string.IsNullOrWhiteSpace(contract?.Text) ||
                contract.Ts < DateTime.UtcNow)
            {
                Response.HttpContext.Response.StatusCode = 400;
                return;
            }

            var message = new Message() 
            { 
                Id = Guid.NewGuid().ToString(),
                Text = contract.Text,
                Ts = contract.Ts,
            };

            await _messageStorage.AddMessage(message.Id, message);

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            scheduler.JobFactory = _jobFactory;
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<PrintJob>()
                .UsingJobData("Id", message.Id)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString(), "PrintJob")
                .StartAt(DateTime.UtcNow.AddMilliseconds((contract.Ts - DateTime.UtcNow).TotalMilliseconds))
                .WithPriority(1)
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public class PrintMeContract
        {
            public string Text { get; set; }
            public DateTime Ts { get; set; }
        }
    }
}
