//// --------------------------------------------------------------------------------------------------------------------
//// <copyright>Marc Schürmann</copyright>
//// --------------------------------------------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SolutionExplorerFileSelector.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SolutionExplorerFileSelector
{
    /// <summary>Command handler</summary>
    internal sealed class SolutionExplorerFileSelectorCommand
    {
        #region Public Fields

        /// <summary>Command ID.</summary>
        public const int CommandId = 0x0100;

        /// <summary>Command menu group (command set GUID).</summary>
        public static readonly Guid CommandSet = new Guid("02018036-5a77-4bc9-a7f1-1373f81bdbcc");

        #endregion Public Fields

        #region Private Fields

        /// <summary>VS Package that provides this command, not null.</summary>
        private readonly AsyncPackage package;

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionExplorerFileSelectorCommand"/>
        /// class. Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SolutionExplorerFileSelectorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>Gets the instance of the command.</summary>
        public static SolutionExplorerFileSelectorCommand Instance
        {
            get;
            private set;
        }

        #endregion Public Properties

        #region Private Properties

        private static DTE dte { get; set; }
        private static DTE2 dte2 { get; set; }

        /// <summary>Gets the service provider from the owner package.</summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>Initializes the singleton instance of the command.</summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in
            // SolutionExplorerFileSelectorCommand's constructor requires the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new SolutionExplorerFileSelectorCommand(package, commandService);

            dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            dte2 = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var logger = new TraceLogger(dte);

            var itemName = GetItemName();
            logger.Log($"The active document is '{itemName}'.");

            var itemToSelect = dte2.ToolWindows.SolutionExplorer.GetItem(itemName);
            logger.Log($"The item in the solution explorer is '{itemToSelect.Name}'.");

            itemToSelect?.Select(vsUISelectionType.vsUISelectionTypeSelect);
        }

        private string GetItemName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var activeDocumentFullPath = dte.ActiveDocument.FullName.Split(new char[] { '\\' });
            var fullPathFromSolution = new List<string>();
            var isItemInSolution = false;
            var solutionName = GetSolutionName();

            foreach (var activeDocumentPathItem in activeDocumentFullPath)
            {
                if (activeDocumentPathItem == solutionName)
                    isItemInSolution = true;

                if (isItemInSolution)
                    fullPathFromSolution.Add(activeDocumentPathItem);
            }

            return string.Join("\\", fullPathFromSolution);
        }

        private string GetSolutionName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionWithPath = dte.Solution.FileName.Replace(".sln", string.Empty);
            var solutionPathItems = solutionWithPath.Split('\\');

            return solutionPathItems.LastOrDefault();
        }

        #endregion Private Methods
    }
}