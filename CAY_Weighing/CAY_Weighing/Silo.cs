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

        private static double upperLimit = 1400;    //for blink
        private static double lowerLimit = 300;


        public int _ıd { get; set; }
        public string _Ip { get; set; }
        public int _port { get; set; }
        public Communication modbusComm { get; set; }

        private double _currentWeight;
        private double _brutWeight;
        public int _valueLow;
        public int _valueHigh;
        private bool _isActive = true;
        private bool _blinkStatic;                      // if current weight is below the lower limit it remanins true
        private bool _blink;                            // if current weight is below the lower limit it changes !_blink
        
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
                return CurrentWeight;
            int[] value = modbusComm.GetMessage();
            if (value != null)
            {
                CurrentWeight = value[1] / 10.0;
                BrutWeight = value[0] / 10.0;
                return CurrentWeight;
            }
            AddRemove(DisconnectedSilos, ConnectedSilos);
            return CurrentWeight;
        }
        public bool WriteTare()
        {
            if (!_isActive)
                return false;
            return modbusComm.WriteMessage(88, 2);
        }
        public bool WriteZero()
        {
            if (!_isActive)
                return false;
            return modbusComm.WriteMessage(88, 1);
        }
        public bool WriteValueLow()
        {
            if (!_isActive)
                return false;
            return modbusComm.WriteMessage(19, _valueLow);
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
                CurrentHeight = (int)value;
                if (_ıd == 9 || _ıd == 10)
                    Blink = _currentWeight > upperLimit ? true : false;
                else
                {
                    Blink = _currentWeight < lowerLimit ? true : false;
                }
                OnPropertyChanged("CurrentWeight");
            }
        }
        public double BrutWeight { 
            get => _brutWeight;
            set
            {
                _brutWeight = value;
                OnPropertyChanged("BrutWeight");
            }
         }
        public int CurrentHeight
        {
            get => (this._ıd ==9 || this._ıd == 10) ? (int)_currentWeight / 13: (int)_currentWeight / 10;
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
    }


}
