using CleanArchitecture.CodeGenerator.Helpers;
using CleanArchitecture.CodeGenerator.Models;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CleanArchitecture.CodeGenerator
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.guidCodeGeneratorPkgString)]
	public sealed class CodeGeneratorPackage : AsyncPackage
	{
		private const string _solutionItemsProjectName = "Solution Items";
		private static readonly Regex _reservedFileNamePattern = new Regex($@"(?i)^(PRN|AUX|NUL|CON|COM\d|LPT\d)(\.|$)");
		private static readonly HashSet<char> _invalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
		public List<string> ActionList = new List<string> {
			"Create",
			"Update",
			"Delete",
			"GetAll",
			"GetById",
			"GetAllWithPagination"
		};

		private const string CommandProjectName = "Camms.Risk.Application.Command";
		private const string QueryProjectName = "Camms.Risk.Application.Query";




		public static DTE2 _dte;

		protected async override System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{

			// starting point
			await JoinableTaskFactory.SwitchToMainThreadAsync();

			_dte = await GetServiceAsync(typeof(DTE)) as DTE2;
			Assumes.Present(_dte);

			Logger.Initialize(this, Vsix.Name);

			if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
			{
				CommandID menuCommandID = new CommandID(PackageGuids.guidCodeGeneratorCmdSet, PackageIds.cmdidMyCommand);
				OleMenuCommand menuItem = new OleMenuCommand(ExecuteAsync, menuCommandID);
				mcs.AddCommand(menuItem);
			}
		}

		private void ExecuteAsync(object sender, EventArgs e)
		{
			NewItemTarget target = NewItemTarget.Create(_dte);

			if (!(target.Project?.Name == CommandProjectName || target.Project?.Name == QueryProjectName))
			{
				MessageBox.Show(
							"Selected class library not expected one to generate files. It should be 'Camms.Risk.Application.Command' or 'Camms.Risk.Application.Query'",
							Vsix.Name,
							MessageBoxButton.OK,
							MessageBoxImage.Error);
			}


			NewItemTarget domain = NewItemTarget.Create(_dte, "Camms.Risk.Domain.Entity");
			//NewItemTarget domain= NewItemTarget.Create(_dte, "Domain");
			//domain.Directory = @"C:\Developments\CAMMS\Camms.Risk\Camms.Risk\Camms.Risk.Domain.Entity";
			// get all domain classes from domain project
			var includes = new string[] { "IEntity" };
			var entities = ProjectHelpers.GetEntities(domain.Project)
				//.Where(x=>includes.Contains(x.BaseName) && !includes.Contains(x.Name))
				.Where(o => o.IsImplementedFromIEntity)
				.Select(x => x.Name)
				.Distinct().ToArray();

			if (target == null)
			{
				MessageBox.Show(
						"Could not determine where to create the new file. Select a file or folder in Solution Explorer and try again.",
						Vsix.Name,
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				return;
			}

			var (input, actions) = PromptForFileName(target.Directory, entities);
			input = input.TrimStart('/', '\\').Replace("/", "\\");
			

			if (string.IsNullOrEmpty(input))
			{
				return;
			}

			string[] parsedInputs = GetParsedInput(input);

			foreach (string inputname in parsedInputs)
			{
				try
				{
					var name = Path.GetFileNameWithoutExtension(inputname);
					var nameofPlural = ProjectHelpers.Pluralize(name);
					var templates = new TemplateModelValues().Templates;
					
					foreach (var item in actions)
					{
						var selectedTemplates = templates.Where(o => o.Action == item);
                        foreach (var template in selectedTemplates)
                        {
							// replace template file names
							template.FilePath = template.FilePath.Replace("$NAME_OF_PLURAL", nameofPlural).Replace("$NAME", name);

							// add item async
							AddItemAsync(template.FilePath, name, target, item, template).Forget();
						}
					}
				}
				catch (Exception ex) when (!ErrorHandler.IsCriticalException(ex))
				{
					Logger.Log(ex);
					MessageBox.Show(
							$"Error creating file '{inputname}':{Environment.NewLine}{ex.Message}",
							Vsix.Name,
							MessageBoxButton.OK,
							MessageBoxImage.Error);
				}
			}
		}

		private async Task AddItemAsync(string name, string itemname, NewItemTarget target, string action, TemplateModel template)
		{
			// The naming rules that apply to files created on disk also apply to virtual solution folders,
			// so regardless of what type of item we are creating, we need to validate the name.
			ValidatePath(name);

			if (name.EndsWith("\\", StringComparison.Ordinal))
			{
				if (target.IsSolutionOrSolutionFolder)
				{
					GetOrAddSolutionFolder(name, target);
				}
				else
				{

					AddProjectFolder(name, target);
				}
			}
			else
			{
				await AddFileAsync(name, itemname, target, action, template);
			}
		}

		private void ValidatePath(string path)
		{
			do
			{
				string name = Path.GetFileName(path);

				if (_reservedFileNamePattern.IsMatch(name))
				{
					throw new InvalidOperationException($"The name '{name}' is a system reserved name.");
				}

				if (name.Any(c => _invalidFileNameChars.Contains(c)))
				{
					throw new InvalidOperationException($"The name '{name}' contains invalid characters.");
				}

				path = Path.GetDirectoryName(path);
			} while (!string.IsNullOrEmpty(path));
		}

		private async System.Threading.Tasks.Task AddFileAsync(string name, string itemname, NewItemTarget target, string action, TemplateModel template)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			//target.Directory = target.Directory.Replace("Commands\\AcceptChanges\\", "");
			//name = name.Replace("Commands/AcceptChanges/", "");
			FileInfo file;

			// If the file is being added to a solution folder, but that
			// solution folder doesn't have a corresponding directory on
			// disk, then write the file to the root of the solution instead.
			if (target.IsSolutionFolder && !Directory.Exists(target.Directory))
			{
				file = new FileInfo(Path.Combine(Path.GetDirectoryName(_dte.Solution.FullName), Path.GetFileName(name)));
			}
			else
			{


				file = new FileInfo(Path.Combine(target.Directory, name));
			}

			// Make sure the directory exists before we create the file. Don't use
			// `PackageUtilities.EnsureOutputPath()` because it can silently fail.
			Directory.CreateDirectory(file.DirectoryName);

			if (!file.Exists)
			{
				Project project;

				if (target.IsSolutionOrSolutionFolder)
				{
					project = GetOrAddSolutionFolder(Path.GetDirectoryName(name), target);
				}
				else
				{
					project = target.Project;
				}

				int position = await WriteFileAsync(project, file.FullName, itemname, target.Directory, action, template);
				if (target.ProjectItem != null && target.ProjectItem.IsKind(Constants.vsProjectItemKindVirtualFolder))
				{
					target.ProjectItem.ProjectItems.AddFromFile(file.FullName);
				}
				else
				{
					project.AddFileToProject(file);
				}

				file = new FileInfo(Path.Combine(target.Directory, name));


				VsShellUtilities.OpenDocument(this, file.FullName);

				// Move cursor into position.
				if (position > 0)
				{
					Microsoft.VisualStudio.Text.Editor.IWpfTextView view = ProjectHelpers.GetCurentTextView();

					if (view != null)
					{
						view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, position));
					}
				}

				ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument");
				_dte.ActiveDocument.Activate();
			}
			else
			{
				MessageBox.Show($"The file '{file}' already exists.", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private static async Task<int> WriteFileAsync(Project project, string file, string itemname, string selectFolder, string action, TemplateModel template)
		{
			string templateContent = await TemplateMap.GetTemplateFilePathAsync(project, file, itemname, selectFolder, action, template);

			if (!string.IsNullOrEmpty(templateContent))
			{
				int index = templateContent.IndexOf('$');

				if (index > -1)
				{
					templateContent = templateContent.Remove(index, 1);
				}

				await WriteToDiskAsync(file, templateContent);
				return index;
			}

			await WriteToDiskAsync(file, string.Empty);

			return 0;
		}

		private static async System.Threading.Tasks.Task WriteToDiskAsync(string file, string content)
		{
			file = file.Replace("Commands\\AcceptChanges\\", "");
			using (StreamWriter writer = new StreamWriter(file, false, GetFileEncoding(file)))
			{
				await writer.WriteAsync(content);
			}
		}

		private static Encoding GetFileEncoding(string file)
		{
			string[] noBom = { ".cmd", ".bat", ".json" };
			string ext = Path.GetExtension(file).ToLowerInvariant();

			if (noBom.Contains(ext))
			{
				return new UTF8Encoding(false);
			}

			return new UTF8Encoding(true);
		}

		private Project GetOrAddSolutionFolder(string name, NewItemTarget target)
		{
			if (target.IsSolution && string.IsNullOrEmpty(name))
			{
				// An empty solution folder name means we are not creating any solution 
				// folders for that item, and the file we are adding is intended to be 
				// added to the solution. Files cannot be added directly to the solution,
				// so there is a "Solution Items" folder that they are added to.
				return _dte.Solution.FindSolutionFolder(_solutionItemsProjectName)
						?? ((Solution2)_dte.Solution).AddSolutionFolder(_solutionItemsProjectName);
			}

			// Even though solution folders are always virtual, if the target directory exists,
			// then we will also create the new directory on disk. This ensures that any files
			// that are added to this folder will end up in the corresponding physical directory.
			if (Directory.Exists(target.Directory))
			{
				// Don't use `PackageUtilities.EnsureOutputPath()` because it can silently fail.
				Directory.CreateDirectory(Path.Combine(target.Directory, name));
			}

			Project parent = target.Project;

			foreach (string segment in SplitPath(name))
			{
				// If we don't have a parent project yet, 
				// then this folder is added to the solution.
				if (parent == null)
				{
					parent = _dte.Solution.FindSolutionFolder(segment) ?? ((Solution2)_dte.Solution).AddSolutionFolder(segment);
				}
				else
				{
					parent = parent.FindSolutionFolder(segment) ?? ((SolutionFolder)parent.Object).AddSolutionFolder(segment);
				}
			}

			return parent;
		}

		private void AddProjectFolder(string name, NewItemTarget target)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Make sure the directory exists before we add it to the project. Don't
			// use `PackageUtilities.EnsureOutputPath()` because it can silently fail.
			Directory.CreateDirectory(Path.Combine(target.Directory, name));

			// We can't just add the final directory to the project because that will 
			// only add the final segment rather than adding each segment in the path.
			// Split the name into segments and add each folder individually.
			ProjectItems items = target.ProjectItem?.ProjectItems ?? target.Project.ProjectItems;
			string parentDirectory = target.Directory;

			foreach (string segment in SplitPath(name))
			{
				parentDirectory = Path.Combine(parentDirectory, segment);

				// Look for an existing folder in case it's already in the project.
				ProjectItem folder = items
						.OfType<ProjectItem>()
						.Where(item => segment.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
						.Where(item => item.IsKind(Constants.vsProjectItemKindPhysicalFolder, Constants.vsProjectItemKindVirtualFolder))
						.FirstOrDefault();

				if (folder == null)
				{
					folder = items.AddFromDirectory(parentDirectory);
				}

				items = folder.ProjectItems;
			}
		}

		private static string[] SplitPath(string path)
		{
			return path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string[] GetParsedInput(string input)
		{
			// var tests = new string[] { "file1.txt", "file1.txt, file2.txt", ".ignore", ".ignore.(old,new)", "license", "folder/",
			//    "folder\\", "folder\\file.txt", "folder/.thing", "page.aspx.cs", "widget-1.(html,js)", "pages\\home.(aspx, aspx.cs)",
			//    "home.(html,js), about.(html,js,css)", "backup.2016.(old, new)", "file.(txt,txt,,)", "file_@#d+|%.3-2...3^&.txt" };
			Regex pattern = new Regex(@"[,]?([^(,]*)([\.\/\\]?)[(]?((?<=[^(])[^,]*|[^)]+)[)]?");
			List<string> results = new List<string>();
			Match match = pattern.Match(input);

			while (match.Success)
			{
				// Always 4 matches w. Group[3] being the extension, extension list, folder terminator ("/" or "\"), or empty string
				string path = match.Groups[1].Value.Trim() + match.Groups[2].Value;
				string[] extensions = match.Groups[3].Value.Split(',');

				foreach (string ext in extensions)
				{
					string value = path + ext.Trim();

					// ensure "file.(txt,,txt)" or "file.txt,,file.txt,File.TXT" returns as just ["file.txt"]
					if (value != "" && !value.EndsWith(".", StringComparison.Ordinal) && !results.Contains(value, StringComparer.OrdinalIgnoreCase))
					{
						results.Add(value);
					}
				}
				match = match.NextMatch();
			}
			return results.ToArray();
		}

		private (string, List<string>) PromptForFileName(string folder, string[] entities)
		{
			DirectoryInfo dir = new DirectoryInfo(folder);
			FileNameDialog dialog = new FileNameDialog(dir.Name, entities);

			//IntPtr hwnd = new IntPtr(_dte.MainWindow.HWnd);
			//System.Windows.Window window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
			dialog.Owner = Application.Current.MainWindow;

			bool? result = dialog.ShowDialog();
			var inputValue = string.Empty;
			var selectedCommands = new List<string>();
			var selectedQueryies = new List<string>();
			if (result.HasValue && result.Value)
			{
				inputValue = dialog.Input;
				selectedCommands = dialog.CheckList.Where(o => o.IsSelected).Select(p => p.TheText).ToList();
			}

			return (inputValue, selectedCommands);
		}

		private void ExecuteCommandIfAvailable(string commandName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			Command command;

			try
			{
				command = _dte.Commands.Item(commandName);
			}
			catch (ArgumentException)
			{
				// The command does not exist, so we can't execute it.
				return;
			}

			if (command.IsAvailable)
			{
				_dte.ExecuteCommand(commandName);
			}
		}
	}
}