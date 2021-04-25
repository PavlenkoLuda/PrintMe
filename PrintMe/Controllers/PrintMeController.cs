using Microsoft.AspNetCore.Mvc;
using PrintMe.Logic;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace PrintMe.Controllers
{
    [ApiController]
    public class PrintMeController : ControllerBase
    {
        [HttpPost]
        [Route("printMeAt")]
        public async Task PrintMeAt(PrintMeContract contract)
        {
            if (string.IsNullOrWhiteSpace(contract?.Message) ||
                contract.Ts < DateTime.UtcNow)
            {
                Response.HttpContext.Response.StatusCode = 400;
                return;
            }

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<PrintJob>()
                .UsingJobData("Message", contract.Message)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString(), "PrintJob")
                .StartAt(DateTime.UtcNow.AddMilliseconds((contract.Ts - DateTime.UtcNow).TotalMilliseconds))
                .WithPriority(1)
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public class PrintMeContract
    {
        public string Message { get; set; }
        public DateTime Ts { get; set; }
    }
}
