#r "paket: groupref FakeBuild //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Core.TargetOperators

let [<Literal>] ProjectFilePath = "src/aver/aver.fsproj"
let [<Literal>] TestProjectPath = "test/aver.test/aver.test.fsproj"
let [<Literal>] ProjectDirPath = "src/aver/"
let [<Literal>] TestProjectDirPath = "test/aver.test/"

Target.create "Clean" (fun _ ->
    Shell.deleteDirs  [ ProjectDirPath + "/bin"; ProjectDirPath + "/obj";
                        TestProjectDirPath + "/bin"; TestProjectDirPath + "/obj"]
)

Target.create "Restore" <| fun _ ->
    let setParams (defaults: DotNet.RestoreOptions) =
        { defaults with
            NoCache = true }

    DotNet.restore setParams "aver.sln"

Target.create "Build" <| fun _ ->
    let setParams (defaults: DotNet.BuildOptions) =
        { defaults with
           Configuration = DotNet.BuildConfiguration.Release
           OutputPath = Some "bin/Release" }

    DotNet.build setParams ProjectFilePath

Target.create "Test" <| fun _ ->
    let setParams (defaults: DotNet.TestOptions) =
        { defaults with
            NoRestore = true }

    DotNet.test setParams TestProjectPath

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"

Target.runOrDefault "Test"
