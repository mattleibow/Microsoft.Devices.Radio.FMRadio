using System;
using System.Windows.Input;

namespace FMRadioApp
{
	public class Command : ICommand
	{
		private Action<object> execute;
		private Func<object, bool> canExecute;

		public Command(Action<object> execute, Func<object, bool> canExecute = null)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter) => canExecute?.Invoke(parameter) ?? true;

		public void Execute(object parameter) => execute(parameter);
	}
}
