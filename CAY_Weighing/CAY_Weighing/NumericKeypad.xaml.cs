
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System;

namespace CAY_Weighing
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class NumericKeypad : Window, INotifyPropertyChanged
    {
        #region Public Properties
        public int maxLength { get; set; }
        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; this.OnPropertyChanged("Result"); }
        }

        #endregion

        public NumericKeypad(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            this.DataContext = this;
            if (owner.Name == "BinlerceDansozVar")
            {
                btnDot.IsEnabled = false;
                maxLength = 4;
            }
            else
                maxLength = 7;

            this.Top = 250;
            this.Left = 500;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Result == "Şifre giriniz" || Result == "Değer giriniz")
                Result = "";
            Button button = sender as Button;
            switch (button.CommandParameter.ToString())
            {
                case "ESC":
                    this.DialogResult = false;
                    break;

                case "RETURN":
                    this.DialogResult = true;
                    break;

                case "BACK":
                    if (Result.Length > 0
                        && !String.IsNullOrEmpty(Result))
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    if (String.IsNullOrEmpty(Result)
                        || Result.Length < maxLength)
                    {
                        if (button.Content.ToString() == ".")
                        {
                            if (String.IsNullOrEmpty(Result))
                                break;
                            else if (Result.Contains("."))
                                return;
                        }
                        Result += button.Content.ToString();
                    }
                    break;
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }



        #endregion
    }
}
