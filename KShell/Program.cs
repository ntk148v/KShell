using System.Diagnostics;
using Microsoft.VisualBasic;

namespace KShell;

class KShell
{
    // Test it in Linux only
    private static string _currUser = Environment.UserName;
    private static string _currDir = Directory.GetCurrentDirectory();
    private static string _hostname = Environment.MachineName;
    
    static void Main(string[] args)
    {
        
        // Run command loop
        while (true)
        {
            Console.Write($"{_currUser}@{_hostname}:{_currDir}$ ");
            // Read the keyboard input
            string? input = Console.ReadLine();
            if (String.IsNullOrEmpty(input))
            {
                continue;
            }
            
            execCommand(input);
        }
    }

    static void execCommand(string input)
    {
        // Split the input separate the command and the arguments
        string[] args = Strings.Split(Strings.RTrim(input), " ");
        
        // Check for the built-in shell commands
        switch (args[0])
        {
            case "cd":
                if (args.Length < 2)
                {
                    // cd to home with empty path
                    Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    _currDir = Directory.GetCurrentDirectory();
                    break;
                }
                
                // Change the directory
                Directory.SetCurrentDirectory(args[1]);
                _currDir = Directory.GetCurrentDirectory();
                break;
            case "exit":
                Environment.Exit(0);
                break;
            case "help":
                string help = @"
Kien's Shell, written in C#.
Type program names and arguments, and hit <enter>.
These shell commands are defined internally.  Type `help' to see this list.

cd
help";
                Console.WriteLine(help);
                break;
            case "#":
                // Handle the comment case
                break;
            default:
                // Execute command
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = args[0],
                    Arguments = Strings.Join(args.Skip(1).ToArray()),
                    RedirectStandardOutput = true,
                };

                Process proc = new Process() { StartInfo = startInfo };
                
                // Clear the standard output
                proc.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                break;
        }
    }
}