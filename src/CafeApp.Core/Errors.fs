module Errors

open Domain

type Error =
    | TabAlreadyOpened
    | CanNotPlaceEmptyOrder
    | CanNotOrderWithClosedTab
    | OrderAlreadyPlaced
    | OrderAlreadyServed
    | CanNotServeWithClosedTab
    | CanNotServeForNonPlacedOrder
    | CanNotServeNonOrderedDrink of Drink
    | CanNotServeAlreadyServedDrink of Drink
    | CanNotPrepareWithClosedTab
    | CanNotPrepareForNonPlacedOrder
    | CanNotPrepareNonOrderedFood of Food
    | CanNotPrepareAlreadyPreparedFood of Food
    | CanNotServeNonOrderedFood of Food
    | CanNotServeNonPreparedFood of Food
    | CanNotServeAlreadyServedFood of Food

