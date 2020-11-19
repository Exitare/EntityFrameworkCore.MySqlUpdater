using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public enum SHAStatus: uint
    {
        NotApplied = 0,
        Equals,
        Changed,
    }

    public class Constants
    {
        public static bool HashSumTracking = true;
        public static uint SQLTimeout = 60;
        public static bool DebugOutput = false;
    }
}
