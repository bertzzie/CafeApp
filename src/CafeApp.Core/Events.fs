module Events

open Domain
open System

type Event =
    | TabOpened of Tab
    | OrderPlaced of Order
    | OrderModified of OrderModificationInfo
    | DrinkServed of Drink * Guid
    | FoodPrepared of Food * Guid
    | FoodServed of Food * Guid
    | OrderServed of Order * Payment
    | TabClosed of Payment

