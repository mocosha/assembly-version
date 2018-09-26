namespace AssemblyVersion

open System
open System.Reflection
open System.Diagnostics

module Program =
    let printUsage () =
        printfn "%s" "Usage: version [path] [options]"
        printfn "%s" "Path:"
        printfn "%s" "\tThe path to the assembly. Required."
        printfn "%s" "Options:"
        printfn "%s" "\t Display different assembly information. Optional. Default is assembly version."
        printfn "%s" "\t --p | --product"
        printfn "%s" "\t --f | --file"
        printfn "%s" "\t --all"

    let parseArgs (argv: string []) =
        if argv.Length < 1 || argv.Length > 2 then
            Error "Invalid arguments."
        else
            let path = argv.[0]
            let option =
                if argv.Length = 2 && not (String.IsNullOrWhiteSpace argv.[1]) then 
                    Some argv.[1]
                else
                    None
            Ok (path, option)

    let readAssemblyInfo path =
        let logEndReturnError (ex: Exception) (msg: string option)=
            Debug.WriteLine (ex.GetType().FullName)
            Debug.WriteLine (ex.StackTrace)
            Error (if msg.IsSome then msg.Value else ex.Message)
        try
            let assembly = Assembly.LoadFrom (path)
            let versionInfo = FileVersionInfo.GetVersionInfo (assembly.Location)

            let map =
                Map.empty
                    .Add("FullName", assembly.FullName)
                    .Add("TargetFramework", assembly.ImageRuntimeVersion)
                    .Add("CompanyName", versionInfo.CompanyName)
                    .Add("AssemblyVersion", assembly.GetName().Version.ToString())
                    .Add("ProductVersion", versionInfo.ProductVersion)
                    .Add("FileVersion", versionInfo.FileVersion)
            Ok map
        with
            | :? System.IO.FileNotFoundException as ex ->
                logEndReturnError ex None
            | :? System.IO.FileLoadException as ex ->
                let msg = Some "A file that was found could not be loaded."
                logEndReturnError ex msg
            | :? BadImageFormatException as ex ->
                logEndReturnError ex None
            | :? System.Security.SecurityException as ex ->
                logEndReturnError ex None
            | :? System.IO.PathTooLongException as ex ->
                logEndReturnError ex None

    let printResult (option: string option) map =
        match option with
        | Some value ->
            match value with
            | "--all" ->
                Map.iter (fun key value -> printfn "%s: %s" key value) map
            | "--file"
            | "--f" ->
                printfn "FileVersion: %s" map.["FileVersion"]
            | "--product" | "--p" ->
                printfn "ProductVersion: %s" map.["ProductVersion"]
            | "--assembly" | "--a" | _ ->
                printfn "AssemblyVersion: %s" map.["AssemblyVersion"]
        | None ->
            printfn "AssemblyVersion: %s" map.["AssemblyVersion"]

    [<EntryPoint>]
    let main argv =
        match parseArgs argv with
        | Error msg ->
            printfn "%s" msg
            printUsage ()
            2 //for example invalid argv // TODO: check error codes
        | Ok (path, option) ->
            readAssemblyInfo path
            |> Result.bind (fun infos -> Ok (printResult option infos))
            |> Result.mapError (fun msg -> printfn "%s" msg)
            |> ignore
            0
