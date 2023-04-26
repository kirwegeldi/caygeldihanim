using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CAY_Weighing
{
    internal class Logger
    {
        /// <summary>
        /// Logger Operation
        /// </summary>
        private string _path;
        private object _lock = new object();

        public Logger(string path)
        {
            _path = path;
        }

        public void Log(string time, string row, string lable)
        {
            lock (_lock)
            {
                int s = File.ReadLines(_path).Count();
                if (s < 500)
                {
                    using (var fs = new FileStream(_path, FileMode.Append))
                    {

                        StreamWriter sw = new StreamWriter(fs);
                        var logString = $"{time} - #{lable} - {row}";
                        sw.WriteLine(logString);
                        sw.Close();
                        fs.Close();
                        //fs.Dispose();
                    }
                }
                else
                {
                    LogDelete();
                }
            }
        }
        public bool LogDelete()
        {
            try
            {
                File.WriteAllText(_path, String.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void LogInfo(string row)
        {
            string time = DateTime.Now.ToString();
            Log(time, row, "Info");
        }

        public void LogError(string row)
        {
            string time = DateTime.Now.ToString();
            Log(time, row, "Error");
        }
    }
}
