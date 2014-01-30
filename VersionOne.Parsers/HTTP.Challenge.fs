namespace VersionOne.Parsers.HTTP

open FParsec
open VersionOne.Parsers.Primitives

module Parsers = 
  
    let followingCommaItem parser = (spaces .>> skipString "," .>> spaces) >>. parser

    let commaSep1 parser = parser .>>.? many (attempt <| followingCommaItem parser) |>> fun (x,xs) -> x::xs

    let QUOTEDPARAM = TOKEN .>> skipString "=" .>>. QUOTEDSTRING
    let PARAMSLIST = commaSep1 QUOTEDPARAM

    let parseParams s =
      match run PARAMSLIST s with
      | Success (cs, _, _) -> cs
      | _ as failure -> [] //failwith "Unable to parse params"

type Challenge = {
  Scheme: string
  Realm: string
  Parameters: (string * string) list
  }
  with

  static member ofTuple (scheme,parameters) =
    let realm =
      match parameters |> List.tryFind (fst >> (=) "realm") with
      | Some (k,v) ->  v
      | _ -> null
    {Scheme=scheme; Parameters=parameters; Realm=realm}

  static member ofScheme scheme =
    {Scheme=scheme; Parameters=[]; Realm=null}

  static member parseChallenges s = 
    let CHALLENGE_WITH_PARAMS = TOKEN .>> skipString " " .>>. Parsers.PARAMSLIST |>> Challenge.ofTuple
    let CHALLENGE_WITHOUT_PARAMS = TOKEN |>> Challenge.ofScheme
    let CHALLENGES = Parsers.commaSep1 (attempt CHALLENGE_WITH_PARAMS <|> CHALLENGE_WITHOUT_PARAMS) 
    match run CHALLENGES s with
    | Success (cs, _, _) -> cs
    | _ as failure -> [] //failwith "Unable to parse WWW-Authenticate challenge"

