using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

	         FileAttributes attributes = File.GetAttributes(AssemblyInfoPath);

	         if (attributes.HasFlag(FileAttributes.ReadOnly))
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


	         string[] content = File.ReadAllLines(AssemblyInfoPath, Encoding.UTF8);

                int lineIndex = 0;
                bool needsFileVersionAdding = true;

                foreach (string line in content)
                {
                    if (line.Trim().StartsWith("[assembly: AssemblyVersion"))
                    {
                        content[lineIndex] = GetNewVersion(line);
                    }
                    else if (line.Trim().StartsWith("[assembly: AssemblyFileVersion"))
                    {
                        needsFileVersionAdding = false;
                        content[lineIndex] = GetNewVersion(line);
                    }
                    lineIndex++;
                }

                if (needsFileVersionAdding)
                {
                    List<string> fileLines = content.ToList();
                    fileLines.Add(GetNewVersion("[assembly: AssemblyFileVersion(\"1.0.0.0\")]"));
                    content = fileLines.ToArray();
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
	     int months = Math.Abs(((DateTime.Now.Year - ProjectStartDate.Year) * 12) + DateTime.Now.Month - ProjectStartDate.Month);
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
