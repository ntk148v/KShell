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
            // Shell prefix
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
                builtInCD(args);
                break;
            case "exit":
                builtInExit(0);
                break;
            case "help":
                builtInHelp(args);
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

    /// <summary>
    /// Built-in cd - Change the shell working directory.
    /// Change the current directory to DIR. The default DIR is the value of the
    /// HOME shell variable.
    /// cd [dir]
    /// </summary>
    /// <param name="args"></param>
    static void builtInCD(string[] args)
    {
        if (args.Length < 2)
        {
            // cd to home with empty path
            Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            _currDir = Directory.GetCurrentDirectory();
            return;
        }

        // Change the directory
        Directory.SetCurrentDirectory(args[1]);
        _currDir = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Built-in exit - Exit the shell with a status of n. If n is omitted,
    /// the exit status is that of the last command executed.
    /// exit [n]
    /// </summary>
    /// <param name="exitCode"></param>
    static void builtInExit(int exitCode)
    {
        // TODO(kiennt26): Handle the given exit code, it should be in range 0-255
        Environment.Exit(exitCode);
    }

    static void builtInHelp(string[] args)
    {
        string help;
        if (args.Length < 2)
        {
            help = @"
KShell aka. Kien's Shell, written in C#.
    
    Type program names and arguments, and hit <enter>.
    These shell commands are defined internally.  Type `help` to see this list.
    Type `help name` to find out more about the function `name'.

    cd [dir]
    exit [n]
    help";
        }
        else
        {
            switch (args[1])
            {
                case "cd":
                    help = @"
cd: cd [dir]

    Change the shell working directory.

    Change the current directory to 'dir'. The default 'dir' is the value of the user's home directory.";
                    break;
                case "exit":
                    help = @"
exit: exit [n]

    Exit the shell.

    Exits the shell with a status of 'n'.";
                    break;
                default:
                    help = @"
    KShell aka. Kien's Shell, written in C#.
    Type program names and arguments, and hit <enter>.
    These shell commands are defined internally.  Type `help` to see this list.

    cd [dir]
    exit [n]
    help";
                    break;
            }
        }

        Console.WriteLine(help);
    }
}