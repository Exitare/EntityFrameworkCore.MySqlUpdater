using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLDBUpdater
{
    public enum UpdateStatusCodes
    {
        NO_MATCHING_PATH = 0x00,
        ERROR_EXECUTING_SQL = 0x10,
        SUCCESS = 0x20,
        NOT_A_SQL_FILE = 0x30,
        INSECURE_SQL_QUERY = 0x40,
        EMPTY_CONTENT = 0x50,
        UPDATE_ALREADY_APPLIED = 0x60,
    }

    public enum SHAStatus
    {
        NOT_APPLIED = 0,
        EQUALS,
        CHANGED,
    }

}
