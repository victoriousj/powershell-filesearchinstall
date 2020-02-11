using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace install_filesearch
{
    class Program
    {
        private const string FILE_NAME = "FileSearch.ps1";
        // true = cyan - false = pink
        private static bool CyanOrPink = true; 
        
        static void Main(string[] args)
        {
            FlipColorAndWrite($"{DateTime.Now} - This will install FileSearch. Press 'Enter' to continue, or exit this to abort...");

            Console.ReadLine();
            Console.SetCursorPosition(0, Console.CursorTop - 1);

            FlipColorAndWrite($"{DateTime.Now} - Installing FileSearch on PATH for user {Environment.UserName}...");

            var localAppDataDirectory = Environment.GetEnvironmentVariable("LocalAppData");

            var programAppDataDirectory = localAppDataDirectory + @"\Programs\";

            var fileSearchInstallDirectory = programAppDataDirectory + "FileSearch";

            var regexAddPath = Regex.Escape(fileSearchInstallDirectory);

            // Prevent FileSearch from being added to path again if already installed.
            var environmentVariables = Environment
                .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                .Split(';')
                .Where(x =>
                    !string.IsNullOrEmpty(x) &&
                    !Regex.IsMatch(x, $"^{regexAddPath}?")
                )
                .Append(fileSearchInstallDirectory);

            var newEnvironmentVariable = string.Join(";", environmentVariables);

            FlipColorAndWrite($"{DateTime.Now} - Creating directory in {fileSearchInstallDirectory}...");

            var fileSearchFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FILE_NAME);

            var fileSearchDir = Directory.CreateDirectory(fileSearchInstallDirectory);

            var fileSearchDestination = new FileInfo($"{fileSearchDir.FullName}\\{FILE_NAME}");

            if (fileSearchDestination.Exists)
            {
                FlipColorAndWrite($"{DateTime.Now} - Old version found... Overriding...");
                try
                {
                    File.Delete(fileSearchDestination.FullName);
                }
                catch (IOException ex)
                {
                    FlipColorAndWrite($"{DateTime.Now} - {ex.Message}");
                    return;
                }
            }

            FlipColorAndWrite($"{DateTime.Now} - Copying file...");

            try
            {
                // PowerShell throws a cautionary message for scripts download from the 
                // internet (rightfully so) but we will create a brand new file that contains
                // the contents of the original but is missing the flag that says it was 
                // downloaded ¬‿¬ 
                using (var streamReader = new StreamReader(fileSearchFile))
                using (var streamWriter = new StreamWriter(fileSearchDestination.FullName))
                {
                    string contents = streamReader.ReadToEnd();
                    streamWriter.Write(contents);
                }
            } catch (Exception ex)
            {
                FlipColorAndWrite("Please download repository and try again");
                FlipColorAndWrite(ex.Message);
            }

            FlipColorAndWrite($"{DateTime.Now} - File copied...");

            FlipColorAndWrite($"{DateTime.Now} - Setting PATH variable to include FileSearch...");

            Environment.SetEnvironmentVariable("Path", newEnvironmentVariable, EnvironmentVariableTarget.User);

            FlipColorAndWrite($"{DateTime.Now} - PATH variable set...");

            FlipColorAndWrite($"{DateTime.Now} - Complete...");

            FlipColorAndWrite($"{DateTime.Now} - Open PowerShell to start using...");

            Console.ReadLine();
        }

        static void FlipColorAndWrite(string message)
        {
            CyanOrPink = !CyanOrPink;
            Console.ForegroundColor = CyanOrPink 
                ? ConsoleColor.Cyan 
                : ConsoleColor.Magenta;
            Console.WriteLine($"{message}\n");
        }
    }
}
