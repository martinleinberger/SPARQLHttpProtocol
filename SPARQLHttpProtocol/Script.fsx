// Weitere Informationen zu F# finden Sie unter http://fsharp.net. Im Projekt 'F#-Lernprogramm' finden Sie
// einen Leitfaden zum Programmieren in F#.
#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
#load "SPARQLHttpEndpoint.fs"

open SPARQLHttpProtocol

// Skriptcode für die Bibliothek hier definieren
let endpoint = 
    new SPARQLHttpEndpoint("http://stardog.west.uni-koblenz.de:8080/openrdf-sesame/repositories/test", 
                           "http://stardog.west.uni-koblenz.de:8080/openrdf-sesame/repositories/test/statements")


let prefixes = [
    "rdf" ,"http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    "rdfs","http://www.w3.org/2000/01/rdf-schema#"
]

prefixes |> Seq.iter endpoint.AddPrefix

endpoint.Query("SELECT DISTINCT * WHERE { ?s ?p ?o .}") |> printfn "%A"
endpoint.Update("INSERT DATA { <http://testuri.com> a <http://myArtist> .}")
