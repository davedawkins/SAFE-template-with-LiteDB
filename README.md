# SAFE Template with LiteDB

This template was created with `dotnet new safe`, and then I've replaced the in-memory storage with storage to LiteDB using [LiteDB.FSharp](https://github.com/Zaid-Ajaj/LiteDB.FSharp).

## Main Changes

- Add `[<CliMutable>]` to `type Todo`. This attribute effectively makes the type serializable (adds default ctor, getters and setters).

```fs
[<CLIMutable>]
type Todo =
    { Id : Guid
      Description : string }
```

- Add `LiteDB.FSharp` to `paket.references
- Add `nuget LiteDB.FSharp` to `paket.dependencies`

- Open `LiteDB.FSharp` in `Server.cs`

```fs
open LiteDB.FSharp
open LiteDB
```

- Instantiate `LiteDB` with function `createDatabase`

```fs
let createDatabase =
    let mapper = FSharpBsonMapper()
    let dbFile = Environment.databaseFilePath
    let connStr = sprintf "Filename=%s;mode=Exclusive" dbFile
    new LiteDatabase( connStr, mapper )
```

- Modify `Storage` to take `db` as constructor argument, and this is for insertions and queries

```fs
type Storage (db : LiteDatabase) =
    let todos = db.GetCollection<Todo> "todos"

    member __.GetTodos () =
        todos.FindAll() |> List.ofSeq

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Insert(todo) |> ignore
            Ok ()
        else Error "Invalid todo"

let storage = Storage(createDatabase)
```

- Implement `Environment.databaseFilePath` based on code from []()

```fs
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
```

- Initialize the database first time only
```fs
if List.isEmpty (storage.GetTodos()) then
    storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
    storage.AddTodo(Todo.create "Write your app") |> ignore
    storage.AddTodo(Todo.create "Ship it !!!") |> ignore
```


This is what I consider to be the bare minimum to bring `LiteDB` into the template.

I'd like to extend this additional functionality (e.g., delete records, login screen). 

Original template documentation follows:

----
This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites
You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download) 3.1 or higher.
* [npm](https://nodejs.org/en/download/) package manager.
* [Node LTS](https://nodejs.org/en/download/).

## Starting the application
Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet fake build -t run
```

Then open `http://localhost:8080` in your browser.

To run concurrently server and client tests in watch mode (run in a new terminal):

```bash
dotnet fake build -t runtests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

## SAFE Stack Documentation
If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
