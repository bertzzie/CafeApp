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
    | OrderInProgress ipo, DrinkServed (item, _) ->
        { ipo with ServedDrinks = item :: ipo.ServedDrinks } |> OrderInProgress
    | OrderInProgress ipo, FoodPrepared (item, _) ->
        { ipo with PreparedFoods = item :: ipo.PreparedFoods } |> OrderInProgress
    | OrderInProgress ipo, FoodServed (item, _) ->
        { ipo with ServedFoods = item :: ipo.ServedFoods } |> OrderInProgress
    | OrderInProgress ipo, OrderServed (order, _) -> ServedOrder order
    | _ -> state