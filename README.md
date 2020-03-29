# EntityFrameworkCore.MySqlUpdater
An entity core framework extension package, to auto apply sql files.


# Installation

Use one of these possibilities to add the package to your project.  

```Install-Package EntityFrameworkCore.MySqlUpdater```

```dotnet add package EntityFrameworkCore.MySqlUpdater```

```<PackageReference Include="EntityFrameworkCore.MySqlUpdater"" />```

```paket add EntityFrameworkCore.MySqlUpdater```


# Idea
The idea behind this package is to extend EFCore with an auto updater for mysql databases. It is tedious to apply single sql files one by one, and even if you put them together in one big file you most likely can execute them more than once, because of update queries which will fail. 
This package introduces convenience functions to apply sql files, keep track of already applied files and executing single sql files.
This package is not meant to be used for executing sql files that are not in your direct control. So never use it with queries that are vulnerable to sql injection attacks etc.




# Usage

The package is using a database table called updates, to store hashsums of applied files to your database.
The updates folder contains information about the date and speed of execution, as well as the filename and the SHA hash.  
We will refer to the system as HashSumTracker.  
This HashSumTracker is used to prevent multiple executions of already applied sql files. This will cause issue if your sql files, contain update statements.

-----

## Example
One intended example workflow can be described as the following.
A server is setup and at startup one can call the ApplyBaseFile function. This function will check if the schema is already populated. If it is populated it will return without executing the sql file. If the schema is empty ( table count == 0), the sql will be apllied. 
After the sql base file is applied, one can call ApplyUpdates(List of update folders). The package will iterate through all folders, trying to execute every sql file to your db. It is trying, because if you did not disable hashSumTracking, the tool is checking if the file is not already applied to your database. If the sql file is already applied, it will skip it and continue with the next one.


## Available adjustable parameters:

- Timeout: Specifies the mysql command timeout. Default = 60 seconds
- DebugOutput: Activate/Deactivate the debugoutput. Default = false
- Hashsumtracking: Activate/Deactivate the hashsum tracking. Default = true 

## Examples

Create the update table  
```
await _context.CreateUpdatesTable().ConfigureAwait(false);
```


Apply all sql files from given folders.
```
List<string> folders = new List<string> { "PATH_TO_YOUR_FOLDER", "PATH_TO_ANOTHER_FOLDER" };
await _context.ApplyUpdates(folders, true).ConfigureAwait(false);
```  


Apply a single file
```
await _context.ApplySQLFile("PATH_TO_YOUR_FILE");
```


Apply a base file

```
_context.ApplyBaseFile("YOUR_SCHAME_NAME","PATH_TO_YOUR_BASE_FILE");
```



