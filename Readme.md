# Core Update

Console application to quickly update Harcourts.Core packages.config and *.csproj files in a solution
rather than using the slow and cumbersome nuget package manager and/or command line.

**Syntax**:

    CoreUpdate version [solution-path]

    version:        The Harcourts Core nuget version to use.
    solution-path:  The path to the solution (optional).
                    Defaults to the current directory.

**Examples**:

    CoreUpdate 6.2.0-beta0001

    CoreUpdate 6.2.0-beta0001 D:\Repos\MySolution


## Download

Download the executable without pulling the repository and building the executable:

[CoreUpdate.exe](https://github.com/Harcourts/CoreUpdate/blob/master/Download/CoreUpdate.exe?raw=true)