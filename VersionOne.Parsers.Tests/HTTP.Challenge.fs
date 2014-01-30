module VersionOne.Parsers.Tests.HTTP.Challenge

open NUnit.Framework
open VersionOne.Parsers.HTTP


let ( =?= ) (actual:obj) (expected:obj) =
  printfn "actual: %A\nexpected: %A" actual expected
  Assert.AreEqual(actual, expected)


let [<Test>] ``just parameters`` () = 
  let sample = "realm=\"The So-Called, \\\"Realm\\\"\", scope=\"apiv1\", error=\"invalid_token\""

  //printfn "testing %s" sample
  Parsers.parseParams sample =?= [
    "realm", "The So-Called, \"Realm\""
    "scope", "apiv1"  
    "error", "invalid_token"
    ]



let [<Test>] ``single www-authenticate challenge`` () = 
  let sample = "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
  //printfn "testing %s" sample
  Challenge.parseChallenges sample =?= [{ Scheme = "Bearer"
                                          Realm = "V1Production"
                                          Parameters = [ "realm", "V1Production"
                                                         "scope", "apiv1"  
                                                         "error", "invalid_token" ] } ]
                                          
let [<Test>] ``comma-joined www-authenticate challenges`` () = 
  let samples = [ "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                  "Basic realm=\"V1Production\"" ]
  let sample = System.String.Join(", ", samples)
  //printfn "testing %s" sample
  Challenge.parseChallenges sample =?= [

    { Scheme = "Bearer"
      Realm = "V1Production"
      Parameters = [ "realm", "V1Production"
                     "scope", "apiv1"  
                     "error", "invalid_token" ] } 

    { Scheme = "Basic"
      Realm = "V1Production"
      Parameters = [ "realm", "V1Production" ] }

    ]

let [<Test>] ``simple comma-joined challenges 0`` () = 
  let samples = [ "NTLM"
                  "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                  ]
  let sample = System.String.Join(", ", samples)
  //printfn "testing %s" sample
  Challenge.parseChallenges sample =?= [

    { Scheme = "NTLM"
      Realm = null
      Parameters = [ ] }

    { Scheme = "Bearer"
      Realm = "V1Production"
      Parameters = [ "realm", "V1Production"
                     "scope", "apiv1"  
                     "error", "invalid_token" ] } 

    ]

let [<Test>] ``more simple comma-joined challenges`` () = 
  let samples = [ "Negotiate"
                  "NTLM"
                  "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                  ]
  let sample = System.String.Join(", ", samples)
  //printfn "testing %s" sample
  Challenge.parseChallenges sample =?= [

    { Scheme = "Negotiate"
      Realm = null
      Parameters = [ ] }

    { Scheme = "NTLM"
      Realm = null
      Parameters = [ ] }

    { Scheme = "Bearer"
      Realm = "V1Production"
      Parameters = [ "realm", "V1Production"
                     "scope", "apiv1"  
                     "error", "invalid_token" ] } 

    ]

let [<Test>] ``comma-joined simple www-authenticate challenges`` () = 
  let samples = [ "Bearer realm=\"V1Production\", scope=\"apiv1\", error=\"invalid_token\""
                  "Negotiate" 
                  ]
  let sample = System.String.Join(", ", samples)
  //printfn "testing %s" sample
  Challenge.parseChallenges sample =?= [

    { Scheme = "Bearer"
      Realm = "V1Production"
      Parameters = [ "realm", "V1Production"
                     "scope", "apiv1"  
                     "error", "invalid_token" ] } 

    { Scheme = "Negotiate"
      Realm = null
      Parameters = [ ] }

    ]
        
        