using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace install_filesearch
{
    class Program
    {
        const string FILE_NAME = "FileSearch.ps1";
        // true = cyan - false = pink
        static bool CyanOrPink;

        static void Main(string[] args)
        {
            FlipColorAndWrite($"This will install FileSearch. Press 'Enter' to continue, or exit this to abort...");

            Console.ReadLine();
            Console.SetCursorPosition(0, Console.CursorTop - 1);

            FlipColorAndWrite($"Installing FileSearch on PATH for user {Environment.UserName}...");

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

            FlipColorAndWrite($"Creating directory in {fileSearchInstallDirectory}...");

            var fileSearchFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FILE_NAME);

            var fileSearchDir = Directory.CreateDirectory(fileSearchInstallDirectory);

            var fileSearchDestination = new FileInfo($"{fileSearchDir.FullName}\\{FILE_NAME}");

            if (fileSearchDestination.Exists)
            {
                FlipColorAndWrite($"Old version found... Overriding...");
                try
                {
                    File.Delete(fileSearchDestination.FullName);
                }
                catch (IOException ex)
                {
                    FlipColorAndWrite($"{ex.Message}");
                    return;
                }
            }

            FlipColorAndWrite($"Copying file...");

            try
            {
                // PowerShell throws a cautionary message for scripts downloaded from the 
                // internet (rightfully so) but we will create a brand new file that contains
                // the contents of the original but is missing the flag that says it was 
                // downloaded ¬‿¬ 
                using (var streamReader = new StreamReader(fileSearchFile))
                using (var streamWriter = new StreamWriter(fileSearchDestination.FullName))
                {
                    string contents = streamReader.ReadToEnd();
                    streamWriter.Write(contents);
                }
            }
            catch (Exception ex)
            {
                FlipColorAndWrite("Please download repository and try again");
                FlipColorAndWrite(ex.Message);
            }

            FlipColorAndWrite($"File copied...");

            FlipColorAndWrite($"Setting PATH variable to include FileSearch...");

            Environment.SetEnvironmentVariable("Path", newEnvironmentVariable, EnvironmentVariableTarget.User);

            FlipColorAndWrite($"PATH variable set...");

            string executionPolicy = GetExecutionPolicy();
            if (!executionPolicy.Equals("Unrestricted"))
            {
                // This is a major security concern. If you don't know the implications of this, don't say yes.
                var changeExecutionPolicy = Confirm("Do you want to change your Execution Policy to run scripts? (may be required to use this) [y/n]");
                if (changeExecutionPolicy)
                {
                    FlipColorAndWrite("Changing Execution Policy to run scripts...");
                    using (PowerShell powerShell = PowerShell.Create())
                    {
                        powerShell.AddCommand("Set-ExecutionPolicy")
                            .AddArgument("Unrestricted")
                            .AddParameter("Scope", "CurrentUser")
                            .Invoke();
                    }
                }
                else
                {
                    FlipColorAndWrite("Execution Policy not altered. You may need to do further configuration to run this...");
                }
            }

            FlipColorAndWrite($"Complete...");

            FlipColorAndWrite($"Open PowerShell to start using...");

            Console.ReadLine();
        }

        static bool Confirm(string title)
        {
            ConsoleKey response;
            do
            {
                FlipColorAndWrite($"{ title }");
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                response = Console.ReadKey(true).Key;
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            Console.SetCursorPosition(0, Console.CursorTop + 2);
            return (response == ConsoleKey.Y);
        }

        static void FlipColorAndWrite(string message)
        {
            CyanOrPink = !CyanOrPink;
            Console.ForegroundColor = CyanOrPink
                ? ConsoleColor.Cyan
                : ConsoleColor.Magenta;
            Console.WriteLine($"{DateTime.Now} - {message}\n");
        }

        static string GetExecutionPolicy()
        {
            using (var powerShell = PowerShell.Create())
            {
                Collection<PSObject> obj = powerShell
                    .AddCommand("Get-ExecutionPolicy")
                    .AddParameter("Scope", "CurrentUser")
                    .Invoke();

                return $"{obj.FirstOrDefault()}";
            }
        }
    }
}
