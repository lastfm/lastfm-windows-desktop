using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class FileLogger
    {
        private static object _fileLock = new object();

        public static void Write(string pathAndFileName, string functionalArea, params string[] linesToWrite)
        {
            System.IO.StreamWriter logFile = null;

            try
            {
                lock (_fileLock)
                {
                    using (logFile = new System.IO.StreamWriter(pathAndFileName, true))
                    {
                        string joinedLines = string.Join(Environment.NewLine, linesToWrite);
                        logFile.Write($"{DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)}\t{functionalArea}\t{joinedLines}\r\n");
                        logFile.Close();
                        logFile.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (logFile != null && logFile.BaseStream != null)
                {
                    logFile.Close();
                    logFile.Dispose();
                }
            }
        }
    }
}
