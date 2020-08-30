
(* 
 * Based on Server/Environment.fs from https://github.com/Zaid-Ajaj/tabula-rasa
 *)
module Environment

open System.IO

let (</>) x y = Path.Combine(x, y)

/// The path of the directory that holds the data of the application such as the database file, the config files and files concerning security keys.
let dataFolder =
    let appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
    let folder = appDataFolder </> "safe-todo"
    let directoryInfo = DirectoryInfo(folder)
    if not directoryInfo.Exists then Directory.CreateDirectory folder |> ignore
    printfn "Using data folder: %s" folder
    folder

/// The path of database file
let databaseFilePath = dataFolder </> "Todo.db"