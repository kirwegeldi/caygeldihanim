using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CAY_Weighing
{
    /// <summary>
    /// Interaction logic for Password.xaml
    /// </summary>
    public partial class Password : Window, INotifyPropertyChanged
    {
        public Password(Window owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this;

            this.Top = 250;
            this.Left = 500;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private string _oldPassword;
        public string oldPassword
        {
            get { return _oldPassword; }
            set { _oldPassword = value; this.OnPropertyChanged("oldPassword"); }
        }
        private string _newPassword;
        public string newPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; this.OnPropertyChanged("newPassword"); }
        }
        private string _newPasswordAgain;
        public string newPasswordAgain
        {
            get { return _newPasswordAgain; }
            set { _newPasswordAgain = value; this.OnPropertyChanged("newPasswordAgain"); }
        }

        private void TxtOldPassword_MouseUp(object sender, MouseButtonEventArgs e)
        {
            oldPassword = string.Empty;
            NumericKeypad keypad = new NumericKeypad(this);
            if (keypad.ShowDialog() == true &&
            !String.IsNullOrEmpty(keypad.Result))
            {
                if (keypad.Result.Length < 4)
                {
                    keypad.Result = string.Empty;
                    MessageBox.Show("Şifre 4 haneli olmalı", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtOldPassword_MouseUp(sender, e);
                }
                oldPassword = keypad.Result;
            }
        }
        private void TxtNewPassword_MouseUp(object sender, MouseButtonEventArgs e)
        {
            newPassword = string.Empty;
            NumericKeypad keypad = new NumericKeypad(this);
            if (keypad.ShowDialog() == true &&
            !String.IsNullOrEmpty(keypad.Result))
            {
                if (keypad.Result.Length < 4)
                {
                    keypad.Result = string.Empty;
                    MessageBox.Show("Şifre 4 haneli olmalı", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtNewPassword_MouseUp(sender, e);
                }
                newPassword = keypad.Result;
            }
        }

        private void TxtNewPasswordAgain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            newPasswordAgain = string.Empty;
            NumericKeypad keypad = new NumericKeypad(this);
            if (keypad.ShowDialog() == true &&
            !String.IsNullOrEmpty(keypad.Result))
            {
                if (keypad.Result.Length < 4)
                {
                    keypad.Result = string.Empty;
                    MessageBox.Show("Şifre 4 haneli olmalı", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtNewPasswordAgain_MouseUp(sender, e);
                }
                newPasswordAgain = keypad.Result;
            }
        }

        private void BtnPasswordConfirm_Click(object sender, RoutedEventArgs e)
        {

            bool result = User.ChangePassword(oldPassword, newPassword, newPasswordAgain);
            if (!result)
                MessageBox.Show("Bilgiler hatalı", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                this.Close();
        }
    }

}
