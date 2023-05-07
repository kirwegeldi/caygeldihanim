using CAY_Weighing.IpConfiguration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Security.Cryptography;

namespace CAY_Weighing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private System.Timers.Timer siloTimer = new System.Timers.Timer();
        private System.Timers.Timer UIUpdateTimer = new System.Timers.Timer();
        private System.Timers.Timer connectionTimer = new System.Timers.Timer();

        private readonly BackgroundWorker Hat1Startworker = new BackgroundWorker();
        private readonly BackgroundWorker Hat2Startworker = new BackgroundWorker();
        private readonly BackgroundWorker Hat1Stopworker = new BackgroundWorker();
        private readonly BackgroundWorker Hat2Stopworker = new BackgroundWorker();

        private Dictionary<int, double> FillingInfo = new Dictionary<int, double>();
        private List<Canvas> AllCanvas = new List<Canvas>();

        private bool screenLogBusy = false;
        private int screenLogQueue = 0;

        public MainWindow()
        {
            Common.GetloggerDir();
            InitializeComponent();
            ScreenLogger.DataContext = this;
            Hat1StartButton.DataContext = this;
            Hat1StopButton.DataContext = this;
            Hat2StartButton.DataContext = this;
            Hat2StopButton.DataContext = this;

            Initialize();
        }
        ~MainWindow()
        {
            foreach (var silo in Silo.ConnectedSilos)
            {
                if (silo.Connected)
                    silo.Disconnect();
            }
        }
        private void Gıybet_Loaded(object sender, RoutedEventArgs e)
        {
            siloTimer.Elapsed += new ElapsedEventHandler(SiloTimeEvent);
            siloTimer.Interval = 200;
            siloTimer.Enabled = true;

            UIUpdateTimer.Elapsed += new ElapsedEventHandler(UIUpdateEvent);
            UIUpdateTimer.Interval = 1000;
            UIUpdateTimer.Enabled = true;

            connectionTimer.Elapsed += new ElapsedEventHandler(ConnectionTimeEvent);
            connectionTimer.Interval = 5000;
            connectionTimer.Enabled = true;
        }
        private void Initialize()
        {
            var ipconfig = ConfigurationManager.GetSection("SiloIp") as IpConfig;
            foreach (IpElement instance in ipconfig.Silos)
            {
                Silo silo = new Silo(instance.id, instance.IpAddress, instance.port);
                Silo.AllSilos.Add(silo);

                if (silo._ıd < 5)
                    Silo.Hat1.Add(silo);
                if (silo._ıd > 4 && silo._ıd <9)
                    Silo.Hat2.Add(silo);

                silo.Connect();
                Common.Logger.LogInfo("Connecting " + silo._ıd);
                silo.modbusComm.ConnectedChanged += ModbusComm_ConnectedChanged;
                silo.CompletedChanged += Silo_CompletedChanged;
            }
            AllCanvas.Add(CanvasSilo1);
            AllCanvas.Add(CanvasSilo2);
            AllCanvas.Add(CanvasSilo3);
            AllCanvas.Add(CanvasSilo4);
            AllCanvas.Add(CanvasSilo5);
            AllCanvas.Add(CanvasSilo6);
            AllCanvas.Add(CanvasSilo7);
            AllCanvas.Add(CanvasSilo8);
            AllCanvas.Add(CanvasMixer1);
            AllCanvas.Add(CanvasMixer2);

            for (int i = 0; i < Silo.AllSilos.Count; i++)
            {
                var canvas = AllCanvas[i];
                canvas.DataContext = Silo.AllSilos.FirstOrDefault(s => s._ıd == int.Parse(canvas.Uid));
                Console.WriteLine(canvas.Name + " ----- " + canvas.DataContext.ToString());
            }
            PLC.Connect();
            User.LoadUserInfo();
            LoadAllData();


            Hat1Startworker.DoWork += Hat1Start;    //PLC de WTM completed coillerini takip eden background workerlar.
            Hat1Startworker.WorkerSupportsCancellation = true;
            Hat2Startworker.DoWork += Hat2Start;
            Hat2Startworker.WorkerSupportsCancellation = true;

            Hat1Stopworker.DoWork += Hat1Stop;
            Hat1Stopworker.WorkerSupportsCancellation = true;
            Hat2Stopworker.DoWork += Hat2Stop;
            Hat2Stopworker.WorkerSupportsCancellation = true;


            foreach (var silo in Silo.AllSilos)
            {
                PLC.WriteCoil(8268 + silo._ıd, true);
            }
        }
        private void PLCTimeEvent(object sender, ElapsedEventArgs e)
        {
            if (!PLC._connected)
            {
                PLC.Connect();
                return;
            }
            //List<Silo> currentConnectedList = new List<Silo>(Silo.ConnectedSilos);
            //FillingInfo.Clear();
            //for (int i = 0; i < currentConnectedList.Count; i++)
            //{
            //    Silo silo = currentConnectedList[i];
            //    int id = silo._ıd;
            //    if (id == 9 || id == 10)
            //        continue;
            //    double total = PLC.GetMessage(silo);
            //    if (total != -1)
            //    {
            //        FillingInfo.Add(silo._ıd, total);
            //    }
            //}
            //if (FillingInfo.Count != 0)                         //Herhangi bir silonun PLC si acılıp kapandıysa varsa database'e gönderir
            //    Report.SaveFillingData(FillingInfo);
        }
        private void SiloTimeEvent(object sender, ElapsedEventArgs e)
        {
            GetMessage();
        }
        private void UIUpdateEvent(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < Silo.ConnectedSilos.Count; i++)
            {
                var silo = Silo.ConnectedSilos[i];
                silo.UpdateUI();
            }
        }
        private void ConnectionTimeEvent(object sender, ElapsedEventArgs e)
        {
            ConnectAsync();
            if (!PLC._connected)
            {
                PLC.Connect();
                return;
            }
        }

        private void GetMessage()
        {
            Parallel.For(0, Silo.ConnectedSilos.Count, i =>
            {
                try
                {
                    var silo = Silo.ConnectedSilos[i];
                    lock (silo)
                    {
                        silo.GetMessage();
                    }
                }
                catch
                {
                    Console.WriteLine("Error in GetMessage main");
                }
            });
        }
        private void ConnectAsync()
        {
            Parallel.For(0, Silo.DisconnectedSilos.Count, i =>
            {
                try
                {
                    var silo = Silo.DisconnectedSilos[i];
                    lock (silo)
                    {
                        silo.Connect();
                    }
                }
                catch { }
            });
        }

        #region Database

        /// <summary>
        /// Daily data pull.
        /// </summary>
        private void LoadDataDaily()
        {

            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();

            string query = $"SELECT* FROM Result WHERE DATE = '{DateTime.Now.Date.ToString().Substring(0, 10)}'";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            //dtgResultListDaily.DataSource = table;
        }
        /// <summary>
        /// Load All Date
        /// </summary>
        private void LoadAllData()
        {
            System.Data.DataTable table = DbManager.LoadDataTable(DbSchema.Result.NAME);

            //dtgResultListAll.DataSource = table;          
        }
        /// <summary>
        /// Delete Last Row
        /// </summary>
        private void DeleteLastRow()
        {
            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();
            string query = $"DELETE FROM Result  WHERE  ID = (SELECT Max(ID) FROM Result)";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            string query2 = $"declare @max int select @max=max([Id]) from [Result] if @max IS NULL SET @max = 0 DBCC CHECKIDENT ('[Result]', RESEED, @max)";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query2, out table);
            LoadAllData();
        }
        /// <summary>
        /// Delete All Data
        /// </summary>
        public void DeleteAllData()
        {
            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();
            string query = $"TRUNCATE TABLE " + "Result";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            LoadAllData();
        }
        #endregion

        #region ScreenButtons
        private void Enable_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            int id = int.Parse((sender as CheckBox).Uid) + 1;
            var silo = Silo.AllSilos.Find(s => s._ıd == id);

            if (silo == null)
                return;
            if ((sender as CheckBox).IsChecked == false)
            {
                MessageBoxResult message = MessageBox.Show("Silo devredışı kalacak.", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (message == MessageBoxResult.OK)
                    result = false;
                else
                    result = true;
            }
            else
            {
                MessageBoxResult message = MessageBox.Show("Silo devreye alınacak.", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (message == MessageBoxResult.OK)
                    result = true;
                else
                    result = false;
            }
            (sender as CheckBox).IsChecked = result;

            try
            {
                silo.IsActive = result;
                switch (id)
                {
                    case 1:
                        PLC.WriteCoil(8258, result);
                        break;
                    case 2:
                        PLC.WriteCoil(8259, result);
                        break;
                    case 3:
                        PLC.WriteCoil(8260, result);
                        break;
                    case 4:
                        PLC.WriteCoil(8261, result);
                        break;
                    case 5:
                        PLC.WriteCoil(8264, result);
                        break;
                    case 6:
                        PLC.WriteCoil(8265, result);
                        break;
                    case 7:
                        PLC.WriteCoil(8266, result);
                        break;
                    case 8:
                        PLC.WriteCoil(8267, result);
                        break;
                }
            }
            catch (Exception ex) { Common.Logger.LogError(ex.ToString()); }
            if (!silo.Connected)
                silo.Connect();
        }
        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (Password()) // şifre girilmesi istenirse true kısmını Password() olarak değiştir.
            {
                bool done = false;
                try
                {
                    int id = int.Parse((sender as Button).Uid) + 1;
                    done = (bool)Silo.AllSilos.Find(s => s._ıd == id)?.WriteZero();
                }
                catch { }
                RefreshScreenLog(done);
                ButtonEffect(button, done);
            }
        }
        private void Tare_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (Password()) // şifre girilmesi istenirse true kısmını Password() olarak değiştir.
            {
                bool done = false;
                try
                {
                    int id = int.Parse((sender as Button).Uid) + 1;
                    done = (bool)Silo.AllSilos.Find(s => s._ıd == id)?.WriteTare();
                }
                catch { }
                RefreshScreenLog(done);
                ButtonEffect(button, done);
            }
        }
        private void Set1_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (true) // şifre girilmesi istenirse true kısmını Password() olarak değiştir.
            {
                bool done = false;
                try
                {
                    int id = int.Parse((sender as Button).Uid) + 1;
                    var silo = Silo.AllSilos.Find(s => s._ıd == id);
                    if (silo != null)
                    {
                        int tempValue = silo._valueLow;
                        NumericKeypad keypad = new NumericKeypad(this);
                        keypad.Title = "Set 1";
                        keypad.Result = "Değer giriniz";
                        if (keypad.ShowDialog() == true && !String.IsNullOrEmpty(keypad.Result))
                        {
                            silo._valueLow = Convert.ToInt32(keypad.Result);
                            done = silo.WriteValueLow();
                            
                            if (done)
                            {
                                (sender as Button).Content = "Set: " + silo._valueLow;
                            }
                            else
                                silo._valueLow = tempValue;
                        }
                    }
                }
                catch { }
                RefreshScreenLog(done);
                ButtonEffect(button, done);
            }
        }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult message = MessageBox.Show("Program kapatılsın mı?", "Kapat", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (message == MessageBoxResult.OK)
            {
                #region Hat1Stop
                Hat1Startworker.CancelAsync();
                PLC.WriteCoil(8256, false);
                PLC.WriteCoil(8257, true);
                foreach (var silo in Silo.Hat1)
                {
                    if (!silo.Completed)
                    {
                        silo.Completed = true;
                        PLC.WriteCoil(8268 + silo._ıd, true);
                    }
                }
                #endregion
                #region Hat2Stop
                Hat2Startworker.CancelAsync();
                PLC.WriteCoil(8262, false);
                Thread.Sleep(100);
                PLC.WriteCoil(8268, true);
                Thread.Sleep(100);
                foreach (var silo in Silo.Hat2)
                {
                    if (!silo.Completed)
                    {
                        silo.Completed = true;
                        PLC.WriteCoil(8268 + silo._ıd, true);
                    }
                }

                #endregion
                Thread.Sleep(1000);
                Application.Current.Shutdown();
            }

        }

        #region Start-Stop Butons Disabled
        private bool _plcbuttonsenabled = true;
        public bool PlcButtonsEnabled 
        {
            get => _plcbuttonsenabled;
            set
            {
                _plcbuttonsenabled = value;
                OnPropertyChanged("PlcButtonsEnabled");

            }
        }
        #endregion
        #endregion

        #region EventHandlers
        private void ModbusComm_ConnectedChanged(object sender, ConnectedChangedEventArgs e)
        {
            int id = e.silo._ıd;
            string log = "";
            bool green = false;
            if (e.NewState)
            {
                green = true;
                if (id == 9 || id == 10)
                {
                    id = (id == 9) ? 1 : 2; // :D //Promil 150 .d
                    log = "Mixer " + id + " connected";
                }
                else
                {
                    log = "Silo " + id + " connected";
                }
            }
            else
            {
                if (id == 9 || id == 10)
                {
                    id = (id == 9) ? 1 : 2; // :D //Promil 150 .d
                    log = "Mixer " + id + " disconnected";
                }
                else
                {
                    log = "Silo " + id + " disconnected";
                }

            }
            this.Dispatcher.Invoke(() =>
            {
                RefreshScreenLog(log, green);
            });

        }
        private void Silo_CompletedChanged(object sender, CompletedChangedEventArgs e)
        {
            double total;
            bool hatFinishedFlag = true; // start verdikten sonra hattaki tüm silolar set değerini tamamladıysa true değeri verir
            List<Silo> hat;
            if (e.NewState && !e.PrevState)
            {
                total = e.silo._firstWeight - e.silo._currentWeight;
                try
                {
                    FillingInfo.Add(e.silo._ıd, total);
                }
                catch
                {
                    FillingInfo[e.silo._ıd] = total;
                }


                if (e.silo._ıd < 5)
                    hat = Silo.Hat1;
                else
                    hat = Silo.Hat2;

                for (int i = 0; i < hat.Count; i++)
                {
                    var silo = hat[i];
                    if (!silo.Completed)
                        hatFinishedFlag = false;
                }
                if (hatFinishedFlag)
                {
                    try
                    {
                        Report.SaveFillingData(FillingInfo);
                        foreach (var silo in hat)       //raporu yazdıktan sonra fillinginfo listesinde ilgili hattın verilerini sıfırlar.(sıfırlamazsan bir sonraki yazmada ilgili silo aktif değilse bile eski değerini database' e yazar)
                        {
                            FillingInfo[silo._ıd] = 0;
                        }
                    }
                    catch { }

                    
                }
            }
            else
            {
                e.silo._firstWeight = e.silo.CurrentWeight;
            }
        }
        #endregion

        #region ReportButtons

        private void BtnDailyReport_Click(object sender, RoutedEventArgs e)
        {
            if (Password())
            {
                if (Report.ExportasExcel(Report.LoadDataDaily(), "Günlük"))
                    MessageBox.Show("1 günlük işlem geçmişi excele aktarıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("1 günlük işlem geçmişi excele aktarılmadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Btn1MountlyReport_Click(object sender, RoutedEventArgs e)
        {
            if (Password())
            {
                if (Report.ExportasExcel(Report.LoadMountly(), "1Aylık"))
                    MessageBox.Show("1 aylık işlem geçmişi excele aktarıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("1 aylık işlem geçmişi excele aktarılmadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Btn3MountlyReport_Click(object sender, RoutedEventArgs e)
        {
            if (Password())
            {
                if (Report.ExportasExcel(Report.Load3Mountly(), "3Aylık"))
                    MessageBox.Show("3 aylık işlem geçmişi excele aktarıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("3 aylık işlem geçmişi excele aktarılmadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Btn6MountlyReport_Click(object sender, RoutedEventArgs e)
        {
            if (Password())
            {
                if (Report.ExportasExcel(Report.Load6Mountly(), "6Aylık"))
                    MessageBox.Show("6 aylık işlem geçmişi excele aktarıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("6 aylık işlem geçmişi excele aktarılmadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private void BtnAllReport_Click(object sender, RoutedEventArgs e)
        {
            if (Password())
            {
                if (Report.ExportasExcel(Report.LoadAllData(), "All"))
                    MessageBox.Show("Bütün veriler excele aktarıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Bütün veriler excele aktarılmadı.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnClearAllReport_Click(object sender, RoutedEventArgs e)
        {
            string log;
            if (Password())
            {
                bool result = false;
                MessageBoxResult message = MessageBox.Show("Dikkat! Tüm işlem geçmişi silinecek.", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (message == MessageBoxResult.OK)
                    result = true;

                if (Report.DeleteAllData(result))
                {
                    log = "Geçmiş silme başarılı.";
                    Common.Logger.LogInfo(log);
                }
                else
                {
                    result = false;
                    log = "Geçmiş silme başarısız.";
                }
                RefreshScreenLog(log, result);
            }
        }
        #endregion

        #region Passsword
        private bool Password()
        {
            NumericKeypad keypad = new NumericKeypad(this);
            keypad.Title = "Password";
            keypad.Result = "Şifre giriniz";
            if (keypad.ShowDialog() == true &&
                !String.IsNullOrEmpty(keypad.Result) &&
                (keypad.Result == User.MANUFACTURER_PASSWORD || keypad.Result == User.UserPassword))
            {
                return true;
            }
            if (keypad.DialogResult == false)
                return false;
            MessageBox.Show("Parola yanlış", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            return Password();

        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Password password = new Password(this);
            string tempPassword = User.UserPassword;
            password.ShowDialog();
            bool result = (tempPassword == User.UserPassword) ? false : true;
            string log;
            if (result)
            {
                log = "Şifre başarıyla değiştirildi";
                Common.Logger.LogInfo(log);
            }
            else
                log = "Şifre değiştirme başarısız";
            RefreshScreenLog(log, result);
        }

        #endregion

        #region ScreenLogger

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        private string _screenLog;
        public string ScreenLog
        {
            get
            {
                return _screenLog;
            }
            set
            {
                _screenLog = value;
                PlcButtonsEnabled = (_screenLog == "İşlem başlatılıyor...") ? false : true;
                OnPropertyChanged("ScreenLog");
            }
        }
        private async void RefreshScreenLog(bool done)
        {
            if (screenLogQueue > 4)
                return;
            while (screenLogBusy)
            {
                await Task.Delay(25);
            }

            screenLogBusy = true;
            screenLogQueue++;

            SolidColorBrush brushcolor = new SolidColorBrush();
            Color color = done ? Colors.Green : Colors.Red;
            if (done)
                ScreenLog = "İşlem başarılı...";
            else
                ScreenLog = "İşlem başarısız...";

            brushcolor.Color = color;
            ScreenLogger.Foreground = brushcolor;
            await Task.Delay(500);

            brushcolor.Color = Colors.Black;
            ScreenLogger.Foreground = brushcolor;
            await Task.Delay(500);

            brushcolor.Color = color;
            ScreenLogger.Foreground = brushcolor;
            await Task.Delay(500);

            ScreenLog = string.Empty;

            brushcolor.Color = Colors.Green;
            ScreenLogger.Foreground = brushcolor;

            screenLogBusy = false;
            screenLogQueue--;
        }
        private async void RefreshScreenLog(string log, bool isGreen)
        {
            if (screenLogQueue > 4)
                return;
            while (screenLogBusy)
            {
                await Task.Delay(25);
            }

            screenLogBusy = true;
            screenLogQueue++;


            SolidColorBrush logColor = new SolidColorBrush();
            ScreenLog = log;

            logColor.Color = isGreen ? Colors.Green : Colors.Red;
            ScreenLogger.Foreground = logColor;
            await Task.Delay(500);

            logColor.Color = Colors.Black;
            ScreenLogger.Foreground = logColor;
            await Task.Delay(500);

            logColor.Color = isGreen ? Colors.Green : Colors.Red;
            ScreenLogger.Foreground = logColor;
            await Task.Delay(500);

            ScreenLog = string.Empty;

            logColor.Color = Colors.Green;
            ScreenLogger.Foreground = logColor;

            screenLogBusy = false;
            screenLogQueue--;
        }
        private async void ButtonEffect(Button button, bool done)
        {
            Color color = done ? Colors.Green : Colors.Red;

            GradientStop gradient = (button.BorderBrush as LinearGradientBrush).GradientStops[0];
            gradient.Color = color;

            //effect
            gradient.Offset = 2;
            while (gradient.Offset > 0.5)
            {
                gradient.Offset -= 0.2;
                await Task.Delay(10);
            }
            while (gradient.Offset > 0.1)
            {
                gradient.Offset -= 0.1;
                await Task.Delay(10);
            }
            gradient.Offset = 0.01;
            await Task.Delay(500);
            gradient.Offset = 0;
            await Task.Delay(500);
            gradient.Offset = 0.01;
            await Task.Delay(500);
            gradient.Offset = 0;
            //flash

            //gradient.Offset = 0.01;
            //await Task.Delay(1000);
            //gradient.Offset = 0;
            //await Task.Delay(1000);
            //gradient.Offset = 0.01;
            //await Task.Delay(1000);
            //gradient.Offset = 0;

        }
        private async void BorderEffect(GradientStop gradientStop, bool done)
        {
            gradientStop.Color = done ? Colors.Green : Colors.Red;
            gradientStop.Offset = 2;
            while (gradientStop.Offset > 0.1)
            {
                gradientStop.Offset -= 0.1;
                await Task.Delay(100);
            }
            gradientStop.Color = (Color)ColorConverter.ConvertFromString("#462ad8");
        }

        #endregion

        #region PLC
        private void Hat1Start(object sender, DoWorkEventArgs e)
        {
            ScreenLog = "İşlem başlatılıyor...";
            foreach (var silo in Silo.Hat1)
            {
                if (silo.Connected && silo.IsActive && silo.WriteTare())
                    silo.listenPLC();
            }
            PLC.WriteCoil(8257, false);//Stop
            PLC.WriteCoil(8256, true);//Start
            PLC.WriteCoil(8256, false);//Stop

            ScreenLog = String.Empty;
        }
        private void Hat2Start(object sender, DoWorkEventArgs e)
        {
            ScreenLog = "İşlem başlatılıyor...";
            Parallel.ForEach(Silo.Hat2, silo =>
            {
                if (silo.Connected && silo.IsActive && silo.WriteTare())
                    silo.listenPLC();
            });
            
            PLC.WriteCoil(8268, false);//Stop
            PLC.WriteCoil(8262, true);//Start
            PLC.WriteCoil(8262, false); //Stop

            ScreenLog = String.Empty;

        }
        private void Hat1Stop(object sender, DoWorkEventArgs e)
        {
            Hat1Startworker.CancelAsync();
            PLC.WriteCoil(8256, false);
            PLC.WriteCoil(8257, true);
            foreach (var silo in Silo.Hat1)
            {
                if (!silo.Completed)
                {
                    if(PLC.WriteCoil(8268 + silo._ıd, true))
                        silo.Completed = true;
                }
            }
        }
        private void Hat2Stop(object sender, DoWorkEventArgs e)
        {
            Hat2Startworker.CancelAsync();
            PLC.WriteCoil(8262, false);
            PLC.WriteCoil(8268, true);
            foreach (var silo in Silo.Hat2)
            {
                if (!silo.Completed)
                {
                    if(PLC.WriteCoil(8268 + silo._ıd, true))
                        silo.Completed = true;
                }
            }
        }


        private void Hat1_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hat1Startworker.RunWorkerAsync();
            }
            catch 
            {
                var log = "İşlem devam ediyor.";
                RefreshScreenLog(log, false);
            }
        }
        private void Hat2_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hat2Startworker.RunWorkerAsync();
            }
            catch
            {
                var log = "İşlem devam ediyor.";
                RefreshScreenLog(log, false);
            }
        }
        private void Hat1_Stop_Click(object sender, RoutedEventArgs e)
        {
            Hat1Stopworker.RunWorkerAsync();
        }
        private void Hat2_Stop_Click(object sender, RoutedEventArgs e)
        {
            Hat2Stopworker.RunWorkerAsync();
        }


        #endregion

    }
}
