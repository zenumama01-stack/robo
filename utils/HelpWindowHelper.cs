using Microsoft.PowerShell.Commands.ShowCommandInternal;
namespace Microsoft.PowerShell.Commands.Internal
    /// Implements the WPF window part of the ShowWindow option of get-help.
    internal static class HelpWindowHelper
        /// Shows the help window.
        /// <param name="helpObj">Object with help information.</param>
        /// <param name="cmdlet">Cmdlet calling this method.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from methods called using reflection")]
        private static void ShowHelpWindow(PSObject helpObj, PSCmdlet cmdlet)
            Window ownerWindow = ShowCommandHelper.GetHostWindow(cmdlet);
            if (ownerWindow != null)
                ownerWindow.Dispatcher.Invoke(
                    new SendOrPostCallback(
                        (_) =>
                            HelpWindow helpWindow = new HelpWindow(helpObj);
                            helpWindow.Owner = ownerWindow;
                            helpWindow.Show();
                            helpWindow.Closed += new EventHandler((sender, e) => ownerWindow.Focus());
                        string.Empty);
            Thread guiThread = new Thread(
            (ThreadStart)delegate
                helpWindow.ShowDialog();
            guiThread.SetApartmentState(ApartmentState.STA);
            guiThread.Start();
