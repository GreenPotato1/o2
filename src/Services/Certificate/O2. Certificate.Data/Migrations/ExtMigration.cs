using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.Business.Data.Migrations
{
    public static class ExtMigration
    {
        public static void Scripts(MigrationBuilder migrationBuilder)
        {
            const string path = "src";
            var scriptsDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            while (scriptsDir != null && scriptsDir.FullName.Contains(path))
            {
                scriptsDir = scriptsDir.Parent;
            }

            if (scriptsDir != null)
            {
                var scriptsDirFullname = scriptsDir.FullName+"/src/O2.Business.Database/";
                var dirFunctions = scriptsDirFullname + "dbo/functions/";
                var dirSchemas = scriptsDirFullname + "schemas/";
            
                const string fileLog = "MigrationScripts.txt";
                using (var sw = File.CreateText(fileLog))
                {
                    RunSqlScriptsFromDir(migrationBuilder, dirSchemas, sw);
                    RunSqlScriptsFromDir(migrationBuilder, dirFunctions, sw);
                }
            }
        }

        private static void RunSqlScriptsFromDir(MigrationBuilder migrationBuilder, 
            string dirFunctions, 
            StreamWriter sw)
        {
            foreach (var fileFunc in Directory.GetFiles(dirFunctions))
            {
                var fullname = Path.Combine(fileFunc);
                // Create a file to write to.
                sw.WriteLine(fullname);
                migrationBuilder.Sql(File.ReadAllText(fullname));
            }
            
        }
    }
}