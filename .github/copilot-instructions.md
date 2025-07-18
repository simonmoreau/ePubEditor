---
description: 'Guidelines for building and maintaining C# console applications with .NET 8 and Azure OpenAI integration'
applyTo: '**/*.cs'
---

# C# Development

## C# Instructions
- Always use the latest C# features (currently C# 13).
- Write clear, concise and expressive names for  function and class, including their purpose and usage.
- Don't write comments, except for all public APIs.
- Use XML doc comments for all public APIs, including `<example>` and `<code>` tags where applicable.
- Always use the type insted of `var`.

## General Instructions
- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on design decisions.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.
- When the project structure or practices change, update this file to keep Copilot suggestions relevant.

## Project Structure

- The `ePubEditor.Core` project is a .NET 8 library
- Its main entry point is `Main.cs`.
- Services and dependency injection helpers are in `Services/`
- Dependency injection is set up in `Main.cs` using `Microsoft.Extensions.DependencyInjection`.
- Azure OpenAI integration is configured in `Main.cs` and settings files.
- Use feature folders for new features to keep code organized by domain or functionality.

## Naming Conventions

- PascalCase for component names, method names, and public members.
- camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., `IUserService`).
- Prefix private field names with "_" (e.g., `_myField`).

## Formatting

- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Favor returning early when checking inputs or preconditions, instead of using nested if statements or loops.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests for methods and services.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints and services.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for UI responsiveness.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark application performance.

---

> **Note:**  
> As the project evolves, update this file with new instructions, patterns, or architectural decisions to ensure Copilot remains helpful and aligned with current best practices.  
> See: https://docs.github.com/en/copilot/customizing-copilot/adding-repository-custom-instructions-for-github-copilot
