module CloseTabTests

open Domain
open Errors
open Events
open States
open Commands
open TestData
open CafeAppTestsDSL

open NUnit.Framework

[<Test>]
let ``Can close the tab by paying full amount``() =
    let order = { order with
                      Foods = [salad; pizza]
                      Drinks = [coke]
                }
    let payment = { Tab = order.Tab; Amount = (payment order).Amount }
    
    Given (ServedOrder order)
    |> When (CloseTab payment)
    |> ThenStateShouldBe (ClosedTab (Some order.Tab.Id))
    |> WithEvents [TabClosed payment]
    
[<Test>]
let ``Can not close tab with invalid payment amount``() =
    let order = { order with
                      Foods = [salad; pizza]
                      Drinks = [coke]
                }
    let realAmount = (payment order).Amount
    let invalidAmount = realAmount - 0.5m
    
    Given (ServedOrder order)
    |> When (CloseTab { Tab = order.Tab; Amount = invalidAmount })
    |> ShouldFailWith (InvalidPayment (realAmount, invalidAmount))
