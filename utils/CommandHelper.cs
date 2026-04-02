using System.Security;
    internal static class CommandHelper
        internal static void ExecuteCommand(ICommand command, object parameter, IInputElement target)
            RoutedCommand command2 = command as RoutedCommand;
            if (command2 != null)
                if (command2.CanExecute(parameter, target))
                    command2.Execute(parameter, target);
            else if (command.CanExecute(parameter))
                command.Execute(parameter);
        internal static bool CanExecuteCommand(ICommand command, object parameter, IInputElement target)
            if (command == null)
                return command2.CanExecute(parameter, target);
                return command.CanExecute(parameter);
