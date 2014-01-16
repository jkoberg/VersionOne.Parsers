namespace VersionOne.Parsers.Tests

open NUnit.Framework

module HTTP =

  let ( =?= ) (actual:obj) (expected:obj) =
    printfn "actual: %A\nexpected: %A" actual expected
    Assert.AreEqual(actual, expected)

  open VersionOne.Parsers.HTTP

  let [<Test>] ``handles single www-authenticate challenge`` () = 
    let sample = "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
    parseChallenges sample =?= [ { Scheme = "Bearer"
                                   Realm = "V1Production"
                                   Parameters = [ "realm", "V1Production"
                                                  "scope", "apiv1"  
                                                  "error", "invalid_token" ] } ]
                                          
  let [<Test>] ``handles comma-joined www-authenticate challenges`` () = 
    let sample = "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
    let sample = sample + ", Basic realm=\"V1Production\""
    parseChallenges sample =?= [

      { Scheme = "Bearer"
        Realm = "V1Production"
        Parameters = [ "realm", "V1Production"
                       "scope", "apiv1"  
                       "error", "invalid_token" ] } 

      { Scheme = "Basic"
        Realm = "V1Production"
        Parameters = [ "realm", "V1Production" ] }

      ]
    