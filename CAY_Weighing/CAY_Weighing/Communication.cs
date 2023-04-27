using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyModbus;

namespace CAY_Weighing
{
    public class Communication
    {
        public int _id { get; set; }
        public string _Ip { get; set; }
        public int _port { get; set; }
        public bool _connected;
        public ModbusClient modbusClient { get; set; }
        public Silo _silo { get; set; }
        public object obj = new object();
        public Communication(Silo silo)
        {
            _id = silo._ıd;
            _Ip = silo._Ip;
            _port = silo._port;
            _silo=silo;
            modbusClient = new ModbusClient(_Ip,_port);
            //modbusClient.UnitIdentifier = (byte)this._id;
        }

        public Communication()
        {
            
        }
        public bool Connect()
        {
            if (Connected)
                return true;
            try
            {
                
                modbusClient.Connect();
                Connected = true;
                Console.WriteLine("Connected {0}:{1}",
                    this._Ip, this._port);
                Common.Logger.LogInfo("Connected " +
                    this._Ip.ToString() + ":" + this._port.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection Error on {0}:{1} :\n{2}",
                    this._Ip, this._port, ex.ToString());
                return false;
            }
            return true;
        }
        public bool Disconnect()
        {
            if (!Connected)
                return true;
            try
            {
                modbusClient.Disconnect();
                Connected = false;
                Console.WriteLine("Disconnected {0}:{1}",
                    this._Ip, this._port);
                Common.Logger.LogInfo("Disconnected " +
                   this._Ip.ToString() + ":" + this._port.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnect Error {0}:{1} :\n{2}",
                    this._Ip, this._port, ex.ToString());
                Common.Logger.LogError("Disconnect Error on " +
                   this._Ip.ToString() + ":" + this._port.ToString() + "\n" + ex.ToString());
                return false;
            }
            return true;
        }
        public int[] GetMessage()
        {
            try
            {
                return modbusClient.ReadHoldingRegisters(7, 2);
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
                Silo.AllSilos.FirstOrDefault(s => s._ıd == this._id).Disconnect(); // sahada değişecek(_port kısmı _ip veya id olacak)
                return null;
            }
        }
        public bool WriteMessage(int register,int value)
       {
            if (!modbusClient.Connected)
            {
                this.Connect();
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
                _silo.Disconnect();
                return false;
            }
        }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnConnectedChanged(new ConnectedChangedEventArgs(_silo, _connected));
                _silo.OnPropertyChanged("BlinkColor");
                _silo.OnPropertyChanged("ConnectionColor");
            }
        }
        public event EventHandler<ConnectedChangedEventArgs> ConnectedChanged;

        public virtual void OnConnectedChanged(ConnectedChangedEventArgs e)
        {
            if (ConnectedChanged != null)
                ConnectedChanged(this._silo, e);
        }
    }
    public class ConnectedChangedEventArgs : EventArgs
    {
        public readonly Silo silo;
        public readonly bool NewState;
        public ConnectedChangedEventArgs(Silo silo, bool NewState)
        {
            this.silo = silo;
            this.NewState = NewState;
        }
    }
}
