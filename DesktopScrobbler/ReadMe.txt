This project was built using Visual Studio 2015, Update 3.  
The startup project is 'Desktop Scrobbler' (right-click on the project and choose 'Set as StartUp project').

The 'BuildTask' project is used to automate version numbering based on the current date, it must be built first to successfully build the rest of the project.
It will automatically amend the 'properties -> AssemblyInfo.cs' of every project to update the current version number.

You can remove this requirement, by right-clicking on the 'DesktopScrobbler' project, selecting 'Properties', selecting 'Build Events' and removing the 'Pre-build event command line'

Any dependencies are downloaded using the NuGet package manager when your first run the project.

To successfully run the project, you must substitute the values in the 'Common -> Static Classes -> APIDetails.cs' with your own details.
