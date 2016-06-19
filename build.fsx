// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO
open System.CodeDom
open System.CodeDom.Compiler

#r "Microsoft.CSharp"
open Microsoft.CSharp
open Microsoft.CSharp.RuntimeBinder

Target "Travis" (fun _ ->
  printfn "Codepage: %d" Console.OutputEncoding.CodePage

  //try Console.OutputEncoding <- System.Text.Encoding.GetEncoding(1200) with _ -> ()
  //try Console.InputEncoding <- System.Text.Encoding.GetEncoding(1200) with _ -> ()
  //try Console.OutputEncoding <- System.Text.Encoding.GetEncoding(12000) with _ -> ()
  //try Console.InputEncoding <- System.Text.Encoding.GetEncoding(12000) with _ -> ()
  
  use prov = new CSharpCodeProvider()
  let source = @"
#warning Trigger Some Warning
namespace MyNamespace {
  public class MyClass {
    public static string MyMethod () { return ""data""; }
  }
}
"
  let tempDirectory = Path.Combine(Path.GetTempPath(), "RazorEngine_" + Path.GetRandomFileName())
  Directory.CreateDirectory(tempDirectory) |> ignore
  let p = 
    new CompilerParameters(
      GenerateInMemory = false,
      GenerateExecutable = false,
      IncludeDebugInformation = true,
      TreatWarningsAsErrors = false,
      TempFiles = new TempFileCollection(tempDirectory, true),
      CompilerOptions = String.Format("/target:library /optimize /define:RAZORENGINE {0}", ""))
  
  let tempDir = p.TempFiles.TempDir
  let assemblyName = Path.Combine(tempDir, String.Format("{0}.dll", "MyNamespace"))
  p.TempFiles.AddFile(assemblyName, true)
  let results = prov.CompileAssemblyFromSource(p, [| source |])
  if isNull results.Errors |> not && results.Errors.HasErrors then
    printfn "Results: %A" results
    for e in results.Errors do
      printfn " - %s: (%d, %d) %s" e.ErrorNumber e.Line e.Column e.ErrorText
      let enc = System.Text.Encoding.GetEncoding(1200)
      let b = enc.GetBytes(e.ErrorText)
      printfn "Decoded: %s" (System.Text.Encoding.UTF8.GetString b)

    printfn "Native return value: %d" results.NativeCompilerReturnValue
    for m in results.Output do
      printfn "Message: %s" m
    printfn "ResultAssembly: %s" results.PathToAssembly
    printfn "Exists: %s" (if File.Exists results.PathToAssembly then "true" else "false")
    failwith "Compilation failed"
  else 
    printfn "Success"
)

// start build
RunTargetOrDefault "Travis"