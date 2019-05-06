#r "paket: groupref netcorebuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.Testing

let buildDir = "./build/"
let testDir  = "./tests/"

let buildParams (defaults: MSBuildParams) =
    { defaults with
          DoRestore = true
    }

Target.create "Clean" (fun _ -> Shell.cleanDirs [buildDir; testDir])
Target.create "BuildApp" (fun _ ->
    !! "src/**/*.fsproj"
    -- "src/**/*.Tests.fsproj"
    |> MSBuild.runRelease buildParams buildDir "build"
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "BuildTests" (fun _ ->
    !! "src/**/*.Tests.fsproj"
    |> MSBuild.runDebug buildParams testDir "build"
    |> Trace.logItems "BuildTest-Output: "
)

Target.create "RunUnitTests" (fun _ ->
    !! (testDir + "*.Tests.dll")
    |> NUnit3.run (fun p -> { p with ShadowCopy = false })
)

"Clean" 
    ==> "BuildApp"
    ==> "BuildTests"
    ==> "RunUnitTests"

Target.runOrDefault "RunUnitTests"
