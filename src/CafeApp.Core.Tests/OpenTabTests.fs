module OpenTabTests

open CafeAppTestsDSL
open Commands
open Domain
open Events
open States
open Errors

open System
open NUnit.Framework

[<Test>]
let ``Can Open a new Tab``() =
    let tab = { Id = Guid.NewGuid(); TableNumber = 1 }
    
    Given (ClosedTab None)
    |> When (OpenTab tab)
    |> ThenStateShouldBe (OpenedTab tab)
    |> WithEvents [TabOpened tab]
    
[<Test>]
let ``Cannot open an already opened tab``() =
    let tab = { Id = Guid.NewGuid(); TableNumber = 1 }
    
    Given (OpenedTab tab)
    |> When (OpenTab tab)
    |> ShouldFailWith TabAlreadyOpened

