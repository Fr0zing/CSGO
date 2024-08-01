using System;
using System.Windows.Input;

namespace CSGOCheatDetector.Commands
{
    // Реализация интерфейса ICommand для создания команд
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Конструктор для создания команды без проверки возможности выполнения
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        // Конструктор для создания команды с проверкой возможности выполнения
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Метод для проверки, может ли команда выполняться
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Метод для выполнения команды
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Событие, уведомляющее об изменении состояния выполнения команды
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        // Метод для явного вызова обновления состояния команды
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
