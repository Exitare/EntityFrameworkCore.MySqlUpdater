using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EntityFrameworkCore.MySqlUpdater.Service
{
    public class FileService
    {
        private static FileService _class;
        public static FileService Class => _class ?? new FileService();


        public FileService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteConfigFile()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public XDocument ReadConfigFile()
        {
          
            try
            {
               return  XDocument.Load(@"config.xml");
            }
            catch (Exception ex)
            {
                CreateConfigFile();
                return XDocument.Load(@"config.xml");
            }
            
            
         
        }

        public void DebugMode(bool debugMode)
        {
            XDocument xml = ReadConfigFile();
            xml.XPathSelectElement("DebugMode")?.SetValue(debugMode);
            xml.Save(@"config.xml");
        }

        public bool DebugMode()
        {  
            
            // Query the data and write out a subset of contacts
            var query = from c in xml.Root.Descendants("contact")
                where (int)c.Attribute("id") < 4
                select c.Element("firstName").Value + " " +
                       c.Element("lastName").Value;


            foreach (string name in query)
            {
                Console.WriteLine("Contact's Full Name: {0}", name);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CreateConfigFile()
        {
            try
            {
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("This is a comment"),
                    new XElement("DebugMode", new XAttribute("active", "false")),
                    new XElement("StoreMode", new XAttribute("storeMode", "db")),
                    new XElement("Timeout", new XAttribute("timeout", 60)),
                    new XElement("Updates"));

                doc.Save(@"config.xml");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}