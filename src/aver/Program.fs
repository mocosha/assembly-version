namespace Aver

module AssemblyProperties =
    let [<Literal>] FullName = "FullName"
    let [<Literal>] TargetFramework = "TargetFramework"
    let [<Literal>] CompanyName = "CompanyName"
    let [<Literal>] AssemblyVersion = "AssemblyVersion"
    let [<Literal>] ProductVersion = "ProductVersion"
    let [<Literal>] FileVersion = "FileVersion"
    let [<Literal>] Filename = "Filename"

    let default' = [AssemblyVersion]

    let all =
        [FullName
         TargetFramework
         CompanyName
         AssemblyVersion
         ProductVersion
         FileVersion
         Filename]

module Args =
    open Argu

    type Options =
        { AssemblyPath: string
          AssemblyProperties: string list }

    [<CliPrefix(CliPrefix.DoubleDash)>]
    type private CLIArguments =
        | [<MainCommand; Unique>] Path of string
        | [<AltCommandLine("-a")>] Assembly
        | [<AltCommandLine("-p")>] Product
        | [<AltCommandLine("-f")>] File
        | [<AltCommandLine("-n")>] Filename
        | [<AltCommandLine("-A")>] All
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Path _ -> "Path to the assembly (by default only assembly version is printed)"
                    | Assembly _ -> "Print assembly version"
                    | Product _ -> "Print product version"
                    | File _ -> "Print file version"
                    | Filename _ -> "Print file name"
                    | All -> "Print whole assembly info"

    let parse cliargs =
        let errorHandler = ProcessExiter(colorizer= function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)
        let parser = ArgumentParser.Create<CLIArguments> (errorHandler=errorHandler)

        let results = parser.ParseCommandLine (cliargs)

        let getProps lst = function
            | Some _ -> lst
            | None -> []

        let details =
            getProps [AssemblyProperties.AssemblyVersion] (results.TryGetResult Assembly)
            @ getProps [AssemblyProperties.ProductVersion] (results.TryGetResult Product)
            @ getProps [AssemblyProperties.FileVersion] (results.TryGetResult File)
            @ getProps [AssemblyProperties.Filename] (results.TryGetResult Filename)
            @ getProps AssemblyProperties.all (results.TryGetResult All)
            |> function
                | [] -> AssemblyProperties.default'
                | x -> List.distinct x

        { AssemblyPath = results.GetResult Path
          AssemblyProperties = details }

module Program =
    open System.Reflection
    open System.Diagnostics

    let readAssemblyInfo (options: Args.Options) =
        try
            let assembly = Assembly.LoadFrom (options.AssemblyPath)
            let versionInfo = FileVersionInfo.GetVersionInfo (assembly.Location)

            let map =
                Map.empty
                    .Add(AssemblyProperties.FullName, assembly.FullName)
                    .Add(AssemblyProperties.TargetFramework, assembly.ImageRuntimeVersion)
                    .Add(AssemblyProperties.CompanyName, versionInfo.CompanyName)
                    .Add(AssemblyProperties.AssemblyVersion, assembly.GetName().Version.ToString())
                    .Add(AssemblyProperties.ProductVersion, versionInfo.ProductVersion)
                    .Add(AssemblyProperties.FileVersion, versionInfo.FileVersion)
                    .Add(AssemblyProperties.Filename, options.AssemblyPath)
                |> Map.filter (fun k _ -> List.contains k options.AssemblyProperties)

            Ok map
        with
        | ex ->
            Debug.WriteLine (ex.ToString ())
            Error ex.Message

    let printProperties =
        Map.iter (fun key value -> printfn "%s: %s" key value)

    [<EntryPoint>]
    let main argv =

        let options = Args.parse argv

        match readAssemblyInfo options with
        | Ok props -> printProperties props
        | Error err -> printfn "Error: %s" err

        0
