using System;
using System.Windows.Input;

namespace Lilabu.ViewModels
{
    public class BaseCommand : ICommand
    {
        /// <summary>
        /// Действие
        /// </summary>
        public Action Action { get; }

        /// <summary>
        /// Базовая команда
        /// </summary>
        /// <param name="action">Действие</param>
        public BaseCommand(Action action)
        {
            Action = action;
        }

        #region Implementation of ICommand

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            Action.Invoke();
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}
