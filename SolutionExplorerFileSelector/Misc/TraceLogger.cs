//// --------------------------------------------------------------------------------------------------------------------
//// <copyright>Marc Schürmann</copyright>
//// --------------------------------------------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace SolutionExplorerFileSelector.Misc
{
    /// <summary>The Trace logger.</summary>
    public class TraceLogger
    {
        #region Private Fields

        private DTE dte;
        private OutputWindowPane outputPane;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>The trace logger.</summary>
        /// <param name="dte"></param>
        public TraceLogger(DTE dte)
        {
            this.dte = dte;
            outputPane = GetOutputPane();

            outputPane.Activate();
            Log("Successfuly created solution explorer file selector logger");
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>Logs the specified message.</summary>
        /// <param name="message">The message.</param>
        public void Log(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            outputPane?.Activate();
            outputPane?.OutputString(Environment.NewLine);
            outputPane?.OutputString(message);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>Gets the output pane.</summary>
        /// <returns>The output pane.</returns>
        private OutputWindowPane GetOutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (outputPane == null)
            {
                var window = dte.Windows.Item(Constants.vsWindowKindOutput);
                var outputWindow = (OutputWindow)window.Object;
                outputPane = outputWindow.OutputWindowPanes.Add("Solution explorer file selector");
            }

            return outputPane;
        }

        #endregion Private Methods
    }
}