module SPARQLHttpProtocol

open System
open System.IO
open System.Net
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.HttpRequestHeaders

let internal encodeVariable (x : string) = 
    if x.StartsWith("?") then x
    else "?" + x

let internal encodeUri (x : string) = 
    if not (x.StartsWith("<") || x.EndsWith(">")) then "<" + x + ">"
    else x

// Result types
type ValueType = 
    | URI
    | LITERAL

type QueryResult = 
    { Value : string
      Type : ValueType }

//[1]   	Query 	            ::=   	Prologue
//                                      ( SelectQuery | ConstructQuery | DescribeQuery | AskQuery )
//[5]   	SelectQuery 	    ::=   	'SELECT' ( 'DISTINCT' | 'REDUCED' )? ( Var+ | '*' ) DatasetClause* WhereClause SolutionModifier
type Prefix = string * string

type Variable = Var of string

type Uri = string

type Literal = string

type PatterElement = 
    | Var of string
    | Uri of Uri
    | Literal of Literal
    override this.ToString() = 
        match this with
        | Var x -> encodeVariable x
        | Uri x -> encodeUri x
        | Literal x -> x

type Triple = PatterElement * PatterElement * PatterElement

type Projection = 
    | STAR
    | Vars of string  list
    override this.ToString() = 
        match this with
        | STAR -> "*"
        | Vars x -> 
            x
            |> List.map encodeVariable
            |> String.concat " "

type Modifier = 
    | DISTINCT
    | REDUCED
    | NONE
    override __.ToString() = 
        match __ with
        | DISTINCT -> "DISTINCT"
        | REDUCED -> "REDUCED"
        | NONE -> ""

type SelectQuery = 
    { Modifier : Modifier
      Projection : Projection
      WhereClause : Triple list }
    override this.ToString() = 
        let pattern = 
            this.WhereClause
            |> List.map (fun (s, p, o) -> s.ToString() + " " + p.ToString() + " " + o.ToString() + " .")
            |> String.concat "\n"
        "SELECT " + this.Modifier.ToString() + this.Projection.ToString() + "WHERE { " + pattern + "}"

let DistinctSelect = 
    { Modifier = DISTINCT
      Projection = STAR
      WhereClause = [] }

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
        
        let convertPrefixes = 
            prefixes
            |> Seq.map (fun (prefix, ns) -> "PREFIX " + prefix + ":<" + ns + ">")
            |> String.concat ("\n")
        
        member __.AddPrefix(prefix : Prefix) = prefixes <- prefix :: prefixes
        member __.Prefixes = prefixes
        member __.Query(sparql_select : SelectQuery) = __.Query(sparql_select.ToString())
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sparql_select">The record describing the select query</param>
        member __.Query sparql_select = 
            let sparql_select' = (convertPrefixes + "\n" + sparql_select).Replace(" ", "+")//.Replace("<", "%3a").Replace(">", "%3e")
            
            let complete = selectEndpoint + "?query=" + sparql_select'
            Http.RequestString
                (url = selectEndpoint, httpMethod = "GET", query = [ ("query", sparql_select') ], 
                 headers = [ Accept HttpContentTypes.Json ]) |> parseJsonSelect
        
        member __.Update(sparql_update) = 
            if updateUrl.IsNone then failwith "You need to specify a URL that accepts update
                    queries (in the constructor) before you can actually execute them"
            let sparql_update' = convertPrefixes + "\n" + sparql_update
            Http.RequestString
                (url = updateUrl.Value, httpMethod = "POST", query = [ ("query", sparql_update') ], 
                 headers = [ Accept HttpContentTypes.Json ]) |> printfn "%A"
        
        member __.IsWritable = 
            // Something more clever needed
            updateUrl.IsSome
    end
