using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace install_filesearch
{
    class Program
    {
        private const string FILE_NAME = "FileSearch.ps1";
        private static bool CyanOrPink = true; // true = cyan - false = pink
        static void Main(string[] args)
        {
            FlipColorAndWrite($"{DateTime.Now} - Installing FileSearch on PATH for user {Environment.UserName}...");

            var localAppDataDirectory = Environment.GetEnvironmentVariable("LocalAppData");

            var programAppDataDirectory = localAppDataDirectory + @"\Programs\";

            var fileSearchInstallDirectory = programAppDataDirectory + "FileSearch";

            var regexAddPath = Regex.Escape(fileSearchInstallDirectory);

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

            File.Copy(fileSearchFile, fileSearchDestination.FullName);

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
            Console.ForegroundColor = CyanOrPink ? ConsoleColor.Cyan : ConsoleColor.Magenta;
            Console.WriteLine(message);
        }
    }
}
