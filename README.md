![Nuget](https://img.shields.io/nuget/v/Toendering.RoslynAnalyzer.Akka)

# Roslyn Analyzer for Akka.NET
Roslyn analyzer for C# that flags usage of `Akka.Props<T>(...)` as an error if there is a mismatch between the target type `T` and the passed parameters.

## Install
The analyzer can be installed through [NuGet](https://www.nuget.org/packages/Toendering.RoslynAnalyzer.Akka) or installed as a Visual Studio Extension.

### Nuget (cli)
```
dotnet add package Toendering.RoslynAnalyzer.Akka
```

### Vistual Studio Extension
The analyzer can also be installed as a Visual Studio extension but is currently not available in the Visual Studio Marketplace so you will have to download and build it yourself to use it.

## Known Issues

* Does not take optional constructor parameters into account 
