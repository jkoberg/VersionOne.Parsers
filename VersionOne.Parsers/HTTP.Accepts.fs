﻿module VersionOne.Parsers.HTTP.Accepts

open FParsec
open VersionOne.Parsers.Primitives

type Param =
  | Q of float
  | Mxb of int64
  | Other of string * string

type MediaRange = 
  | Qualified
  | SubtypesOf
  | Any

type AcceptItem = {
    qualification: MediaRange
    mtype: string
    msubtype: string
    q: float
    mxb: int64 option
    parameters: Param list
    }
  with
  static member Default = {
    qualification = Any
    mtype = "*"
    msubtype = "*"
    q = 1.0
    mxb = None
    parameters = []
    }

module Parse = 
  let concat (s:'a seq) = System.String.Concat(s)
  let rstrip (s:string) = s.TrimEnd()

  let rec scanPs state = function
    | [] -> state
    | p::ps -> match p with
                | Mxb count -> scanPs {state with mxb=Some count} ps
                | Q q -> scanPs {state with q=q} ps
                | _ -> scanPs {state with parameters=p::state.parameters} ps

  let unpackAcceptItem ((t,st), ps) =
    ps |> scanPs {AcceptItem.Default 
                  with
                   mtype = t
                   msubtype = st
                   qualification =
                     match (t,st) with
                     | ("*","*") -> Any
                     | (t,"*") -> SubtypesOf
                     | (t,st) -> Qualified
                  }

  let ATTRIBUTE = TOKEN

  let VALUE = TOKEN <|> QUOTEDSTRING

  let PARAMETER = ATTRIBUTE .>> skipString "=" .>>. VALUE |>> Param.Other
  let QPARAM = skipString "q=" >>. pfloat |>> Q
  let MXBPARAM = skipString "mxb=" >>. pint64 |>> Mxb
  let PARAMETERS = many (skipString ";" >>. (QPARAM <|> MXBPARAM <|> PARAMETER))

  let MTYPE = TOKEN
  let MSUBTYPE = TOKEN
  let MEDIATYPE = MTYPE .>> skipString "/" .>>. MSUBTYPE .>>. PARAMETERS 
  
  let ACCEPTS = parse {
    do! spaces
    let! mt = MEDIATYPE |>> unpackAcceptItem
    let! mts = many (parse {
      do! skipString ","
      do! spaces
      return!  MEDIATYPE |>> unpackAcceptItem
      })
    return mt :: mts
    }


open Parse

let findSortableMatches (accept:string)  (mt:string)  = [
  match (run ACCEPTS accept, run MEDIATYPE mt) with
  | Success (ais, _, _), Success (((t,st),ps),_,_) ->
    for item in ais do
      let matchingParams = Set item.parameters |> Set.intersect (Set ps) |> Set.toList
      match item.qualification with
      | Any -> yield ((0,0), List.length matchingParams, item.q), item, matchingParams
      | SubtypesOf when item.mtype = t -> yield ((1,0), List.length matchingParams, item.q), item, matchingParams
      | Qualified when item.mtype=t && item.msubtype=st -> yield ((1,1), List.length matchingParams, item.q), item, matchingParams
      | _ -> ()
  | _ -> ()
  ]

let FindMatches accept mt = 
  let found = findSortableMatches accept mt |> List.sort |> List.rev
  [for (sortKey, ai, matchedParams) in found -> (ai.mtype, ai.msubtype, ai.q, matchedParams)] 




