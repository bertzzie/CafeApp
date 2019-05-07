module PrepareFoodTests

open Domain
open States
open Events
open Errors
open Commands
open TestData
open CafeAppTestsDSL

open NUnit.Framework

[<Test>]
let ``Can prepare food``() =
    let order = { order with Foods = [salad] }
    let expected = {
        PlacedOrder = order
        ServedDrinks = []
        PreparedFoods = [salad]
        ServedFoods = []
    }
    
    Given (PlacedOrder order)
    |> When (PrepareFood (salad, order.Tab.Id))
    |> ThenStateShouldBe (OrderInProgress expected)
    |> WithEvents [FoodPrepared (salad, order.Tab.Id)]

[<Test>]
let ``Can not prepare a non-ordered food``() =
    let order = { order with Foods = [pizza] }
    Given (PlacedOrder order)
    |> When (PrepareFood (salad, order.Tab.Id))
    |> ShouldFailWith (CanNotPrepareNonOrderedFood salad)
    
[<Test>]
let ``Can not prepare a food for served order``() =
    Given (ServedOrder order)
    |> When (PrepareFood (pizza, order.Tab.Id))
    |> ShouldFailWith OrderAlreadyServed
    
[<Test>]
let ``Can not prepare with closed tab``() =
    Given (ClosedTab None)
    |> When (PrepareFood (salad, order.Tab.Id))
    |> ShouldFailWith CanNotPrepareWithClosedTab
