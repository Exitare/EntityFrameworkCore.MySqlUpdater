using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public enum UpdateStatus: uint
    {
        NotApplied = 0,
        Equals,
        Changed,
    }

    public enum Mode : uint
    {
        FileHashSum,
        FileName,
    }

    public static class Constants
    {
        public static bool HashSumTracking = true;
        public static uint SqlTimeout = 60;
        public static bool DebugOutput = false;
    }
    
}
