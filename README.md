# EntityFrameworkCore.MySqlUpdater
A entity core framework extension package, to auto apply sql files.


# Installation

Use one of these possibilities to add the package to your project.  

```Install-Package EntityFrameworkCore.MySqlUpdater```

```dotnet add package EntityFrameworkCore.MySqlUpdater```

```<PackageReference Include="EntityFrameworkCore.MySqlUpdater"" />```

```paket add EntityFrameworkCore.MySqlUpdater```



# Usage

The package is using a database table called updates, to store files applied to your database.
The updates folder contains information about the date and speed of execution, as well as the filename and the SHA hash.

We will refer to the system as HashSumTracker.

This HashSumTracker is used to prevent multiple executions of already applied sql files. This will cause issue if your sql files, contain update statements.

However, it is possible to deactivate the HashSumTracker. Just use the optional parameter ```hashSumTracking```, available for each function and set it to false.