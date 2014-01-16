module VersionOne.Parsers.HTTP

open FParsec

open Primitives



type Challenge = {
  Scheme: string
  Realm: string
  Parameters: (string * string) list
  }
  with
  static member ofTuple (scheme,parameters) =
    match parameters |> List.tryFind (fst >> (=) "realm") with
    | Some (k,v) ->  {Scheme=scheme; Parameters=parameters; Realm=v}
    | _ ->           {Scheme=scheme; Parameters=parameters; Realm=null}

let QUOTEDPARAM = TOKEN .>> skipString "=" .>>. QUOTEDSTRING

let commaSep parser = sepBy parser (spaces .>> skipString "," .>> spaces)

let PARAMSLIST = commaSep QUOTEDPARAM 

let CHALLENGE = TOKEN .>> skipString " " .>>. PARAMSLIST |>> Challenge.ofTuple

let CHALLENGES = commaSep CHALLENGE 

let parseChallenges s =
  match run CHALLENGES s with
  | Success (cs, _, _) -> cs
  | _ as failure ->
    printfn "%A" failure
    []


