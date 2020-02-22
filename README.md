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


The tool returns some statusCode to let you know if everything checks out or if complications might occur.

| Name                   	| ID   	| Explanation                                                                	|
|------------------------	|------	|----------------------------------------------------------------------------	|
| NO_MATCHING_PATH       	| 0x00 	| Path could not be found                                                    	|
| NOT_A_SQL_FILE         	| 0x10 	| File is not an sql file                                                    	|
| INSECURE_SQL_QUERY     	| 0x20 	| SQL is considered insecure                                                 	|
| EMPTY_CONTENT          	| 0x30 	| Content of sql file is empty                                               	|
| UPDATE_ALREADY_APPLIED 	| 0x40 	| The file is already applied                                                	|
| SCHEMA_NOT_EMPTY       	| 0x50 	| The schema which is selected for the base file is not empty                	|
| UPDATE_TABLE_MISSING   	| 0x60 	| The updates table is missing. This table is required for the HashSumTracking 	|
| FILE_NOT_FOUND         	| 0x70 	| The file was not found                                                     	|



## Examples

Create the update table
```
// Creates the updates table
await _context.CreateUpdatesTable().ConfigureAwait(false);
```


Apply all sql files from given folders.
```
// Applies all sql files from a given folder
List<string> folders = new List<string> { "PATH_TO_YOUR_FOLDER" };
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



