using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class ReportViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<object> BlowoutBindigCollection { get; set; }
        public ObservableCollection<object> SpecialControlBindingCollection { get; set; }
        public ObservableCollection<object> IodineBindingCollection { get; set; }
        public ObservableCollection<object> CategoryBlowoutBindingCollection { get; set; }

        public ReportViewModel()
        {
            BlowoutBindigCollection = ReportsCollectionSingleton.GetInstance().BlowoutCollection;
            SpecialControlBindingCollection = ReportsCollectionSingleton.GetInstance().SpecialControlCollection;
            IodineBindingCollection = ReportsCollectionSingleton.GetInstance().IodineCollection;
            CategoryBlowoutBindingCollection = ReportsCollectionSingleton.GetInstance().CategoryCollection;
        }

        public ICommand CloseWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        Window reportWindow = obj as Window;
                        reportWindow.Close();
                    }
                });
            }
        }
    }
}
