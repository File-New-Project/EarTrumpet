# Focused app regression tests

Run on Windows with the repository's .NET SDK:

```powershell
dotnet run --project EarTrumpet.Tests/EarTrumpet.Tests.csproj -c Release -p:Platform=x64
```

This dependency-free runner exercises the production assembly with in-memory audio streams and process snapshots. A failure returns a nonzero exit code. It does not register shortcuts, change audio devices, or start the EarTrumpet application.

The project reference uses EarTrumpet's normal build, which updates the package manifest version. That generated version change should not be committed with source changes.
