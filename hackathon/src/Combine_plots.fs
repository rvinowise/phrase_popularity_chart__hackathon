namespace moth

open System.Collections.Generic
open Plotly.NET
open Plotly.NET.LayoutObjects
open System
open Giraffe.ViewEngine

module Combine_plots =
    
    let pubmed_chart_name = "Pubmed relative"
    let PMC_chart_name = "PMC relative to Pubmed's total"
    let ngram_chart_name = "Literature"
    
    let year_from = 1980
    let year_to = DateTime.Now.Year
    
    let coefficient_to_equalize_graphs_from_max_values
        max_ngram
        max_pubmed
        =
        //multiply pubmed by ...
        max_ngram / max_pubmed
    
    let coefficient_to_equalize_graphs
        base_results
        secondary_results
        =
        let max_base = Plot_from_map.highest_popularity_value base_results
        let max_secondary = Plot_from_map.highest_popularity_value secondary_results
        coefficient_to_equalize_graphs_from_max_values max_base max_secondary
    
    let no_coeffieicnt ngram_results pubmed_relative_results =
        1.
    
    let add_descriptions chart =
        chart
        |>Chart.withTitle
            "Popularity of phrases  <br><sup>on Pubmed, EuropePMC, and in literature</sup>"
        |>Chart.withXAxis (
            LinearAxis.init(Title = Title.init(Text = "Year"))   
        )
        |>Chart.withYAxis (
            LinearAxis.init(Title = Title.init(Text = "Relative occurrence"))   
        )
        |>Chart.withDescription [
            div [] [
                h3 [] [str "Sources"]
                rawText "<b>Pubmed</b>: <i>https://esperr.github.io/pubmed-by-year</i>"
                br []
                rawText "<b>Europe PMC</b>: <i>https://europepmc.org</i>"
                br []
                rawText "<b>Literature</b>: <i>https://books.google.com/ngrams</i>"
                ]
            div [] [
                h3 [] [str "Formula"]
                rawText $"<b>{pubmed_chart_name}</b> = articles on Pumbed with the searched phrase / total articles on Pumbed in that year"
                br []
                rawText $"<b>{PMC_chart_name}</b> = articles on EuropePMC with the searched phrase / total articles on Pumbed in that year (because EuropePMC doesn't provide its total articles per year)"
                br []
                rawText $"<b>{ngram_chart_name}</b> = amount of the searched phrase relative to all other words in all books"
                br []
                rawText $"<i>in order to equalise the amounts of articles with the amount of words, the amount of articles is divided by {Plot.average_words_amount_in_paper} (average words in an article) and multiplied by {Plot.average_words_repetition_in_paper} (average word repetition in an article)</i>"
                ]
        ]
    
    let pubmed_vs_ngram_chart
        (find_pubmed_coefficient)
        (phrases: string list)
        =
        let ngram_query = Read_google_ngram.modern_data_query phrases
        
        let ngram_results =
            ngram_query
            |>Read_google_ngram.phrases_popularity
            |>Plot.only_needed_years_for_popularities year_from year_to
            
        let pubmed_relative_results =
            phrases
            |>Read_pubmed_keywords.pubmed_relative_popularities
            |>Plot.papers_to_words
            |>Plot.only_needed_years_for_popularities year_from year_to

        let ngram_chart =
            ngram_results
            |>Plot_from_map.popularity_chart "(Literature)" 1
            |>Chart.withLineStyle (
                Dash = StyleParam.DrawingStyle.Dash
            )
            
        let pubmed_coefficient =
            find_pubmed_coefficient ngram_results pubmed_relative_results
        
        let pubmed_chart =
            pubmed_relative_results
            |>Plot_from_map.popularity_chart "(pubmed relative)" pubmed_coefficient
            |>Chart.withLineStyle (
                Width = 1.0
                //Dash = StyleParam.DrawingStyle.Dash
            )
            
        [ngram_chart; pubmed_chart]
        |>Chart.combine
        |>add_descriptions
   
   
    let pubmed_vs_PMC_absolute_chart
        (phrases: string list)
        =
        let pubmed_absolute_chart =
            phrases
            |>List.map Read_pubmed_keywords.from_esperr
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot_from_map.popularity_chart "(pubmed absolute)" 1
        
        let PMC_absolute_chart =
            phrases
            |>List.map (fun phrase ->
                phrase,
                Read_PMC_keywords.from_PMC_all_pages phrase
            )
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot_from_map.popularity_chart "(PMC absolute)" 1
            |>Chart.withLineStyle (
                Width = 3.0
                //Dash = StyleParam.DrawingStyle.Dash
            )
            
        [pubmed_absolute_chart;PMC_absolute_chart]
        |>Chart.combine
        |>add_descriptions
        
    let pubmed_vs_PMC_absolute_vs_ngram_chart
        (phrases: string list)
        =
        let pubmed_absolute_chart =
            phrases
            |>List.map Read_pubmed_keywords.from_esperr
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot_from_map.popularity_chart "(pubmed absolute)" 1
            
        
        let PMC_absolute_chart =
            phrases
            |>List.map (fun phrase ->
                phrase,
                Read_PMC_keywords.from_PMC_all_pages phrase
            )
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot_from_map.popularity_chart "(PMC absolute)" 1
            |>Chart.withLineStyle (
                Width = 3.0
                //Dash = StyleParam.DrawingStyle.Dash
            )
        
        let ngram_query =
            phrases
            |>Read_google_ngram.modern_data_query
        let ngram_chart =
            ngram_query
            |>Read_google_ngram.phrases_popularity
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot_from_map.popularity_chart "(Literature)" 1
            |>Chart.withLineStyle(
                Dash=StyleParam.DrawingStyle.Dash
            )
            
        [pubmed_absolute_chart;PMC_absolute_chart; ngram_chart]
        |>Chart.combine
        |>add_descriptions
        
    let pubmed_vs_PMC_relative_vs_ngram_chart
        (find_pubmed_coefficient)
        (phrases: string list)
        =

        let ngram_query =
            phrases
            |>Read_google_ngram.max_data_query
        let ngram_results =
            ngram_query
            |>Read_google_ngram.phrases_popularity
            |>Plot.only_needed_years_for_popularities year_from year_to
        
        let ngram_chart =
            ngram_results
            |>Plot_from_map.popularity_chart $"({ngram_chart_name})" 1
            |>Chart.withLineStyle(
                Dash=StyleParam.DrawingStyle.Dash
            )
        
        
        let total_pubmed_papers =
            Read_pubmed_keywords.total_articles_on_pubmed ()
        
        let pubmed_relative_results =
            phrases
            |>List.map Read_pubmed_keywords.from_esperr
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot.absolute_popularity_to_relative total_pubmed_papers
            |>Plot.papers_to_words
        
        let pubmed_coefficient =
            find_pubmed_coefficient ngram_results pubmed_relative_results
            
        let pubmed_chart =
            pubmed_relative_results
            |>Plot_from_map.popularity_chart $"({pubmed_chart_name})" pubmed_coefficient
            

        let PMC_relative_results =
            phrases
            |>List.map (fun phrase ->
                phrase,
                Read_PMC_keywords.from_PMC_all_pages phrase
            )
            |>Plot.only_needed_years_for_popularities year_from year_to
            |>Plot.absolute_popularity_to_relative total_pubmed_papers
            |>Plot.papers_to_words
        
        let PMC_chart =
            PMC_relative_results
            |>Plot_from_map.popularity_chart $"({PMC_chart_name})" pubmed_coefficient
            |>Chart.withLineStyle (
                Width = 3.0
                //Dash = StyleParam.DrawingStyle.Dash
            )
        
            
        [pubmed_chart;PMC_chart; ngram_chart]
        |>Chart.combine
        |>add_descriptions
        // |>Chart.withLayout(Layout.init(
        //     title = Title.init("test"),
        //     hoverLabel = HoverLabel.init(
        //         nameLength = -1    
        //     )    
        // ))
        
    