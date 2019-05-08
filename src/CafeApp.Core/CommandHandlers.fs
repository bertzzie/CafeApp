module CommandHandlers

open Domain
open Errors
open States
open Events
open Commands

open Chessie.ErrorHandling

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

let (|ServeDrinkCompletesIPOrder|_|) ipo drink =
    match willServingDrinkCompletesIPOrder ipo drink with
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

let (|NonPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | false -> Some food
    | true  -> None

let (|AlreadyPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | true  -> Some food
    | false -> None
    
let (|AlreadyServedFood|_|) ipo food =
    match List.contains food ipo.ServedFoods with
    | true  -> Some food
    | false -> None

let (|ServeFoodCompletesIPOrder|_|) ipo food =
    match willServingFoodCompletesIPOrder ipo food with
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
    | NonOrderedDrink order _          -> CanNotServeNonOrderedDrink drink |> fail
    | AlreadyServedDrink ipo _         -> CanNotServeAlreadyServedDrink drink |> fail
    | ServeDrinkCompletesIPOrder ipo _ -> drinkServed :: [OrderServed(order, payment order)] |> ok
    | _                                -> [drinkServed] |> ok
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
    match food with
    | NonOrderedFood order _    -> CanNotPrepareNonOrderedFood food |> fail
    | AlreadyPreparedFood ipo _ -> CanNotPrepareAlreadyPreparedFood food |> fail
    | _                         -> [FoodPrepared (food, order.Tab.Id)] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _   -> CanNotPrepareForNonPlacedOrder |> fail
| ClosedTab _   -> CanNotPrepareWithClosedTab |> fail

let handleServedFood food tabId = function
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let servedFood = FoodServed(food, tabId)
    match food with
    | NonOrderedFood order _          -> CanNotServeNonOrderedFood food |> fail
    | AlreadyServedFood ipo _         -> CanNotServeAlreadyServedFood food |> fail
    | NonPreparedFood ipo _           -> CanNotServeNonPreparedFood food |> fail
    | ServeFoodCompletesIPOrder ipo _ -> servedFood :: [OrderServed(order, payment order)] |> ok
    | _                               -> [servedFood] |> ok
| PlacedOrder _ -> CanNotServeNonPreparedFood food |> fail
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _   -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _   -> CanNotServeWithClosedTab |> fail

let modifyInProgressOrder ipo modification =
    match modification with
    | AddFood food ->
        match food with
        | AlreadyServedFood ipo _   -> CanNotModifyAlreadyServedFood food |> fail
        | AlreadyPreparedFood ipo _ -> CanNotModifyAlreadyPreparedFood food |> fail
        | _                         -> [OrderModified modification] |> ok
    | RemoveFood food ->
        let order = ipo.PlacedOrder
        match food with
        | AlreadyServedFood ipo _   -> CanNotModifyAlreadyServedFood food |> fail
        | AlreadyPreparedFood ipo _ -> CanNotModifyAlreadyPreparedFood food |> fail
        | NonOrderedFood order _    -> CanNotModifyNonOrderedFood food |> fail
        | _                         -> [OrderModified modification] |> ok
    | AddDrink drink ->
        match drink with
        | AlreadyServedDrink ipo _ -> CanNotModifyAlreadyServedDrink drink |> fail
        | _                        -> [OrderModified modification] |> ok
    | RemoveDrink drink ->
        let order = ipo.PlacedOrder
        match drink with
        | AlreadyServedDrink ipo _ -> CanNotModifyAlreadyServedDrink drink |> fail
        | NonOrderedDrink order _  -> CanNotModifyNonOrderedDrink drink |> fail
        | _                        -> [OrderModified modification] |> ok

let modifyPlacedOrder order modification =
    match modification with
    | AddFood _  -> [OrderModified modification] |> ok
    | AddDrink _ -> [OrderModified modification] |> ok
    | RemoveFood food ->
        match food with
        | NonOrderedFood order _ -> CanNotModifyNonOrderedFood food |> fail
        | _                      -> [OrderModified modification] |> ok
    | RemoveDrink drink ->
        match drink with
        | NonOrderedDrink order _ -> CanNotModifyNonOrderedDrink drink |> fail
        | _                       -> [OrderModified modification] |> ok

let handleModifyOrder order modification = function
| OrderInProgress ipo -> modifyInProgressOrder ipo modification
| PlacedOrder _       -> modifyPlacedOrder order modification
| ServedOrder _       -> OrderAlreadyServed |> fail
| OpenedTab _         -> CanNotModifyNonPlacedOrder |> fail
| ClosedTab _         -> CanNotModifyClosedTab |> fail

let handleCloseTab payment = function
| ServedOrder order ->
    let orderAmount = orderAmount order
    if payment.Amount = orderAmount then
        [TabClosed payment] |> ok
    else
        InvalidPayment(orderAmount, payment.Amount) |> fail
| _ -> CanNotPayForNonServedOrder |> fail

let execute state command =
    match command with
    | OpenTab tab               -> handleOpenTab tab state
    | PlaceOrder order          -> handlePlaceOrder order state
    | ModifyOrder (order, modi) -> handleModifyOrder order modi state
    | ServeDrink (drink, tabId) -> handleServedDrink drink tabId state
    | PrepareFood (food, tabId) -> handlePrepareFood food tabId state
    | ServeFood (food, tabId)   -> handleServedFood food tabId state
    | CloseTab payment          -> handleCloseTab payment state
    
let evolve state command =
    match execute state command with
    | Ok (events, _) ->
        let newState = List.fold States.apply state events
        (newState, events) |> ok
        
    | Bad err -> Bad err
