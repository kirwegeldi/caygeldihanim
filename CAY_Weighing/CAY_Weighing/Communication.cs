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
        private bool _clientBusy = false;
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
                Common.Logger.LogInfo("Silo " + this._id + " Connected\t"   +
                    this._Ip + ":" + this._port.ToString());
            }
            catch
            {
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
                Common.Logger.LogInfo("Silo " + this._id + " Disconnected\t" +
                   this._Ip.ToString() + ":" + this._port.ToString());
            }
            catch
            {
                return false;
            }
            return true;
        }
        public int[] GetMessage()
        {
            try
            {
                while (_clientBusy) ;
                _clientBusy = true;
                var result = modbusClient.ReadHoldingRegisters(7, 4);
                _clientBusy = false;
                return result;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError("Silo " + this._id + " GetMessage Error " + "\n" + ex.Message);
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
                while (_clientBusy);
                _clientBusy = true;
                modbusClient.WriteSingleRegister(register, value);
                _clientBusy = false;
                Thread.Sleep(500);
                return true;
            }
            catch(Exception ex)
            {
                Common.Logger.LogError("Silo " + this._id + " WriteMessage Error " + "\n" + ex.Message);
                _silo.Disconnect();
                return false;
            }
        }
        public bool Connected
        {
            get => _connected;
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
