namespace moth

open System.Text.Json
open FsUnit
open Xunit
open FsHttp

module Read_google_ngram =
    type Query_param = {
        phrases: string list
        year_start: int
        year_end: int
        corpus: string
    }
    
    let max_data_query phrases =
        {
            Query_param.corpus = "en"
            year_start = 1800 
            year_end = System.DateTime.Now.Year
            phrases = phrases
        }
    
    let modern_data_query phrases =
        {
            Query_param.corpus = "en"
            year_start = 1970 
            year_end = System.DateTime.Now.Year
            phrases = phrases
        }
    
    type Ngram_result = {
        ngram: string
        timeseries:float list
    }
    
    let base_url = "https://books.google.com/ngrams/json"
    let url_returning_json (query:Query_param) =
        // let query = {
        //     query with
        //         phrases =
        //             query.phrases
        //             |>List.map(fun phrase ->
        //                 "\"" + phrase + "\""
        //             )
        //     }
            
        let query_parms = [            
            "year_start="+(string)query.year_start
            "year_end="+(string)query.year_end
            "corpus="+query.corpus
            "content="+(
                String.concat "," query.phrases   
            )
        ]
        
        
        String.concat "&" query_parms
        |>(+) (base_url + "?")

    let retrieve_json query =
        let full_url = url_returning_json query
        http {
            GET full_url
        }
        |>Request.send
        |>Response.toJson
        
    
    let phrases_popularity (query:Query_param) =
        query
        |>retrieve_json
        |>JsonSerializer.Deserialize<Ngram_result list>
        |>List.map(fun ngram_result ->
            let year_to_amount =
                ngram_result.timeseries
                |>List.fold(fun (year_to_amount,index) amount ->
                    let year = index + query.year_start
                    
                    year_to_amount
                    |>Map.add year amount
                    ,
                    index+1
                        
                )
                    (Map.empty,0)
                |>fst
            
            ngram_result.ngram
            ,
            year_to_amount
        )
    
    
        
    [<Fact>]
    let ``try request from google``()=
        let full_url =
            {
                Query_param.corpus="en"
                year_start = 1800
                year_end = 2022
                phrases = ["transhumanism";"christianity"] 
            }
            |>url_returning_json
        
        let output =
            http {
                GET full_url
            }
            |>Request.send
            |>Response.toJson
        ()