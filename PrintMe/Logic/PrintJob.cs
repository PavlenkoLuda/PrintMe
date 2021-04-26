using PrintMe.Controllers;
using PrintMe.Storage;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintMe.Logic
{
    public class PrintJob : IJob
    {
        private readonly MessageStorage _messageStorage;
        public PrintJob(MessageStorage messageStorage)
        {
            _messageStorage = messageStorage;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string id = context.MergedJobDataMap.Get("Id").ToString();

            if (!(await _messageStorage.HasMessage(id)))
                return;

            var message = await _messageStorage.GetMessage(id);

            Console.WriteLine(message.Text);

            await _messageStorage.RemoveMessage(id);
        }
    }
}
