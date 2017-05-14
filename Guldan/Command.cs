using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Guldan
{
    #region AsyncCommand
    public class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand(Func<object, Task> asyncExecute, Predicate<bool> canExecute = null)
            : base(asyncExecute, canExecute)
        {
        }
    }
    public class AsyncCommand<T> : ICommand
    {
        protected readonly Predicate<bool> _canExecute;
        protected readonly Func<T, Task> _asyncExecute;
        protected bool _configureAwait;
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public AsyncCommand(Func<T, Task> asyncExecute, Predicate<bool> canExecute = null, bool configureAwait = false)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
            _configureAwait = configureAwait;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((bool)parameter);
        }
        public async void Execute(object parameter)
        {
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await _asyncExecute((T)parameter).ConfigureAwait(_configureAwait);
        }
    }
    #endregion

    #region SimpleCommand
    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action ExecuteDelegate { get; set; }
        public SimpleCommand(Action execute, Predicate<object> canExecute = null)
        {
            CanExecuteDelegate = canExecute;
            ExecuteDelegate = execute;
        }
        public bool CanExecute(object parameter)
        {
            return CanExecuteDelegate == null || CanExecuteDelegate(parameter);
        }
        public void Execute(object parameter)
        {
            ExecuteDelegate?.Invoke();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
    public class SimpleCommand<T> : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<T> ExecuteDelegate { get; set; }

        public SimpleCommand(Action<T> execute, Predicate<object> canExecute = null)
        {
            CanExecuteDelegate = canExecute;
            ExecuteDelegate = execute;
        }
        public bool CanExecute(object parameter)
        {
            return CanExecuteDelegate == null || CanExecuteDelegate(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter)
        {
            ExecuteDelegate?.Invoke((T)parameter);
        }
    }
    #endregion
}
