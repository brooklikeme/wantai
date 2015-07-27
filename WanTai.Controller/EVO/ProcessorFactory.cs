using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.Controller.EVO
{
    public class ProcessorFactory
    {
        public static bool HasInitedProcessor = false;
        static bool isMock = WanTai.Common.Configuration.GetIsMock();
        public static bool HasClosed = false;

        public static IProcessor GetProcessor()
        {
            HasInitedProcessor = true;
            if (isMock)
            {
                return MockProcessor.Instance();
            }
            else
            {                
                return EVOApiProcessor.Instance();
            }            
        }

        public static DateTime GetDateTimeNow()
        {
            if (isMock)
            {
                return DateTime.MinValue;
            }
            else
            {
                return DateTime.Now;
            }
        }
    }
}
