using System.Diagnostics;

namespace KShell;

// Test it in Linux only
class KShell
{
    // Current user
    private static string _currUser = Environment.UserName;
    // Current directory
    private static string _currDir = Directory.GetCurrentDirectory();
    // Previous directory, by default it is the same as current directory.
    private static string _prevDir = _currDir;
    // Home directory
    private static readonly string _homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    // Hostname - machine's hostname
    private static string _hostname = Environment.MachineName;
    // Path - Get $PATH environment variables, contains the list of directories of executable files.
    private static readonly string[] _path = Environment.GetEnvironmentVariable("PATH").Split(":");
    // A list of executed commands
    private static List<string> _commandHistory = new List<string>();
    // Current command index
    private static int _currentCommandIndex = -1;
    // The default file to store command history
    private static string _historyFilePath = Path.Combine(_homeDir, ".kshell_history");

    static void Main()
    {
        // Load config files, if any.

        // Load command history
        LoadCommandHistory();

        // Run the input loop
        while (true)
        {
            try
            {
                // 0 - Success
                // 1 - Fail
                Environment.ExitCode = 0;

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
                Environment.ExitCode = 1;
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
                BuiltInExit();
                break;
            case "which":
                BuiltInWhich(args);
                break;
            case "help":
                BuiltInHelp(args);
                break;
            case "history":
                BuiltInHistory();
                break;
            case "#":
                // Handle the comment case
                break;
            case "!!":
                ExecLastCommand();
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
                    UseShellExecute = false,
                };

                var proc = new Process() { StartInfo = startInfo };
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                Console.WriteLine(output);
                break;
        }

        // Add command to history
        _commandHistory.Add(input);
        _currentCommandIndex = _commandHistory.Count - 1;
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
            newWorkingDir = _homeDir;
        }
        else if (args[1] == "~") // handle a special character
        {
            newWorkingDir = _homeDir;
        }
        else if (args[1] == "-")
        {
            newWorkingDir = _prevDir;
        }
        else
        {
            newWorkingDir = args[1];
        }

        // Change the directory
        Directory.SetCurrentDirectory(newWorkingDir);
        _prevDir = _currDir;
        _currDir = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Built-in exit - Exit the shell with a status of n. If n is omitted,
    /// the exit status is that of the last command executed.
    /// exit [n]
    /// </summary>
    /// <param name="exitCode"></param>
    private static void BuiltInExit()
    {
        // Save command before exit
        SaveCommandHistory();

        // TODO(kiennt26): Handle the given exit code, it should be in range 0-255
        Environment.Exit(Environment.ExitCode);
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

    Change the current directory to 'dir'. The default 'dir' is the value of the user's home directory.
    Special cases:
    - `cd ~`: Change the current directory to $HOME.
    - `cd -`: Move back to the previous directory.
    - `cd`: Change current directory to $HOME.",
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
                "history" => @"
history: history

    Display the history list.

    Display the history list with line numbers
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

    /// <summary>
    /// Display history list
    /// </summary>
    private static void BuiltInHistory()
    {
        for (var i = 0; i < _commandHistory.Count; i++)
            Console.WriteLine($"{i + 1}: {_commandHistory[i]}");
    }

    /// <summary>
    /// Load command history from _historyFilePath
    /// </summary>
    private static void LoadCommandHistory()
    {
        if (File.Exists(_historyFilePath))
        {
            _commandHistory = File.ReadAllLines(_historyFilePath).ToList();
            _currentCommandIndex = _commandHistory.Count - 1;
        }
    }

    /// <summary>
    /// Save all commands in this session to file
    /// </summary>
    private static void SaveCommandHistory()
    {
        File.WriteAllLines(_historyFilePath, _commandHistory);
    }

    /// <summary>
    /// Execute the last command get from _commandHistory
    /// </summary>
    private static void ExecLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            var lastCommand = _commandHistory.Last();
            Console.WriteLine(lastCommand);
            ExecCommand(lastCommand);
        }
    }
}