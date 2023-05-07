using EasyModbus;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CAY_Weighing
{
    internal static class PLC
    {
        public static string _Ip = "10.0.0.72";
        public static int _port = 502;

        public static ModbusClient modbusClient = new ModbusClient("127.0.0.1", 512);
        public static bool _connected { get; set; }

        public static bool[] _fillingstatus  = new bool[10];
        public static double[] _firstWeight = new double[10];
        public static double[] _lastWeight { get; set; }
        public static bool[] _nextfiilingStatus { get; set; }

        public static bool Connect()
        {
            if (_connected)
                return true;
            try
            {
                modbusClient.Connect();
                _connected = true;

                foreach (var silo in Silo.AllSilos)
                {
                    PLC.WriteCoil(8268 + silo._ıd, true);
                }

                Common.Logger.LogInfo("PLC Connected " +
                    _Ip.ToString() + ":" + _port.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Connection Error on {0}:{1} :\n{2}",
                    _Ip, _port, ex.ToString());
                return false;
            }
            return true;
        }
        public static bool Disconnect()
        {
            if (!_connected)
                return true;
            try
            {
                modbusClient.Disconnect();
                _connected = false;
                Console.WriteLine("PLC Disconnected {0}:{1}",
                    _Ip, _port);
                Common.Logger.LogInfo("PLC Disconnected " +
                   _Ip.ToString() + ":" + _port.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Disconnect Error {0}:{1} :\n{2}",
                    _Ip, _port, ex.ToString());
                Common.Logger.LogError("PLC Disconnect Error on " +
                   _Ip.ToString() + ":" + _port.ToString() + "\n" + ex.ToString());
                return false;
            }
            return true;
        }
        public static double GetMessage(Silo silo)
        {
            try
            {
                double total;
                int _id = silo._ıd;

                bool nextStatus = modbusClient.ReadCoils(_id + 8191, 1)[0];//değişicek

                if (nextStatus != _fillingstatus[_id])
                {
                    _fillingstatus[_id] = nextStatus;
                    if (nextStatus)
                    {
                        _firstWeight[_id] = silo.CurrentWeight;
                        return -1;
                    }  
                    else
                    {
                        total = silo.CurrentWeight - _firstWeight[_id];
                        return total;
                    }
                }
                return -1;
            }
            catch
            {
                Disconnect();
                return -1;
            }
        }
        public static bool WriteMessage(int register, int value)
        {
            if (!modbusClient.Connected)
            {
                Connect();
                return false;
            }
            try
            {
                modbusClient.WriteSingleRegister(register, value);
                Thread.Sleep(500);
                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }
        public static bool WriteCoil(int coil,bool value)
        {
            if (!modbusClient.Connected)
            {
                Connect();
                return false;
            }
            try
            {
                lock(modbusClient)
                {
                    modbusClient.WriteSingleCoil(coil, value);
                    Thread.Sleep(500);
                    Common.Logger.LogInfo("PLC Write Coil Başarılı");
                }
                return true;
            }
            catch
            {
                Common.Logger.LogInfo("PLC WriteCoil Başarısız " + coil);
                Disconnect();
                return false;
            }
        }
    }
}
