using Quartz;
using System;
using System.Threading.Tasks;

namespace PrintMe.Logic
{
    public class PrintJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine(context.MergedJobDataMap.Get("Message").ToString());
        }
    }
}
