using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQLDBUpdater
{
    public partial class MySQLDBUpdater
    {

        /// <summary>
        /// Returns the occurence of a given word in a given content string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        private int GetWordOccurence(string content, string searchTerm)
        {
            string[] source = content.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Create the query.  Use ToLowerInvariant to match "data" and "Data"   
            var matchQuery = from word in source
                             where word.ToLowerInvariant() == searchTerm.ToLowerInvariant()
                             select word;

            return matchQuery.Count();
        }


        /// <summary>
        /// Validates the sql content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool ValidateContent(string content, uint occurenceCount = 10)
        {
            if (content.ToUpper().Contains("DROP * "))
                return false;

            if (content.ToUpper().Contains("DELETE * "))
                return false;

            if (content.ToUpper().Contains("TRUNCATE"))
                return false;


            if (GetWordOccurence(content, "delete") >= occurenceCount)
                return false;

            if (GetWordOccurence(content, "DROP *") >= occurenceCount)
                return false;


            return true;
        }

    }
}
