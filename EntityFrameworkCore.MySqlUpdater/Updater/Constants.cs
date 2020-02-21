using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public enum UpdateStatusCodes
    {
        SUCCESS = 0x10,
        NOT_A_SQL_FILE = 0x20,
        INSECURE_SQL_QUERY = 0x30,
        EMPTY_CONTENT = 0x40,
        UPDATE_ALREADY_APPLIED = 0x50,
        MAX_ALLOWED_PACKAGE_LIMIT_TOO_SMALL = 0x60,
        SCHEMA_NOT_EMPTY = 0x70,
        UPDATE_TABLE_MISSING = 0x80,
        FILE_NOT_FOUND = 0x90,
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
