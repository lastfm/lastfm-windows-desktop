using System;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client; 

namespace VersionIncrementBuildTask
{
   public class AutoIncrementTask
   {
      public string AssemblyInfoPath { get; set; }
	  public int MajorVersion { get; set; }
	  public DateTime ProjectStartDate { get; set; }

      public bool Execute()
      {
         try
         {
            if (String.IsNullOrEmpty(AssemblyInfoPath)) throw new ArgumentException("AssemblyInfoPath must have a value");

			var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(AssemblyInfoPath);

	         FileAttributes attributes = File.GetAttributes(AssemblyInfoPath);

	         if (attributes.HasFlag(FileAttributes.ReadOnly))
	         {
		         try
		         {
			         using (var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri))
			         {
				         var workspace = workspaceInfo.GetWorkspace(server);
				         workspace.PendEdit(AssemblyInfoPath);
			         }
		         }
		         catch (Exception)
		         {
			         try
					 {					
						// Try and make the file 'Non readonly'
						File.SetAttributes(AssemblyInfoPath, FileAttributes.Normal);
			         }
					 catch (Exception ex)
			         {
						 Console.Out.WriteLine(ex);
						 return false;

			         }
		         }
	         }


	         string[] content = File.ReadAllLines(AssemblyInfoPath, Encoding.UTF8);

	         int lineIndex = 0;

	         foreach (string line in content)
	         {
				 if (line.Trim().StartsWith("[assembly: AssemblyVersion"))
				 {
					 content[lineIndex] = GetNewVersion(line);
					 break;
				 }
		         lineIndex ++;
	         }

			File.WriteAllLines(AssemblyInfoPath, content, Encoding.UTF8);
			}
         catch (Exception ex)
         {
            Console.Out.WriteLine(ex);
            return false;
         }

         return true;
      }


      private string GetNewVersion(string versionLine)
      {
	     int verStartPos = versionLine.IndexOf('\"');
	     int verEndPos = versionLine.IndexOf('\"', verStartPos + 1);

	     string currentVersion = versionLine.Substring(verStartPos + 1, verEndPos - verStartPos - 1);
		 string[] currentVersionParams = currentVersion.Split('.');

	     int day = DateTime.Now.Day;
	     int months = Math.Abs(((DateTime.Now.Year - ProjectStartDate.Year)*12) + DateTime.Now.Month - ProjectStartDate.Month) + 1;
		 int revision = 1;

		 string newVersionFormat = "{0}.{1}.{2}.{3}";
		 string newVersion = string.Empty;

		 if (months == int.Parse(currentVersionParams[1]) && day == int.Parse(currentVersionParams[2]))
		 {
			 revision = int.Parse(currentVersionParams[3]) + 1;
		 }

		newVersion = string.Format(newVersionFormat, MajorVersion, months, day, revision);
		return versionLine.Replace(currentVersion, newVersion);
      }
   }
}
