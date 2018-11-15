#r "paket: groupref FakeBuild //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Core.TargetOperators

let [<Literal>] ProjectFilePath = "src/aver/aver.fsproj"
let [<Literal>] TestProjectFilePath = "test/aver.test/aver.test.fsproj"
let [<Literal>] ProjectDirPath = "src/aver/"
let [<Literal>] TestProjectDirPath = "test/aver.test/"
let [<Literal>] SolutonFile = "aver.sln"
let [<Literal>] OutputDir = "bin/Release"

Target.create "Clean" |> fun _ ->
    [ ProjectDirPath + "/bin"
      ProjectDirPath + "/obj"
      TestProjectDirPath + "/bin"
      TestProjectDirPath + "/obj" ]
    |> Shell.deleteDirs

Target.create "Restore" <| fun _ ->
    let setParams (defaults: DotNet.RestoreOptions) =
        { defaults with
            NoCache = true }

    DotNet.restore setParams SolutonFile

Target.create "Build" <| fun _ ->
    let setParams (defaults: DotNet.BuildOptions) =
        { defaults with
           Configuration = DotNet.BuildConfiguration.Release
           OutputPath = Some OutputDir }

    DotNet.build setParams ProjectFilePath

Target.create "Test" <| fun _ ->
    let setParams (defaults: DotNet.TestOptions) =
        { defaults with
            NoRestore = true }

    DotNet.test setParams TestProjectFilePath

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"

Target.runOrDefault "Test"