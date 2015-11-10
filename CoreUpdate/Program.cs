using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CoreUpdate
{
    public class Program
    {
        private static ConsoleColor _originalConsoleColor;

        public static int Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            _originalConsoleColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"CoreUpdate version {Assembly.GetExecutingAssembly().GetName().Version}\r\n");

            if (args.Length == 0 || args.Length > 2)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(@"Syntax:
CoreUpdate version [solution-path]

version:        The Harcourts Core nuget version to use.
solution-path:  The path to the solution (optional).
                Defaults to the current directory.
");
                Console.WriteLine(@"e.g. CoreUpdate 6.2.0-beta0001 D:\Repos\MySolution");
                Console.ForegroundColor = _originalConsoleColor;
                return 1;
            }

            // Version is the first argument.
            var version = args[0];
            // Determine the number version. e.g. "6.2.0-beta0001" is "6.2.0.0".
            var versionNumberMatch = Regex.Match(version, @"^(?<version>[0-9|\.]{5,})");
            if (versionNumberMatch.Groups.Count < 1)
            {
                WriteConsoleError($"'{version}' is an invalid version. There must be at least three parts to the version number.");
                return 3;
            }
            var versionNumber = string.Join(".", (versionNumberMatch.Groups[1].Value + ".0").Split('.').Take(4));

            // Solution folder is the 2nd argument (if present) otherwise the current folder.
            var folder = args.Length > 1 ? args[1] : Environment.CurrentDirectory;
            if (!Directory.Exists(folder))
            {
                WriteConsoleError($"Folder '{folder}' does not exist.");
                return 2;
            }

            var result = Update(folder, version, versionNumber);
            if (result == 0)
            {
                stopwatch.Stop();
                Console.WriteLine($"\r\nDone in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }

            // Reset console color.
            Console.ForegroundColor = _originalConsoleColor;
            return 0;
        }

        private static int Update(string folder, string version, string versionNumber)
        {
            var di = new DirectoryInfo(folder);

            var configFileInfos = di.GetFiles("packages.config", SearchOption.AllDirectories).ToList();
            if (!configFileInfos.Any())
            {
                WriteConsoleError($"No packages.config files found in '{di.FullName}' or sub folders.");
                return 4;
            }

            var projectFileInfos = di.GetFiles("*.csproj", SearchOption.AllDirectories).ToList();
            if (!projectFileInfos.Any())
            {
                WriteConsoleError($"No *.csproj files found in '{di.FullName}' or sub folders.");
                return 5;
            }

            // Update packages.config files.
            Console.WriteLine($"Updating to Harcourts.Core to version {version} ({versionNumber})...");
            Console.WriteLine($"\r\nUpdating packages.config for {di.FullName}:");

            foreach (var fi in configFileInfos)
            {
                var text = File.ReadAllText(fi.FullName);
                text = Regex.Replace(
                    text,
                    @"id=\""(?<name>Harcourts\.Core.*?)\"" version=\""(?<version>.*?)\""",
                    @"id=""$1"" version=""" + version + @"""");

                File.WriteAllText(fi.FullName, text);
                Console.WriteLine(fi.FullName.Replace(di.FullName + @"\", string.Empty));
            }

            // Update *.csproj files.
            Console.WriteLine($"\r\nUpdating *.csproj files for {di.FullName}:");

            foreach (var fi in projectFileInfos)
            {
                var text = File.ReadAllText(fi.FullName);
                text = Regex.Replace(
                    text,
                    @"Include=\""(?<name>Harcourts\.Core.*?), Version=(?<version>.*?),",
                    @"Include=""$1, Version=" + versionNumber + ",");
                text = Regex.Replace(
                    text,
                    @"\\(?<name>Harcourts\.Core.*?)\.(?<version>\d.*?)\\",
                    @"\$1." + version + @"\");

                File.WriteAllText(fi.FullName, text);
                Console.WriteLine(fi.FullName.Replace(di.FullName + @"\", string.Empty));
            }

            return 0;
        }

        private static void WriteConsoleError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = _originalConsoleColor;
        }
    }
}
