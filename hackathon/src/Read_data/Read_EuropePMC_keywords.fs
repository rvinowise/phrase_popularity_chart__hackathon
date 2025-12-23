namespace moth

open System.Text.Json
open FsHttp
open Xunit
open System

(*  *)

module Read_PMC_keywords =

    let articles_on_page = 1000;
    
    type PMC_article_raw = {
        pubYear: string
    }
    
    type PMC_packed_list = {
        result: PMC_article_raw list
    }
    
    type PMC_root_result_raw = {
        hitCount: int
        nextPageUrl: string
        resultList: PMC_packed_list
    }
    
    
    let base_url = "https://www.ebi.ac.uk/europepmc/webservices/rest/search"


    
    let url_single_word_returning_csv (phrase:string) =
        
        base_url + $"?format=json&pageSize={articles_on_page}&query=\"{phrase}\""
    

    let PMC_raw_to_numerical year_to_amounts (raw_result: PMC_root_result_raw)  = 
        raw_result.resultList.result
        |>List.fold(fun year_to_amounts article ->
            if (isNull article.pubYear ) then
                year_to_amounts
            else
                let year = int article.pubYear
                let old_amount =
                    year_to_amounts
                    |>Map.tryFind year
                    |>Option.defaultValue 0
                year_to_amounts
                |>Map.add
                  year
                  (old_amount + 1)
        )
            year_to_amounts

    
    let rec from_PMC_next_page page_url (year_to_amounts: Map<int, int>) page_number =
        
        
        let str_result = 
            http {
                GET page_url
            }
            |>Request.send
            |>Response.toJson
            |>JsonSerializer.Deserialize<PMC_root_result_raw>
        
        let pages_amount =
            (float str_result.hitCount) / (float articles_on_page)
            |>Math.Ceiling
        $"reading PMS, page {page_number}/{pages_amount}"
        |>Log.important
        
        let year_to_amounts =
            PMC_raw_to_numerical year_to_amounts str_result
    
        if (isNull str_result.nextPageUrl) then
            year_to_amounts
        else
            from_PMC_next_page str_result.nextPageUrl year_to_amounts (page_number+1)
    
    let from_PMC_all_pages phrase =
        let first_page_url = url_single_word_returning_csv phrase

        $"reading PMC for \"{phrase}\""
        |>Log.important
        
        from_PMC_next_page first_page_url Map.empty 1
        
    [<Fact>]
    let ``try from_PMC``() =
        let result = 
            "cryonics"
            |>from_PMC_all_pages
        
        ()