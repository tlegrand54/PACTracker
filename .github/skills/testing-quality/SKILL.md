---
name: testing-quality
description: Use when implementing or reviewing code changes in this repository; keep changes focused and validate behavior with the existing .NET tests.
---

# Testing and Code Quality

- Follow the repository's existing architecture, naming, formatting, and language conventions; keep code, comments, XML documentation, and log or exception messages in English.
- Make the smallest change that fully addresses the request. Avoid unrelated refactors and preserve existing behavior outside the requested change.
- Add focused tests for new or changed behavior, including relevant edge cases. Prefer the existing MSTest patterns in `PACTracker.Tests`; do not add tests that only restate implementation details.
- Run the narrowest applicable test command, normally `dotnet test PACTracker.Tests/PACTracker.Tests.csproj`. Use the solution-level tests when a change affects multiple projects or shared build configuration.
- Report the exact validation performed and any failures; never imply tests passed when they were not run or did not pass.