
namespace moth

open Plotly.NET

module Plot_from_map =
    
    
    
    let inline popularity_chart<
        'Amount
            when 'Amount: (static member (*): 'Amount * 'Amount -> 'Amount )
            and 'Amount :> System.IConvertible
        >   
        marker
        (coefficient: 'Amount) // to equalize it with google ngrams
        (popularities: (string*Map<int, 'Amount>) list)
           
        =
        popularities
        |>List.map(fun (phrase,year_to_amounts) ->
            let years, amounts =
                year_to_amounts
                |>Map.toList|>List.unzip
            
            let amounts = [ for amount in amounts -> amount * coefficient ]
            
            Chart.Line(
                years,
                amounts,
                Name = phrase + $" {marker}",
                Text = phrase + $" {marker}")
            |>Chart.withLineStyle(
                Color = Plot.phrase_color phrase    
            )
        )
        |>Chart.combine
        
    
    let highest_popularity_value (popularities: (string*Map<int, float>) list) =
        let highest_popularity =
            popularities
            |>List.maxBy(fun year_to_amount ->
               year_to_amount
               |>snd
               |>Seq.map _.Value|>Seq.max
            )
        highest_popularity
        |>snd
        |>Seq.map _.Value|>Seq.max