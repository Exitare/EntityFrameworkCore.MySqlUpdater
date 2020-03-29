using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public enum SHAStatus
    {
        NOT_APPLIED = 0,
        EQUALS,
        CHANGED,
    }

    public class Constants
    {
        public static bool HashSumTracking = true;
        public static int SQLTimeout = 60;
        public static bool DebugOutput = false;
    }
}
