module CommandHandlers

open Domain
open Errors
open States
open Events
open Commands

open Chessie.ErrorHandling
open System

let handleOpenTab tab = function
| ClosedTab _ -> [TabOpened tab]  |> ok
| _           -> TabAlreadyOpened |> fail

let handlePlaceOrder (order: Order) = function
| OpenedTab _ ->
    if List.isEmpty order.Foods && List.isEmpty order.Drinks then
        fail CanNotPlaceEmptyOrder
    else
        [OrderPlaced order] |> ok
| ClosedTab _   -> fail CanNotOrderWithClosedTab
| PlacedOrder _ -> fail OrderAlreadyPlaced
| _             -> failwith "Todo"

let (|NonOrderedDrink|_|) order drink =
    match List.contains drink order.Drinks with
    | false -> Some drink
    | true  -> None

let (|ServeDrinkCompletesOrder|_|) order drink =
    match isServingDrinkCompletesOrder order drink with
    | true  -> Some drink
    | false -> None

let (|AlreadyServedDrink|_|) ipo drink =
    match List.contains drink ipo.ServedDrinks with
    | true  -> Some drink
    | false -> None

let (|NonOrderedFood|_|) order food =
    match List.contains food order.Foods with
    | false -> Some food
    | true -> None

let (|AlreadyPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | true  -> Some food
    | false -> None

let handleServedDrink drink tabId = function
| PlacedOrder order ->
    let event = DrinkServed (drink, tabId)
    match drink with
    | NonOrderedDrink order _ -> CanNotServeNonOrderedDrink drink |> fail
    | ServeDrinkCompletesOrder order _ ->
        let payment = { Tab = order.Tab; Amount = orderAmount order }
        event :: [OrderServed (order, payment)] |> ok
    | _ -> [event] |> ok
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let drinkServed = DrinkServed (drink, order.Tab.Id)
    match drink with
    | NonOrderedDrink order _  -> CanNotServeNonOrderedDrink drink |> fail
    | AlreadyServedDrink ipo _ -> CanNotServeAlreadyServedDrink drink |> fail
    | _                        -> [drinkServed] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _   -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _   -> CanNotServeWithClosedTab |> fail

let handlePrepareFood food tabId = function
| PlacedOrder order ->
    match food with
    | NonOrderedFood order _ -> CanNotPrepareNonOrderedFood food |> fail
    | _ -> [FoodPrepared (food, tabId)] |> ok
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let foodPrepared = FoodPrepared (food, order.Tab.Id)
    match food with
    | NonOrderedFood order _    -> CanNotPrepareNonOrderedFood food |> fail
    | AlreadyPreparedFood ipo _ -> CanNotPrepareAlreadyPreparedFood food |> fail
    | _                         -> [foodPrepared] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _   -> CanNotPrepareForNonPlacedOrder |> fail
| ClosedTab _   -> CanNotPrepareWithClosedTab |> fail

let execute state command =
    match command with
    | OpenTab tab               -> handleOpenTab tab state
    | PlaceOrder order          -> handlePlaceOrder order state
    | ServeDrink (drink, tabId) -> handleServedDrink drink tabId state
    | PrepareFood (food, tabId) -> handlePrepareFood food tabId state
    | _           -> failwith "Todo"
    
let evolve state command =
    match execute state command with
    | Ok (events, _) ->
        let newState = List.fold States.apply state events
        (newState, events) |> ok
        
    | Bad err -> Bad err
