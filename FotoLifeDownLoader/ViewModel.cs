using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;

namespace FotoLifeDownLoader
{
    public class ViewModel : INotifyPropertyChanged
    {
        public string HatenaID
        {
            get
            {
                return hatenaID;
            }
            set
            {
                hatenaID = value;
                OnPropertyChanged("HatenaID");
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public string SaveDirectory
        {
            get
            {
                return saveDirectory;
            }
            set
            {
                saveDirectory = value;
                OnPropertyChanged("SaveDirectory");
            }
        }
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }
        public int Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
                OnPropertyChanged("Max");
            }
        }
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }
        public bool IsIndeterminate
        {
            get
            {
                return isIndeterminate;
            }
            set
            {
                isIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }
        public ICommand CloseCommand { get; private set; }
        public ICommand DirectryBrowseCommand { get; private set; }
        public ICommand DownLoadCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string hatenaID;
        private string saveDirectory;
        private string message;
        private string password;
        private int max;
        private int progress;
        private bool isIndeterminate;
        private bool isDownLoadCanceled;

        public ViewModel()
        {
            Message = "全画像のURLを調べています";
            IsIndeterminate = true;
            CloseCommand = new Command(Close);
            DirectryBrowseCommand = new Command(BrowseDirectry);
            DownLoadCommand = new Command(DownLoad);
        }

        public async void DownLoad(object parameter)
        {
            if (string.IsNullOrEmpty(HatenaID) || string.IsNullOrEmpty(SaveDirectory))
            {
                MessageBox.Show("はてなIDと保存場所の設定は必ず行ってください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(saveDirectory))
            {
                MessageBox.Show("不正な保存場所です。そのようなフォルダは存在しません。もう一度設定しなおしてください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var progress = new Progress();
            progress.DataContext = this;
            isDownLoadCanceled = false;

            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    progress.ShowDialog();
                    isDownLoadCanceled = true;
                    return;
                });
            });

            try
            {
                var user = new HatenaUser(HatenaID, Password);
                await user.Connect();
                await user.LoadAllImageUri();

                IsIndeterminate = false;
                Message = user.Images.Count + "枚の画像をダウンロードしています";
                Max = user.Images.Count;

                for (var i = 0; i < Max; i++)
                {
                    if (!isDownLoadCanceled)
                    {
                        Progress = i;
                        await user.DownLoad(user.Images[i], SaveDirectory);
                    }
                }
            }
            catch (WebException)
            {
                MessageBox.Show("通信エラーが発生しました。ダウンロードを中止します", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (XmlException)
            {
                MessageBox.Show("画像のURLの取得ができません。はてなIDとパスワードを確認してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }        

            progress.Close();
        }

        public void BrowseDirectry(object parameter)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsureReadOnly = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.Multiselect = false;

            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                SaveDirectory = dialog.FileName; 
            }
        }

        public void Close(object parameter)
        {
            Application.Current.MainWindow.Close();
        }

        public void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public async Task Sleep(int time)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(time);
            });
        }
    }
}
