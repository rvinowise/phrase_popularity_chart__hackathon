namespace moth

open System.Collections.Generic
open Plotly.NET
open Plotly.NET.LayoutObjects
open System


module Plot =
    let average_words_amount_in_paper = 4100//4500 //
    let average_words_repetition_in_paper = 5//40 //
    
    let color_sequence =
        [ "#1f77b4"; "#ff7f0e"; "#2ca02c"; "#d62728"; "#9467bd";
          "#8c564b"; "#e377c2"; "#7f7f7f"; "#bcbd22"; "#17becf"
          "#0000FF"; "#6495ED"; "#800080"] 
        |> List.map Color.fromHex
    
    let mutable next_color_index = 0;
    
    let phrase_colors: Dictionary<string, Color> = Dictionary()
    let phrase_color phrase =
        phrase_colors.TryGetValue phrase
        |>function
        | true, color -> color
        | false, _ ->
            phrase_colors[phrase] <- color_sequence[next_color_index]
            
            next_color_index <- next_color_index + 1
            color_sequence[next_color_index-1]
                

    let coefficient_to_equalize_graphs_from_max_values
        max_ngram
        max_pubmed
        =
        //multiply pubmed by ...
        max_ngram / max_pubmed
    
   
    let only_needed_years_in_map<'Amount> year_from year_to (popularity: Map<int, 'Amount>) =
        popularity
        |>Map.filter(fun year _ ->
            year <= year_to
            &&
            year >= year_from
        )
        
    let only_needed_years_for_popularities<'Amount>
        year_from
        year_to
        (popularities: (string*Map<int, 'Amount>) list) 
        =
        popularities
        |>List.map(fun popularity ->
            fst popularity,
            snd popularity |> only_needed_years_in_map year_from year_to
        )
     
          
    let absolute_popularity_to_relative
        (relative_to_what: Map<int, int>)
        (searched_amounts: (string*Map<int,int>) list)
        =
        searched_amounts
        |>List.map(fun (phrase,year_to_amount) ->
            let years_to_relative_amount =
                year_to_amount
                |>Map.map(fun year searched_absolute_amount ->
                    let total_amount_for_that_year =
                        relative_to_what
                        |>Map.tryFind year
                        |>Option.defaultValue 1
                    float searched_absolute_amount /
                    float total_amount_for_that_year
                )
            phrase
            ,
            years_to_relative_amount 
        )
        
    
    (* 
    these two coefficients are needed to equalize this result with the google Ngram results 
    (which compares words with words, not papers with papers like Pubmed)
    *)     
    let inline papers_to_words<
        'Amount when 'Amount : (static member op_Explicit: 'Amount -> float)
        >
        (paper_popularities: (string*Map<int,'Amount>) list)
        =
        paper_popularities
        |>List.map (fun (phrase,year_to_amount) ->
            phrase
            ,
            year_to_amount
            |>Map.map(fun _ amount ->
                float amount /
                float average_words_amount_in_paper *
                float average_words_repetition_in_paper
            )
        )