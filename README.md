# KShell - A simple shell in C#

KShell is a simle implementation of a shell in C#. It demonstrates the basics of how a shell works. Since its purpose demonstration (not feature completeness or even fitness for causual use), it has many limitations, including:

- Commands must be on a single line.
- Arguments must be separated by whitespace.
- No quoting arguments or escaping whitespace.
- No piping or redirection.
- Only built-ins are: `cd`, `help`, `exit`, `which`.
