namespace Aver.Test

module Test =
    open Xunit
    open Swensen.Unquote
    open Aver

    module TestData =

        let [<Literal>] ``aver.test.dll`` = "aver.test.dll"

        let mkAverTestArgs args =
            Array.append [|``aver.test.dll``|] args

        let all =
            Map.empty
                .Add(AssemblyProperties.FullName, "aver.test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                .Add(AssemblyProperties.TargetFramework, "v4.0.30319")
                .Add(AssemblyProperties.CompanyName, "mocosha Inc.")
                .Add(AssemblyProperties.AssemblyVersion, "1.0.0.0")
                .Add(AssemblyProperties.ProductVersion, "1.0.0.0")
                .Add(AssemblyProperties.FileVersion, "1.0.0.0")

        let perParam =
            dict
                ["--a", (AssemblyProperties.AssemblyVersion, all.[AssemblyProperties.AssemblyVersion])
                 "--p", (AssemblyProperties.ProductVersion,  all.[AssemblyProperties.ProductVersion])
                 "--f", (AssemblyProperties.FileVersion,     all.[AssemblyProperties.FileVersion])]

        let mkMap props =
            Seq.fold (fun (s: Map<_,_>) x -> s.Add x) Map.empty props

    let private assertIsError = function
        | Ok _ -> Assert.True(false, "Expected Error, got Ok")
        | Error _ -> ()


    [<Fact>]
    let ``no args should read AssemblyVersion only`` () =
        let options = Args.parse (TestData.mkAverTestArgs [||])

        let expected = Ok (TestData.mkMap [TestData.perParam.["--a"]])

        let result = Program.readAssemblyInfo options

        result =! expected

    [<Theory>]
    [<InlineData("--a")>]
    [<InlineData("--p")>]
    [<InlineData("--f")>]
    let ``one arg should read 1 properties`` (arg1) =
        let args = [|arg1|]
        let options = Args.parse (TestData.mkAverTestArgs args)

        let expected =
            args
            |> Seq.map (fun x -> TestData.perParam.[x])
            |> TestData.mkMap

        let result = Program.readAssemblyInfo options

        result =! Ok expected

    [<Theory>]
    [<InlineData("--a", "--p")>]
    [<InlineData("--a", "--f")>]
    [<InlineData("--p", "--f")>]
    let ``two args should read 2 properties`` (arg1, arg2) =
        let args = [|arg1; arg2|]
        let options = Args.parse (TestData.mkAverTestArgs args)

        let expected =
            args
            |> Seq.map (fun x -> TestData.perParam.[x])
            |> TestData.mkMap

        let result = Program.readAssemblyInfo options

        result =! Ok expected

    [<Theory>]
    [<InlineData("--a", "--p", "--f")>]
    let ``three args should read 3 properties`` (arg1, arg2, arg3) =
        let args = [|arg1; arg2; arg3|]
        let options = Args.parse (TestData.mkAverTestArgs args)

        let expected =
            args
            |> Seq.map (fun x -> TestData.perParam.[x])
            |> TestData.mkMap

        let result = Program.readAssemblyInfo options

        result =! Ok expected

    [<Fact>]
    let ``'--all' should read whole info`` () =
        let options = Args.parse (TestData.mkAverTestArgs [|"--all"|])

        let expected = Ok TestData.all

        let result = Program.readAssemblyInfo options

        result =! expected

    [<Theory>]
    [<InlineData("--a", "--p", "--f", "--all")>]
    let ``'--all' with other args should read whole info`` (arg1, arg2, arg3, arg4) =
        let options = Args.parse (TestData.mkAverTestArgs [|arg1; arg2; arg3; arg4|])

        let expected = Ok TestData.all

        let result = Program.readAssemblyInfo options

        result =! expected

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("asdfgh")>]
    [<InlineData("aver.test.pdb")>]
    let ``bad file should cause error error`` (path) =
        let options = Args.parse [|path|]

        let result = Program.readAssemblyInfo options

        assertIsError result

    [<Theory>]
    [<InlineData("--asdadf")>]
    [<InlineData("-a")>]
    [<InlineData("ssdaa")>]
    let ``invalid argument should raise exception`` (arg1) =
        Assert.Throws<_> (fun _ ->
            Args.parse (TestData.mkAverTestArgs [|arg1|]) |> ignore
        )