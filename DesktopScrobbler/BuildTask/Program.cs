using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace VersionIncrementBuildTask
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static void Main(string[] args)
        {
	        if (args.Length == 0)
	        {
				Console.WriteLine("VERSION UPDATE FAILED: You must include the solution path and project start date in the pre-build command line parameters");
	        }
	        else
	        {
		        var splitArgs = args[0].Split(';');

				var Content = File.ReadAllText(splitArgs[0]);

		        Regex projReg = new Regex("Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\"", RegexOptions.Compiled);

		        var matches = projReg.Matches(Content).Cast<Match>();
		        var Projects = matches.Select(x => x.Groups[2].Value).ToList();

		        for (int i = 0; i < Projects.Count; ++i)
		        {
			        if (!Path.IsPathRooted(Projects[i]))
			        {
						Projects[i] = Path.Combine(Path.GetDirectoryName(splitArgs[0]), Projects[i]);
			        }
			        Projects[i] = Path.GetFullPath(Projects[i]);
			        Console.WriteLine(string.Format("Updating project: {0}....", Projects[i]));

					FileInfo projectFile = new FileInfo(Projects[i]);
			        string projectDirectory = projectFile.Directory.FullName;
			        string assemblyFilePath = string.Format("{0}\\Properties\\AssemblyInfo.cs", projectDirectory);

			        if (Directory.Exists(projectDirectory))
			        {
						PreBuildObjectCleanupTask cleanupTask = new PreBuildObjectCleanupTask() { ProjectPath = projectDirectory };
				        cleanupTask.Execute();
			        }

			        if (File.Exists(assemblyFilePath))
			        {
						AutoIncrementTask task = new AutoIncrementTask() { AssemblyInfoPath = assemblyFilePath };
				        DateTime projectDate = DateTime.Now;


				        DateTime.TryParse(splitArgs[1], out projectDate);
				        task.ProjectStartDate = projectDate;

						if (splitArgs.Length == 3)
						{
							task.MajorVersion = int.Parse(splitArgs[2]);
						}

				        task.Execute();
			        }
		        }
	        }
        }
    }
}
