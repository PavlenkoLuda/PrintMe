using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintMe.Storage
{
    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime Ts { get; set; }
    }
}
