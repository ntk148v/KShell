using System.Diagnostics;

namespace KShell;

class KShell
{
    // Test it in Linux only
    private static string _currUser = Environment.UserName;
    private static string _currDir = Directory.GetCurrentDirectory();
    private static string _hostname = Environment.MachineName;
    private static string[] _path = Environment.GetEnvironmentVariable("PATH").Split(":");

    static void Main(string[] args)
    {
        // Load config files, if any.

        // Run the input loop
        while (true)
        {
            try
            {
                Console.Write($"{_currUser}@{_hostname}:{_currDir}$ ");

                // Read the keyboard input
                string? input = Console.ReadLine();
                if (String.IsNullOrEmpty(input))
                    continue;

                execCommand(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // 0 - Success
                // 1 - Fail
                builtInExit(1);
            }
        }
    }

    static void execCommand(string input)
    {
        // Split the input separate the command and the arguments
        string[] args = input.TrimEnd().Split(" ");

        // Check for the built-in shell commands
        switch (args[0])
        {
            case "cd":
                builtInCD(args);
                break;
            case "exit":
                builtInExit(0);
                break;
            case "which":
                builtInWhich(args);
                break;
            case "help":
                builtInHelp(args);
                break;
            case "#":
                // Handle the comment case
                break;
            default:
                // Check if args[0] is an executable file
                if (searchInPath(args[0]).Count < 1)
                {
                    throw new Exception($"{args[0]}: command not found");
                }

                // Execute command
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = args[0],
                    Arguments = string.Join("", args.Skip(1).ToArray()),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                Process proc = new Process() { StartInfo = startInfo };

                // Clear the standard output
                // NOTE(kiennt26): This is a trick to remove the prefix from the command output
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
        string newWorkingDir;
        if (args.Length < 2)
        {
            // cd to home with empty path
            newWorkingDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (args[1] == "~") // handle a special character
        {
            newWorkingDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else
        {
            newWorkingDir = args[1];
        }

        // Change the directory
        Directory.SetCurrentDirectory(newWorkingDir);
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
    which filename ...
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
                case "which":
                    help = @"
which: which filename ...

    Locate a command.

    which returns the pathnames of the files (or links) which would be executed in the current environment.
    It does this by searching the PATH for executable files matching the names of the arguments.
";
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

    /// <summary>
    /// Built-in which - locate a command
    /// which returns the pathnames of the files (or links) which would be executed in the current environment.
    /// It does this by searching the PATH for executable files matching the file names of the arguments.
    /// </summary>
    /// <param name="args"></param>
    static void builtInWhich(string[] args)
    {
        if (args.Length < 2)
            return;
        foreach (string executable in args.Skip(1).ToArray())
        {
            foreach (string p in searchInPath(executable))
                Console.WriteLine(p);
        }
    }

    static List<string> searchInPath(string executable)
    {
        List<string> pathNames = new List<string>();
        // string[] pathNames = new string[0];
        foreach (string p in _path)
            pathNames.AddRange(Directory.GetFiles(p, executable));

        return pathNames;
    }
}