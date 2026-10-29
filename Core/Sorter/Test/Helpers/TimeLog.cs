using Helper.DataGathering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace Core.Sorter.Test.Helpers
{
    /// <summary>
    /// see how long it takes to do an action
    /// </summary>
    public class TimeLog
    {
        public TimeLog(string _name)
        {
            LastLog = DateTime.Now;
            Name = _name;
        }

        public static bool Logging = false;
        private DateTime LastLog { get; set; }
        public string Name { get; private set; }
        private int Flag { get; set; } = 1000;
        
        public void Log(string _action, string _message = " took time to do action: ")
        {
            if (!Logging) return;

            TimeSpan timeSinceLastLog = DateTime.Now - LastLog;
            Debug.WriteLine(Name + _message + timeSinceLastLog.TotalMilliseconds + $"ms for {_action}");
            if (timeSinceLastLog.TotalMilliseconds > Flag)
            {
                Debug.WriteLine($"\n↳ !!! TIME FLAG {Name} {_message} " +
                    $"{timeSinceLastLog.TotalMilliseconds}ms for {_action} !!! \n");
            }
            LastLog = DateTime.Now;
        }

        public void Start()
        {
            if (!Logging) return;
            LastLog = DateTime.Now;
        }
    }
}
