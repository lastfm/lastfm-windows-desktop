using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionIncrementBuildTask
{
	public class PreBuildObjectCleanupTask
	{
		public string ProjectPath { get; set; }
		
		public bool Execute()
		{
			try
			{
				if (String.IsNullOrEmpty(ProjectPath)) throw new ArgumentException("ProjectPath must have a value");

				CleanUpDebugDirectory(ProjectPath);

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		private static void CleanUpDebugDirectory(string projectPath)
		{
			if (Directory.Exists(projectPath))
			{
				string objectPath = Path.Combine(projectPath, @"obj\debug");

				if (Directory.Exists(objectPath))
				{
					foreach (FileInfo file in new DirectoryInfo(objectPath).GetFiles())
					{
						try
						{
							File.Delete(file.FullName);
						}
						catch (Exception ex)
						{
						}
					}
				}

				//objectPath = Path.Combine(projectPath, @"bin\debug");

				//if (Directory.Exists(objectPath))
				//{
				//	foreach (FileInfo file in new DirectoryInfo(objectPath).GetFiles())
				//	{
				//		try
				//		{
				//			File.Delete(file.FullName);
				//		}
				//		catch (Exception ex)
				//		{
				//		}
				//	}
				//}
			}
		}
	}
}
