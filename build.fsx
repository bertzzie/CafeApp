
#r "packages/FAKE/tools/FakeLib.dll"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.Testing

let buildDir = "./build/"
let testDir  = "./tests/"

Target.create "Clean" (fun _ -> Shell.cleanDirs [buildDir; testDir])
Target.create "BuildApp" (fun _ ->
    !! "src/**/*.fsproj"
    -- "src/**/*.Tests.fsproj"
    |> MSBuild.runRelease id buildDir "build"
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "BuildTests" (fun _ ->
    !! "src/**/*.Tests.fsproj"
    |> MSBuild.runDebug id testDir "build"
    |> Trace.logItems "BuildTest-Output: "
)
Target.create "RunUnitTests" (fun _ ->
    !! (testDir + "/*.Tests.dll")
    |> NUnit3.run (fun p -> { p with ShadowCopy = false })
)

"Clean" 
    ==> "BuildApp"
    ==> "BuildTests"
    ==> "RunUnitTests"

Target.runOrDefault "RunUnitTests"
