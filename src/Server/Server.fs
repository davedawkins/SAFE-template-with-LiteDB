module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open LiteDB.FSharp
open LiteDB

open Shared

let createDatabase =
    let mapper = FSharpBsonMapper()
    let dbFile = Environment.databaseFilePath
    let connStr = sprintf "Filename=%s;mode=Exclusive" dbFile
    new LiteDatabase( connStr, mapper )

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

if List.isEmpty (storage.GetTodos()) then
    storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
    storage.AddTodo(Todo.create "Write your app") |> ignore
    storage.AddTodo(Todo.create "Ship it !!!") |> ignore

let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
