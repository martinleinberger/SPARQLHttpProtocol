module SPARQLHttpProtocol

open System
open System.IO
open System.Net
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.HttpRequestHeaders

type ResultType = 
    | URI
    | LITERAL

type QueryResult = 
    { Value : string
      Type : ResultType }

let internal parseJsonSelect response = 
    // Not sure why this isn't offered on JSON Values
    let asString = JsonExtensions.AsString
    let asArray = JsonExtensions.AsArray
    
    let extractResultType x = 
        match asString x?``type`` with
        | "uri" -> URI
        | "literal" -> LITERAL
        | _ -> failwith "Unexpected result type"
    
    let response' = JsonValue.Parse(response)
    asArray (response'?results?bindings) 
    |> Seq.map 
           (fun binding -> 
           binding.Properties 
           |> Seq.fold (fun (accumulator : Map<string, QueryResult>) (variable, boundValue) -> 
                  accumulator.Add(variable, 
                                  { Value = (asString boundValue?value)
                                    Type = extractResultType boundValue })) Map.empty<string, QueryResult>)

type Endpoint(selectEndpoint : string, ?updateUrl : string) = 
    class
        // I dislike this part here >>>
        let mutable prefixes = []
        member __.AddPrefix(p : string * string) = prefixes <- p :: prefixes
        member __.AddPrefixes(l : (string * string) list) = prefixes <- l @ prefixes
        // <<<
        member __.Query(sparql_select) = 
            let queryPrefixes = 
                prefixes
                |> Seq.map (fun (prefix, ns) -> "PREFIX " + prefix + ":<" + ns + ">")
                |> String.concat ("\n")
            
            let sparql_select' = queryPrefixes + "\n" + sparql_select
            Http.RequestString
                (url = selectEndpoint, httpMethod = "GET", query = [ ("query", sparql_select) ], 
                 headers = [ Accept HttpContentTypes.Json ]) |> parseJsonSelect
        
        member __.Update(sparql_update) =
            // Still todo
            ()
        member __.IsWritable = 
            // Something more clever needed
            updateUrl.IsSome
    end
