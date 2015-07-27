using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WanTai.DataModel
{
    public class WanTaiObjectService
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        static extern int UuidCreateSequential(out Guid guid);

        /// <summary>
        /// Get the sequential GUID.
        /// </summary>
        /// <returns>Return a sequential GUID.</returns>
        public static Guid NewSequentialGuid()
        {
            const int RPC_S_OK = 0;
            Guid g;
            int hr = UuidCreateSequential(out g);
            if (hr != RPC_S_OK)
            {
                //if sequential generate failed, then just return the random Guid.
                g = Guid.NewGuid();
            }
            return g;
        }

    }
}
