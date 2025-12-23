
namespace moth

open Plotly.NET

module Google_ngram_plot = ()
        
    // let highest_ngram_value
    //     (ngram_results: (string*Map<int, float>)) =
    //     let highest_ngram =
    //         ngram_results
    //         |>List.maxBy(fun ngram_result ->
    //             ngram_result.timeseries
    //             |>List.max
    //         )
    //     highest_ngram.timeseries
    //     |>List.max