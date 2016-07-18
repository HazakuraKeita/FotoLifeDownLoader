using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FotoLifeDownLoader
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> method;

        public Command(Action<object> method)
        {
            this.method = method;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            method(parameter);
        }
    }
}
