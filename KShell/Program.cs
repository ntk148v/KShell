using System.Diagnostics;

namespace KShell;

class KShell
{
    // Test it in Linux only
    private static string _currUser = Environment.UserName;
    private static string _currDir = Directory.GetCurrentDirectory();
    private static string _hostname = Environment.MachineName;
    private static string[] _path = Environment.GetEnvironmentVariable("PATH").Split(":");

    static void Main()
    {
        // Load config files, if any.

        // Run the input loop
        while (true)
        {
            try
            {
                Console.Write($"{_currUser}@{_hostname}:{_currDir}$ ");

                // Read the keyboard input
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    continue;

                ExecCommand(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // 0 - Success
                // 1 - Fail
                BuiltInExit(1);
            }
        }
    }

    private static void ExecCommand(string input)
    {
        // Split the input separate the command and the arguments
        var args = input.TrimEnd().Split(" ");

        // Check for the built-in shell commands
        switch (args[0])
        {
            case "cd":
                BuiltInCD(args);
                break;
            case "exit":
                BuiltInExit(0);
                break;
            case "which":
                BuiltInWhich(args);
                break;
            case "help":
                BuiltInHelp(args);
                break;
            case "#":
                // Handle the comment case
                break;
            default:
                // Check if args[0] is an executable file
                if (SearchInPath(args[0]).Count < 1)
                {
                    throw new Exception($"{args[0]}: command not found");
                }

                // Execute command
                var startInfo = new ProcessStartInfo()
                {
                    FileName = args[0],
                    Arguments = string.Join("", args.Skip(1).ToArray()),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var proc = new Process() { StartInfo = startInfo };

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
    static void BuiltInCD(string[] args)
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
    private static void BuiltInExit(int exitCode)
    {
        // TODO(kiennt26): Handle the given exit code, it should be in range 0-255
        Environment.Exit(exitCode);
    }

    private static void BuiltInHelp(string[] args)
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
            help = args[1] switch
            {
                "cd" => @"
cd: cd [dir]

    Change the shell working directory.

    Change the current directory to 'dir'. The default 'dir' is the value of the user's home directory.",
                "exit" => @"
exit: exit [n]

    Exit the shell.

    Exits the shell with a status of 'n'.",
                "which" => @"
which: which filename ...

    Locate a command.

    which returns the path names of the files (or links) which would be executed in the current environment.
    It does this by searching the PATH for executable files matching the names of the arguments.
",
                _ => @"
KShell aka. Kien's Shell, written in C#.

    Type program names and arguments, and hit <enter>.
    These shell commands are defined internally.  Type `help` to see this list.

    cd [dir]
    exit [n]
    help"
            };
        }

        Console.WriteLine(help);
    }

    /// <summary>
    /// Built-in which - locate a command
    /// which returns the path names of the files (or links) which would be executed in the current environment.
    /// It does this by searching the PATH for executable files matching the file names of the arguments.
    /// </summary>
    /// <param name="args"></param>
    private static void BuiltInWhich(string[] args)
    {
        if (args.Length < 2)
            return;
        foreach (var executable in args.Skip(1).ToArray())
        {
            foreach (var p in SearchInPath(executable))
                Console.WriteLine(p);
        }
    }

    private static List<string> SearchInPath(string executable)
    {
        var pathNames = new List<string>();
        // string[] pathNames = new string[0];
        foreach (var p in _path)
            pathNames.AddRange(Directory.GetFiles(p, executable));

        return pathNames;
    }
}