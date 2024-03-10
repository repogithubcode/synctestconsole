using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
namespace ProEstimatorData.Services
{
    public static class MigrationService
    {
        public static void RunMigrations()
        {
            try
            {
                DBAccess db = new DBAccess();

                // Create the Migrations table if it does not exist
                var existsResult = db.ExecuteWithIntReturnForQuery("SELECT OBJECT_ID('FocusWrite.dbo._DatabaseMigrations')");
                if (existsResult.Success && existsResult.Value == 0)
                {
                    var createResult = db.ExecuteSql("CREATE TABLE [FocusWrite].[dbo].[_DatabaseMigrations]([DateOfExecution] [datetime], [MigrationFileName] [varchar](255) NOT NULL)");
                    if (!createResult.Success)
                    {
                        ErrorLogger.LogError($"Error encountered when trying to create the migrations table. Error: {createResult.ErrorMessage}",
                            "MigrationService RunMigrations");
                        return;
                    }
                }
                else if (!existsResult.Success)
                {
                    ErrorLogger.LogError($"Error encountered when trying to check for the existance of the migrations table. Error: {existsResult.ErrorMessage}",
                        "MigrationService RunMigrations");
                    return;
                }

                var migrationsResult = db.ExecuteWithTableForQuery("SELECT MigrationFileName FROM _DatabaseMigrations", false);
                if (migrationsResult.Success)
                {
                    // Set the list of migrations that have been executed against the database
                    var migrationsRun = migrationsResult.DataTable
                        .AsEnumerable()
                        .Select(x => x.Field<string>("MigrationFileName"))
                        .ToArray();

                    // Get the list of migration sql files stored in the project, ordering by file name (date) to ensure they are executed in the correct order
                    var dataMigrationsPath = Path.Combine($"{HttpContext.Current.Server.MapPath("")}", "DbMigrations");
                    var migrationDirectories = new DirectoryInfo(dataMigrationsPath);

                    foreach (var migrationDirectory in migrationDirectories.GetDirectories().OrderBy(x => x.Name))
                    {
                        var migrationFileNames = migrationDirectory
                            .GetFiles("*.sql")
                            .Select(x => x.Name)
                            .OrderBy(x => x)
                            .ToList();

                        // Run migrations that are in the directory but not in the database
                        foreach (var migrationFileName in migrationFileNames.Where(x => !migrationsRun.Contains($"{migrationDirectory.Name}/{x}")))
                        {
                            var sql = File.ReadAllText(Path.Combine(dataMigrationsPath, migrationDirectory.Name, migrationFileName));

                            // Remove GO lines, they mess up the execution when done this way.
                            sql = Regex.Replace(sql, @"^GO\s*\n", "\n", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

                            bool errors = false;
                            
                            var sqlResult = db.ExecuteSql(sql);
                            if (!sqlResult.Success)
                            {
                                ErrorLogger.LogError($"Error encountered while running database migration for file '{migrationFileName}'. Error: {sqlResult.ErrorMessage}",
                                    "MigrationService RunMigrations");

                                errors = true;
                            }

                            // Insert row into the migrations table so that the migration is not run again
                            bool onProduction = true;
#if DEBUG
                            onProduction = false;
#endif

                            // On production, always insert the record so that it doesn't run again, even if there were errors.
                            // When running locally, don't insert when there are errors, so another attempt will be made on the next run.
                            if (!errors || onProduction)
                            {
                                var insertResult = db.ExecuteSql($"INSERT INTO _DatabaseMigrations (DateOfExecution, MigrationFileName) VALUES ('{DateTime.UtcNow}', '{migrationDirectory.Name}/{migrationFileName}')");
                                if (!insertResult.Success)
                                {
                                    ErrorLogger.LogError($"Error encountered AFTER running database migration for file '{migrationFileName}' when trying to add the row to the migrations table. Error: {insertResult.ErrorMessage}",
                                        "MigrationService RunMigrations");
                                }
                            }
                        }
                    }
                }
                else
                {
                    ErrorLogger.LogError($"Error encountered when trying to select the list of migrations from the database. Error: {migrationsResult.ErrorMessage}",
                        "MigrationService RunMigrations");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Error encountered while running database migrations. Exception: {ex}", "MigrationService RunMigrations");
            }
        }

    }
}

