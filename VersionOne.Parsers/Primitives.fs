module VersionOne.Parsers.Primitives


open FParsec

let concat (s:'a seq) = System.String.Concat(s)

let separators = "()<>@,;:\\\"/[]?={} \t"

let controls =  [| for n = 0 to 31 do yield char n |] |> concat

let TOKEN : Parser<string,unit> = many1 (noneOf (separators + controls)) |>> concat

let QUOTEDPAIR = skipString "\\" >>. anyChar |>> function
    | 'r' -> '\r'  | 'n' -> '\n'  | 't' -> '\t'  | 'b' -> '\b'
    | 'a' -> '\a'  | 'f' -> '\f'  | 'v' -> '\v'  | _ as qc -> qc

let QDTEXT = noneOf "\""

let QUOTEDSTRING : Parser<string,unit> = skipString "\""  >>. many (QUOTEDPAIR <|> QDTEXT) .>> skipString "\"" |>> concat

