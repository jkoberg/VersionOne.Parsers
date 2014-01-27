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
    let samples = [ "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                    "Basic realm=\"V1Production\"" ]
    let sample = System.String.Join(", ", samples)
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

  let [<Test>] ``handles comma-joined simple www-authenticate challenges`` () = 
    let samples = [ "Negotiate" 
                    "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                    ]
    let sample = System.String.Join(", ", samples)
    parseChallenges sample =?= [

      { Scheme = "Negotiate"
        Realm = null
        Parameters = [ ] }

      { Scheme = "Bearer"
        Realm = "V1Production"
        Parameters = [ "realm", "V1Production"
                       "scope", "apiv1"  
                       "error", "invalid_token" ] } 

      ]

  let [<Test>] ``handles comma-joined simple www-authenticate challenges 2`` () = 
    let samples = [ "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                    "Negotiate" 
                    ]
    let sample = System.String.Join(", ", samples)
    parseChallenges sample =?= [

      { Scheme = "Bearer"
        Realm = "V1Production"
        Parameters = [ "realm", "V1Production"
                       "scope", "apiv1"  
                       "error", "invalid_token" ] } 

      { Scheme = "Negotiate"
        Realm = null
        Parameters = [ ] }

      ]
        
        