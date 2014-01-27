namespace VersionOne.Parsers.HTTP

open FParsec
open VersionOne.Parsers.Primitives

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

  static member ofScheme scheme =
    {Scheme=scheme; Parameters=[]; Realm=null}

  static member parseChallenges s = 
    let QUOTEDPARAM = TOKEN .>> skipString "=" .>>. QUOTEDSTRING

    let followingCommaItem parser = (spaces .>> skipString "," .>> spaces) >>. parser

    let commaSep1 parser = parser .>>.? many (attempt <| followingCommaItem parser) |>> fun (x,xs) -> x::xs

    let PARAMSLIST = commaSep1 QUOTEDPARAM

    let CHALLENGE_WITH_PARAMS = TOKEN .>> skipString " " .>>. PARAMSLIST |>> Challenge.ofTuple
    let CHALLENGE_WITHOUT_PARAMS = TOKEN |>> Challenge.ofScheme

    let CHALLENGES = commaSep1 (attempt CHALLENGE_WITH_PARAMS <|> CHALLENGE_WITHOUT_PARAMS) 

    match run CHALLENGES s with
    | Success (cs, _, _) -> cs
    | _ as failure -> [] //failwith "Unable to parse WWW-Authenticate challenge"