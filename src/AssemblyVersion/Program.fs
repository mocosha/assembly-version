namespace AssemblyVersion

module AssemblyProperties =
    let [<Literal>] FullName = "FullName"
    let [<Literal>] TargetFramework = "TargetFramework"
    let [<Literal>] CompanyName = "CompanyName"
    let [<Literal>] AssemblyVersion = "AssemblyVersion"
    let [<Literal>] ProductVersion = "ProductVersion"
    let [<Literal>] FileVersion = "FileVersion"

    let default' = [AssemblyVersion]

    let all =
        [FullName
         TargetFramework
         CompanyName
         AssemblyVersion
         ProductVersion
         FileVersion]

module Args =
    open Argu

    type Options =
        { AssemblyPath: string
          AssemblyProperties: string list }

    [<CliPrefix(CliPrefix.DoubleDash)>]
    type private CLIArguments =
        | [<MainCommand; Unique>] Path of string
        | [<AltCommandLine("--a")>] Assembly
        | [<AltCommandLine("--p")>] Product
        | [<AltCommandLine("--f")>] File
        | All
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Path _ -> "Path to the assembly (by default only assembly version is printed)"
                    | Assembly _ -> "Print assembly version"
                    | Product _ -> "Print product version"
                    | File _ -> "Print file version"
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
            @ getProps AssemblyProperties.all (results.TryGetResult All)
            |> function
                | [] -> AssemblyProperties.default'
                | x -> List.distinct x

        { AssemblyPath = results.GetResult Path
          AssemblyProperties = details }

open System
open System.Reflection
open System.Diagnostics

module Program =

    let readAssemblyInfo (options: Args.Options) =
        let logAndReturnError (ex: Exception) msg =
            Debug.WriteLine (ex.ToString ())
            Error (Option.defaultValue ex.Message msg)

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
                |> Map.filter (fun k _ -> List.contains k options.AssemblyProperties)

            Ok map
        with
        | :? System.IO.FileNotFoundException as ex ->
            logAndReturnError ex None
        | :? System.IO.FileLoadException as ex ->
            let msg = Some "A file that was found could not be loaded."
            logAndReturnError ex msg
        | :? BadImageFormatException as ex ->
            logAndReturnError ex None
        | :? System.Security.SecurityException as ex ->
            logAndReturnError ex None
        | :? System.IO.PathTooLongException as ex ->
            logAndReturnError ex None

    let printProperties =
        Map.iter (fun key value -> printfn "%s: %s" key value)

    [<EntryPoint>]
    let main argv =

        let options = Args.parse argv

        match readAssemblyInfo options with
        | Ok props -> printProperties props
        | Error err -> printfn "Error: %s" err

        0
