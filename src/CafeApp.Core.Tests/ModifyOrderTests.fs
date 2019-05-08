module ModifyOrderTests

open Domain
open Errors
open Events
open States
open Commands
open TestData
open CafeAppTestsDSL

open NUnit.Framework

[<Test>]
let ``Can add food when placing order``() =
    let order = { order with Foods = [pizza] }
    let addSalad = AddFood salad
    let expected = PlacedOrder { order with Foods = [salad; pizza] }
    
    Given (PlacedOrder order)
    |> When (ModifyOrder (order, addSalad))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified addSalad]
    
[<Test>]
let ``Can add food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let addSalad = AddFood salad
    let expected = OrderInProgress {
        orderInProgress with PlacedOrder = { order with Foods = [salad; pizza] }
    }
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, addSalad))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified addSalad]
    
[<Test>]
let ``Can not add already served food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = [salad]
        ServedDrinks = []
        PreparedFoods = []
    }
    let addSalad = AddFood salad
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, addSalad))
    |> ShouldFailWith (CanNotModifyAlreadyServedFood salad)
    
[<Test>]
let ``Can not add already prepared food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [salad]
    }
    let addSalad = AddFood salad
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, addSalad))
    |> ShouldFailWith (CanNotModifyAlreadyPreparedFood salad)
    
[<Test>]
let ``Can remove food when placing order``() =
    let order = { order with Foods = [pizza; salad] }
    let removeSalad = RemoveFood salad
    let expected = PlacedOrder { order with Foods = [pizza] }
    
    Given (PlacedOrder order)
    |> When (ModifyOrder (order, removeSalad))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified removeSalad]
    
[<Test>]
let ``Can not remove non-ordered food when placing order``() =
    let order = { order with Foods = [pizza ] }
    let removeSalad = RemoveFood salad
    
    Given (PlacedOrder order)
    |> When (ModifyOrder (order, removeSalad))
    |> ShouldFailWith (CanNotModifyNonOrderedFood salad)
    
[<Test>]
let ``Can remove food for in progress order``() =
    let order = { order with Foods = [pizza; salad] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let removeSalad = RemoveFood salad
    let expected = OrderInProgress {
        orderInProgress with PlacedOrder = { order with Foods = [pizza] }
    }
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeSalad))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified removeSalad]
    
[<Test>]
let ``Can not remove non-ordered food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let removeSalad = RemoveFood salad
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeSalad))
    |> ShouldFailWith (CanNotModifyNonOrderedFood salad)
    
[<Test>]
let ``Can not remove already served food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = [salad]
        ServedDrinks = []
        PreparedFoods = []
    }
    let removeSalad = RemoveFood salad
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeSalad))
    |> ShouldFailWith (CanNotModifyAlreadyServedFood salad)
    
[<Test>]
let ``Can not remove already prepared food for in progress order``() =
    let order = { order with Foods = [pizza] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = [salad]
    }
    let removeSalad = RemoveFood salad
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeSalad))
    |> ShouldFailWith (CanNotModifyAlreadyPreparedFood salad)

[<Test>]
let ``Can add drink when placing order``() =
    let order = { order with Drinks = [lemonade] }
    let addJuice = AddDrink appleJuice
    let expected = PlacedOrder { order with Drinks = [appleJuice; lemonade] }
    
    Given (PlacedOrder order)
    |> When (ModifyOrder (order, addJuice))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified addJuice]

[<Test>]
let ``Can add drink for in progress order``() =
    let order = { order with Drinks = [lemonade] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let addCoke = AddDrink coke
    let expected = OrderInProgress {
        orderInProgress with PlacedOrder = { order with Drinks = [coke; lemonade] }
    }
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, addCoke))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified addCoke]
    
[<Test>]
let ``Can not add already served drink for in progress order``() =
    let order = { order with Drinks = [lemonade] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = [coke]
        PreparedFoods = []
    }
    let addCoke = AddDrink coke
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, addCoke))
    |> ShouldFailWith (CanNotModifyAlreadyServedDrink coke)
    
[<Test>]
let ``Can remove drink when placing order``() =
    let order = { order with Drinks = [lemonade; coke; appleJuice] }
    let removeCoke = RemoveDrink coke
    let expected = PlacedOrder { order with Drinks = [lemonade; appleJuice] }
    
    Given (PlacedOrder order)
    |> When (ModifyOrder (order, removeCoke))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified removeCoke]
    
[<Test>]
let ``Can remove drink for in progress order``() =
    let order = { order with Drinks = [coke; lemonade; appleJuice] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let removeCoke = RemoveDrink coke
    let expected = OrderInProgress {
        orderInProgress with PlacedOrder = { order with Drinks = [lemonade; appleJuice] }
    }
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeCoke))
    |> ThenStateShouldBe expected
    |> WithEvents [OrderModified removeCoke]
    
[<Test>]
let ``Can not remove non-ordered drink for in progress order``() =
    let order = { order with Drinks = [lemonade] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = []
        PreparedFoods = []
    }
    let removeCoke = RemoveDrink coke
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeCoke))
    |> ShouldFailWith (CanNotModifyNonOrderedDrink coke)
    
[<Test>]
let ``Can not remove already served drink for in progress order``() =
    let order = { order with Drinks = [appleJuice] }
    let orderInProgress = {
        PlacedOrder = order
        ServedFoods = []
        ServedDrinks = [coke]
        PreparedFoods = []
    }
    let removeCoke = RemoveDrink coke
    
    Given (OrderInProgress orderInProgress)
    |> When (ModifyOrder (order, removeCoke))
    |> ShouldFailWith (CanNotModifyAlreadyServedDrink coke)
    
[<Test>]
let ``Must not modify already served order``() =
    let order = { order with Foods = [pizza]; Drinks = [coke] }
    
    Given (ServedOrder order)
    |> When (ModifyOrder (order, AddDrink lemonade))
    |> ShouldFailWith OrderAlreadyServed
    
[<Test>]
let ``Can not modify opened tab``() =
    let tab = order.Tab
    
    Given (OpenedTab tab)
    |> When (ModifyOrder (order, AddDrink lemonade))
    |> ShouldFailWith CanNotModifyNonPlacedOrder
    
[<Test>]
let ``Can not modify closed tab``() =
    Given (ClosedTab None)
    |> When (ModifyOrder (order, AddFood pizza))
    |> ShouldFailWith CanNotModifyClosedTab
    
