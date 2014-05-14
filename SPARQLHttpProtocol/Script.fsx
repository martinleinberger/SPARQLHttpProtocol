// Weitere Informationen zu F# finden Sie unter http://fsharp.net. Im Projekt 'F#-Lernprogramm' finden Sie
// einen Leitfaden zum Programmieren in F#.

#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
#load "SPARQLHttpEndpoint.fs"
open SPARQLHttpProtocol

// Skriptcode für die Bibliothek hier definieren
let endpoint = new Endpoint("http://stardog.west.uni-koblenz.de:8080/openrdf-sesame/repositories/test", "")
endpoint.Query("SELECT DISTINCT * WHERE { ?s ?p ?o .}") |> printfn "%A"