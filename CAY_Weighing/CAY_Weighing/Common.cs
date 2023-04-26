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
    internal class Common 
    {
        public static Logger Logger;
        public static string GetloggerDir()
        {
            string fileName = "Logger.txt";
            string path = AppDomain.CurrentDomain.BaseDirectory;

            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                using (StreamWriter writer = File.CreateText(path))
                {
                    writer.WriteLine("-------------Application Logs---------------");
                }
            }
            Logger = new Logger(path);

            return path;
        }
       

    }

}
