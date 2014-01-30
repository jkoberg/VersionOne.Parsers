module VersionOne.Parsers.Tests.Primitives

open NUnit.Framework
open VersionOne.Parsers.Primitives
open FParsec

let ( =?= ) (actual:obj) (expected:obj) =
  printfn "actual: %A\nexpected: %A" actual expected
  Assert.AreEqual(actual, expected)
  
let test p expected input = 
  match run p input with
  | Success (s, _, _) -> s =?= expected
  | Failure (why,err,_) -> Assert.Fail(why)

let [<Test>] TestQUOTEDSTRING () = 
  test QUOTEDSTRING "hi\r\n" "\"hi\\r\\n\""
  test QUOTEDSTRING "yo" "\"yo\""
  test QUOTEDSTRING "whatsup\a\b\f\n\r\t\vhithere" "\"whatsup\\a\\b\\f\\n\\r\\t\\vhithere\""