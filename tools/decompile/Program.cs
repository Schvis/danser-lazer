using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

class Program
{
    static void Main()
    {
        var resolver = new UniversalAssemblyResolver(
            @"D:\so\danser-go\tools\lazer-export.dll",
            throwOnError: false,
            targetFramework: ".NETCoreApp,Version=v8.0");
        resolver.AddSearchDirectory(@"C:\Users\Schvis\AppData\Local\osulazer\current");
        resolver.AddSearchDirectory(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.30");

        var decompiler = new CSharpDecompiler(@"D:\so\danser-go\tools\lazer-export.dll", resolver, new DecompilerSettings());
        var code = decompiler.DecompileWholeModuleAsString();
        File.WriteAllText(@"D:\so\danser-go\tools\lazer-export_decompiled.cs", code);
        Console.WriteLine("Decompiled successfully! Length: " + code.Length);
    }
}
