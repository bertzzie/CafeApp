module States

open Domain
open Events
open System

type State =
    | ClosedTab of Guid option
    | OpenedTab of Tab
    | PlacedOrder of Order
    | OrderInProgress of InProgressOrder
    | ServedOrder of Order
    
let private removeListItem items itemToBeRemoved =
    List.filter (fun i -> i <> itemToBeRemoved) items
    
let applyPlacedOrderModification order = function
| AddFood food      -> { order with Foods = food :: order.Foods }
| RemoveFood food   -> { order with Foods = removeListItem order.Foods food  }
| AddDrink drink    -> { order with Drinks = drink :: order.Drinks }
| RemoveDrink drink -> { order with Drinks = removeListItem order.Drinks drink }
    
let apply state event =
    match state, event with
    | ClosedTab _, TabOpened tab     -> OpenedTab tab
    | OpenedTab _, OrderPlaced order -> PlacedOrder order
    | PlacedOrder order, DrinkServed (item, _) ->
        {
            PlacedOrder = order
            ServedDrinks = [item]
            ServedFoods = []
            PreparedFoods = []
        } |> OrderInProgress
    | PlacedOrder order, FoodPrepared (food, _) ->
        {
            PlacedOrder = order
            PreparedFoods = [food]
            ServedDrinks = []
            ServedFoods = []
        } |> OrderInProgress
    | PlacedOrder order, OrderModified modification ->
        applyPlacedOrderModification order modification |> PlacedOrder
    | OrderInProgress ipo, DrinkServed (item, _) ->
        { ipo with ServedDrinks = item :: ipo.ServedDrinks } |> OrderInProgress
    | OrderInProgress ipo, FoodPrepared (item, _) ->
        { ipo with PreparedFoods = item :: ipo.PreparedFoods } |> OrderInProgress
    | OrderInProgress ipo, FoodServed (item, _) ->
        { ipo with ServedFoods = item :: ipo.ServedFoods } |> OrderInProgress
    | OrderInProgress _, OrderServed (order, _) -> ServedOrder order
    | OrderInProgress ipo, OrderModified modification ->
        { ipo with PlacedOrder = applyPlacedOrderModification ipo.PlacedOrder modification }
        |> OrderInProgress
    | ServedOrder _, TabClosed payment -> ClosedTab(Some payment.Tab.Id)
    | _ -> state
