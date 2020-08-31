# SAFE Template with LiteDB

This template was created with `dotnet new safe`, and then I've replaced the in-memory storage with `LiteDB` using [LiteDB.FSharp](https://github.com/Zaid-Ajaj/LiteDB.FSharp).

## Revisions

- Add suggestions from @IsaacAbraham
- Eliminate global instance of `Storage`

## Main Changes

- Add `[<CliMutable>]` to `type Todo`. This attribute effectively makes the type serializable (adds default ctor, getters and setters).

```fs
[<CLIMutable>]
type Todo =
    { Id : Guid
      Description : string }
```

- Add `LiteDB.FSharp` to `paket.references`
- Add `nuget LiteDB.FSharp` to `paket.dependencies`

- Open `LiteDB.FSharp` in `Server.cs`

```fs
open LiteDB.FSharp
open LiteDB
```

- Instantiate `LiteDB` with function `database`

```fs
let database dbName =
    let mapper = FSharpBsonMapper()
    let dbFile = sprintf "%s.db" dbName
    let connStr = sprintf "Filename=%s;mode=Exclusive" dbFile
    new LiteDatabase( connStr, mapper )
```

- Modify `Storage` to take `db` as constructor argument, used for insertions and queries
- Initialize database upon first creation

```fs
type Storage (db : LiteDatabase) as this =
    let collection = "todos"
    let todos = db.GetCollection<Todo> collection

    do 
        if not (db.CollectionExists collection) then
            this.AddTodo(Todo.create "Create new SAFE project") |> ignore
            this.AddTodo(Todo.create "Write your app") |> ignore
            this.AddTodo(Todo.create "Ship it !!!") |> ignore

    member __.GetTodos () =
        todos.FindAll() |> List.ofSeq

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Insert(todo) |> ignore
            Ok ()
        else Error "Invalid todo"
```

- API instance takes `Storage` as an argument
```
let todosApi (storage : Storage) =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }
```

- Pass `Storage` instance to API 
```
let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue (database "Todo" |> Storage |> todosApi)
    |> Remoting.buildHttpHandler
```
    
This is what I consider to be the bare minimum to bring `LiteDB` into the template idiomatically.

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
