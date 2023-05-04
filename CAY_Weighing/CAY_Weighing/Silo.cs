using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using EasyModbus;
using Microsoft.Office.Interop.Excel;

namespace CAY_Weighing
{
    public class Silo : INotifyPropertyChanged
    {
        public static List<Silo> ConnectedSilos = new List<Silo>();
        public static List<Silo> DisconnectedSilos = new List<Silo>();
        public static List<Silo> AllSilos = new List<Silo>();
        public static List<Silo> Hat1 = new List<Silo>();
        public static List<Silo> Hat2 = new List<Silo>();

        private static double upperLimit = 1400;    //for blink
        private static double lowerLimit = 300;


        public int _ıd { get; set; }
        public string _Ip { get; set; }
        public int _port { get; set; }
        public Communication modbusComm { get; set; }

        public double _currentWeight;
        private double _brutWeight;
        public int _valueLow;
        public int _valueHigh;
        public bool _isActive = true;
        private bool _blinkStatic;                      // if current weight is below the lower limit it remanins true
        private bool _blink;                            // if current weight is below the lower limit it changes !_blink
        private bool _completed = true;                 // plc start verildiğinde filling miktarı sete geldiğinde true verir.
        public double _firstWeight;                    // plc wtm rölesi açıldığındaki ağırlık

        public Silo(int ıd, string ip,int port)
        {
            _ıd = ıd;
            _Ip = ip;
            _port = port;
            modbusComm = new Communication(this);
        }
        public Silo()
        {
            modbusComm = new Communication();

        }
        public void Connect()
        {
            if (!_isActive)
                return;
            modbusComm.Connect();
            if (modbusComm._connected)
            {
                AddRemove(ConnectedSilos, DisconnectedSilos);
                return;
            }
            AddRemove(DisconnectedSilos, ConnectedSilos);
        }
        public void Disconnect() 
        { 
            if(modbusComm.Disconnect())
            {
                AddRemove(DisconnectedSilos, ConnectedSilos);
                return;
            }
            AddRemove(ConnectedSilos, DisconnectedSilos);
        }
        public double GetMessage()
        {
            if (!_isActive)
                return _currentWeight;
            int[] value = modbusComm.GetMessage();
            if (value != null)
            {
                _currentWeight = value[3] / 10.0;
                _brutWeight = value[1] / 10.0;
                return _currentWeight;
            }
            AddRemove(DisconnectedSilos, ConnectedSilos);
            return _currentWeight;
        }
        public void UpdateUI()
        {
            CurrentWeight = _currentWeight;
            BrutWeight = _brutWeight;
        }
        public async void listenPLC()
        {
            await Task.Run(() => {
                if (this.Connected && this.IsActive && _valueLow>0)
                {
                    this.Completed = false;
                    PLC.WriteCoil(8268 + this._ıd, false);
                    while (_currentWeight > -1*_valueLow)
                    {
                        if (Completed || !Connected || !IsActive)
                            break;
                        Task.Delay(50);
                    }
                    Completed = true;
                    PLC.WriteCoil(8268 + this._ıd, true);
                }
                });
        }
        public bool WriteTare()
        {
            if (!_isActive)
                return false;
            try
            {
                var result = modbusComm.WriteMessage(88, 2);
                return result;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }
        public bool WriteZero()
        {
            if (!_isActive)
                return false;
            try
            {
                var result = modbusComm.WriteMessage(88, 1);
                return result;
            }
            catch 
            {

               Disconnect();
                return false;
            }
            
        }
        public bool WriteValueLow()
        {
            if (!_isActive)
                return false;
            try
            {
                var result = modbusComm.WriteMessage(19, _valueLow);
                return result;
            }
            catch
            {
                Disconnect();
                return false;
            }
            
            
        }
        public bool WriteValueHigh()
        {
            if (!_isActive)
                return false;
            return modbusComm.WriteMessage(40019, _valueHigh);
        }
        private void AddRemove(List<Silo> addList, List<Silo> removeList)
        {
            if (!addList.Contains(this))
                addList.Add(this);
            if(removeList.Contains(this))
                removeList.Remove(this);
        }

        #region Binding Properties

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public bool Connected
        {
            get => modbusComm._connected;
        }
        public double CurrentWeight
        {
            get => _currentWeight;
            set
            {
                _currentWeight = value;
                
                OnPropertyChanged("CurrentWeight");
            }
        }
        public double BrutWeight { 
            get => _brutWeight;
            set
            {
                _brutWeight = value;
                CurrentHeight = value > 0 ? (int)value : 0;
                if (_ıd == 9 || _ıd == 10)
                    Blink = _brutWeight > upperLimit ? true : false;
                else
                {
                    Blink = _brutWeight < lowerLimit ? true : false;
                }
                OnPropertyChanged("BrutWeight");
            }
         }
        public int CurrentHeight
        {
            get => (this._ıd ==9 || this._ıd == 10) ? (int)_brutWeight / 13: (int)_brutWeight / 10;
            set
            {
                OnPropertyChanged("CurrentHeight");
            }
        }
        public SolidColorBrush ConnectionColor                  //calls from modbusComm object
        {
            get => Connected ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
                OnPropertyChanged("BlinkColor");
            }
        }
        public bool Blink
        {
            get => _blinkStatic;
            set
            {
                _blink = value ? !_blink : _blink;
                _blinkStatic = value;
                OnPropertyChanged("Blink");
                OnPropertyChanged("BlinkColor");
            }
        }
        public SolidColorBrush BlinkColor
        {
            get
            {
                if (!Connected || !_isActive)
                    return new SolidColorBrush(Colors.Gray);
                else if (Blink)
                {
                    if(_blink)
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d3d3d"));
                    
                    else
                        return new SolidColorBrush(Colors.Red);
                } 
                else
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d3d3d"));
            }
        }



        #endregion


        #region PLCCompleted
        public bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnCompletedChanged(new CompletedChangedEventArgs(this, _completed));
            }
        }
        public event EventHandler<CompletedChangedEventArgs> CompletedChanged;

        public virtual void OnCompletedChanged(CompletedChangedEventArgs e)
        {
            if (CompletedChanged != null)
                CompletedChanged(this, e);
        }

        #endregion
    }
    public class CompletedChangedEventArgs : EventArgs
    {
        public readonly Silo silo;
        public readonly bool NewState;
        public CompletedChangedEventArgs(Silo silo, bool NewState)
        {
            this.silo = silo;
            this.NewState = NewState;
        }
    }


}
