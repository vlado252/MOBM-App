using System;
using System.Windows.Input;

namespace MOBM_App.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _exec;
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public RelayCommand(Action<object> exec) { _exec = exec; }
        public bool CanExecute(object p) => true;
        public void Execute(object p) => _exec(p);
    }
}