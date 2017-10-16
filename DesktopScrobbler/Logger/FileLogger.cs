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
        public static void Write(string pathAndFileName, string functionalArea, params string[] linesToWrite)
        {
            System.IO.StreamWriter logFile = null;

            try
            {
                using (logFile = new System.IO.StreamWriter(pathAndFileName, true))
                {
                    string joinedLines = string.Join(Environment.NewLine, linesToWrite);
                    logFile.Write($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}\t{functionalArea}\t{joinedLines}\r\n");
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
