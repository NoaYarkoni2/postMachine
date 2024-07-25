using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpCaller
{
    public class Config
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string JsonPayload { get; set; }
        public int Interval { get; set; }
        public int Duration { get; set; }
        public int NumberOfThreads { get; set; }
    }
}
