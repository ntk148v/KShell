# KShell - A simple shell in C#

## 1. Introduction

KShell is a simle implementation of a shell in C#. It demonstrates the basics of how a shell works. Since its purpose demonstration (not feature completeness or even fitness for causual use), it has many limitations, including:

- Tested only in Linux environment.
- Commands must be on a single line.
- Arguments must be separated by whitespace.
- No quoting arguments or escaping whitespace.
- No piping or redirection.
- Only built-ins are: `cd`, `help`, `exit`, `which`, `history`.

## 2. How to execute

## 3. Considered improvements

- Catch keyboard input: `Ctrl + q` - exit, `up/down`.
- Browse your input history with the up/down keys.

## 4. Contribution

Since this is my very first C# project (for education purpose), the source code isn't optimized. All contributions are welcome.

- If you find a bug, fire an issue and create a PR to solve it. Even if it's a typo fix, it is still welcomed.
- If you implement a new feature or you have an idea, create an issue.
