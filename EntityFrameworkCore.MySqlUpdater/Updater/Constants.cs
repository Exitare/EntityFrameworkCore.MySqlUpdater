using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public enum UpdateStatusCodes
    {
        SUCCESS = 0x00,
        NOT_A_SQL_FILE = 0x10,
        INSECURE_SQL_QUERY = 0x20,
        EMPTY_CONTENT = 0x30,
        UPDATE_ALREADY_APPLIED = 0x40,
        SCHEMA_NOT_EMPTY = 0x50,
        UPDATE_TABLE_MISSING = 0x60,
        FILE_NOT_FOUND = 0x70,
    }

    public enum SHAStatus
    {
        NOT_APPLIED = 0,
        EQUALS,
        CHANGED,
    }

    public class Constants
    {
        public static bool HashSumTracking = true;
    }
}
