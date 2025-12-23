namespace moth

open System
open System.Text.Json
open FsHttp
open Xunit

(* using unofficial pub-med data provider: https://esperr.github.io/visualizingpubmed/ *)

module Read_pubmed_keywords =
    
    
    
    type Pubmed_popularity_esperr = {
        counts: Map<string, string>
        search: string
        error: string
    }
    
    
    let esperr_result_to_numerical (esper_result: Pubmed_popularity_esperr) =
        let year_to_amount =
            esper_result.counts
            |>Map.toSeq
            |>Seq.map(fun (str_year, str_amount) ->
                int str_year, int str_amount
            )|>Map.ofSeq
        
        esper_result.search//.Replace("-", " ")
        ,
        year_to_amount 
    
    
    
    //let base_url = "https://esperr.github.io/pubmed-by-year" //?q1=immortality
    let base_url = "https://med-by-year.appspot.com/newsearch"

    let url_multiple_words_returning_csv words =
        
        let words_as_params = 
            words
            |>List.mapi(fun index word  ->
                "q"+string index + "=" + word
            )
    
        String.concat "&" words_as_params
        |> (+) (base_url + "?")
    
    let url_single_word_returning_csv (phrase:string) =
        phrase.Replace(" ", "-") //pubmed can group words into phrases with hyphens (or, maybe, double quotes)
        |>(+) (base_url + "?q=")
        //base_url + "?q=\"" + phrase + "\""
        
    

    let from_esperr phrase =
        
        let full_url = url_single_word_returning_csv phrase
        
        let str_result = 
            http {
                GET full_url
            }
            |>Request.send
            |>Response.toJson
            |>JsonSerializer.Deserialize<Pubmed_popularity_esperr>
        if (str_result.error |> isNull) then
            esperr_result_to_numerical str_result            
        else
            phrase, Map.empty
    
    let total_articles_on_pubmed () =
        from_esperr "all[sb]"
        |>snd
    
    
        
    let pubmed_relative_popularities (words: string list) =
        let total_amounts = (total_articles_on_pubmed()) 
        
        words
        |>List.map from_esperr
        |>Plot.absolute_popularity_to_relative total_amounts
    
    [<Fact>]
    let ``try from_esperr``() =
        let result = 
            "mind uploading"
            |>from_esperr
        
        ()