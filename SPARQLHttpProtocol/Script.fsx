// Weitere Informationen zu F# finden Sie unter http://fsharp.net. Im Projekt 'F#-Lernprogramm' finden Sie
// einen Leitfaden zum Programmieren in F#.
#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
#load "SPARQLHttpEndpoint.fs"

open SPARQLHttpProtocol

// Skriptcode für die Bibliothek hier definieren
let endpoint = 
    new SPARQLHttpEndpoint("http://stardog.west.uni-koblenz.de:8080/openrdf-sesame/repositories/test", 
                           "http://stardog.west.uni-koblenz.de:8080/openrdf-sesame/repositories/test/statements")

let prefixes = 
    [ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"
      "rdfs", "http://www.w3.org/2000/01/rdf-schema#" ]

prefixes |> Seq.iter endpoint.AddPrefix
//endpoint.Query({ Modifier = DISTINCT
//                 Projection = STAR
//                 WhereClause = [ (Var "s", Var "p", Uri typeUri) ] })
//endpoint.Query({ DistinctSelect with Projection = Vars [ "s"; "p" ]
//                                     WhereClause = [ (Var "s", Var "p", Uri typeUri) ] })
//let x = SELECT(DISTINCT, STAR, [ (Var "s", Var "p", Literal "mimimi") ])
//endpoint.Query("SELECT DISTINCT * WHERE { ?s ?p ?o .}") |> printfn "%A"
endpoint.Query("SELECT DISTINCT ?p WHERE { ?p a rdf:Property .}") |> printfn "%A"

//endpoint.Update("INSERT DATA { <http://testuri.com> a <http://myArtist> .}")

// SELECT DISTINCT %3Fp WHERE {%0A                    %3Fp a <http%3A%2F%2Fwww.w3.org%2F1999%2F02%2F22-rdf-syntax-ns%23Property> .%0A 
// SELECT%20DISTINCT%20?p%20WHERE%20%7B%20?p%20a%20%253Chttp://www.w3.org/1999/02/22-rdf-syntax-ns#Property%253E%20.%7D
// SELECT+DISTINCT+%3fp+WHERE+%7b+%3fp+a+%3chttp%3a%2f%2fwww.w3.org%2f1999%2f02%2f22-rdf-syntax-ns%23Property%3e+.%7d