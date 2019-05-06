module CafeAppTestsDSL

open FsUnit
open NUnit.Framework
open Chessie.ErrorHandling

open CommandHandlers
open Commands
open Errors
open States

let Given (state: State) = state
let When command state = (command, state)

let ThenStateShouldBe expectedState (command, state) =
    match evolve state command with
    | Ok((actualState, events), _) ->
        actualState |> should equal expectedState
        events |> Some
    | Bad errs ->
        sprintf "Expected %A but actually %A" expectedState errs.Head
        |> Assert.Fail
        None
    
let WithEvents expectedEvents actualEvents =
    match actualEvents with
    | Some (actualEvents) -> actualEvents |> should equal expectedEvents
    | None                -> None |> should equal expectedEvents
    
let ShouldFailWith (expectedError: Error) (command, state) =
    match evolve state command with
    | Bad errs  -> errs.Head |> should equal expectedError
    | Ok (r, _) ->
        sprintf "Expected %A but actually %A" expectedError r
        |> Assert.Fail
