module PlaceOrderTests

open CafeAppTestsDSL
open Commands
open Domain
open States
open Events
open Errors

open System
open NUnit.Framework

let tab = { Id = Guid.NewGuid(); TableNumber = 1 }
let order = { Tab = tab; Foods = []; Drinks = [] }
let coke = Drink {
    MenuNumber = 1
    Name = "Coke"
    Price = 1.5m
}

[<Test>]
let ``Can place only drinks order``() =
    let testOrder = { order with Drinks = [coke] }
    
    Given (OpenedTab tab)
    |> When (PlaceOrder testOrder)
    |> ThenStateShouldBe (PlacedOrder testOrder)
    |> WithEvents [OrderPlaced testOrder]
    
[<Test>]
let ``Cannot place empty order``() =
    Given (OpenedTab tab)
    |> When (PlaceOrder order)
    |> ShouldFailWith CanNotPlaceEmptyOrder
    
[<Test>]
let ``Cannot place order with closed tab``() =
    let testOrder = { order with Drinks = [coke] }
    Given (ClosedTab None)
    |> When (PlaceOrder testOrder)
    |> ShouldFailWith CanNotOrderWithClosedTab
    
[<Test>]
let ``Cannot place order multiple times``() =
    let testOrder = { order with Drinks = [coke] }
    Given (PlacedOrder testOrder)
    |> When (PlaceOrder testOrder)
    |> ShouldFailWith OrderAlreadyPlaced
