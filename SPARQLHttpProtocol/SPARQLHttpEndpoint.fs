module SPARQLHttpProtocol

open System
open System.IO
open System.Net
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.HttpRequestHeaders

type Prefix =
    string * string

type ValueType = 
    | URI
    | LITERAL

type QueryResult = 
    { Value : string
      Type : ValueType }

let internal parseJsonSelect plainResponse = 
    // Not sure why this isn't offered on JSON Values
    let asString = JsonExtensions.AsString
    let asArray = JsonExtensions.AsArray
    
    let extractResultType x = 
        match asString x?``type`` with
        | "uri" -> URI
        | "literal" -> LITERAL
        | _ -> failwith "Unexpected result type"
    
    let response = JsonValue.Parse(plainResponse)
    asArray (response?results?bindings) 
    |> Seq.map 
           (fun binding -> 
           binding.Properties 
           |> Seq.fold (fun (accumulator : Map<string, QueryResult>) (variable, boundValue) -> 
                  accumulator.Add(variable, 
                                  { Value = (asString boundValue?value)
                                    Type = extractResultType boundValue })) Map.empty<string, QueryResult>)

type SPARQLHttpEndpoint(selectEndpoint : string, ?updateUrl : string) = 
    class
        let mutable prefixes = []
        member __.AddPrefix(prefix : Prefix) = prefixes <- prefix :: prefixes
        member __.Prefixes = prefixes
        
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
            if updateUrl.IsNone then failwith "You need to specify a URL that accepts update
                    queries (in the constructor) before you can actually execute them"
            Http.RequestString
                (url = updateUrl.Value, httpMethod = "POST", query = [], 
                 headers = [ Accept HttpContentTypes.Json ]) |> printfn "%A"
        
        member __.IsWritable = 
            // Something more clever needed
            updateUrl.IsSome
    end
