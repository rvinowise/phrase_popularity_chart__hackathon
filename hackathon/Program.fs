namespace moth

open Plotly.NET

module Program =
    [<EntryPoint>]
    let main args =
        
        let words = 
            if Array.length args = 0 then
                //my main TH words
                // ["head transplantation"; "superintelligence"; "singularity";
                // "transhumanism"; "cryonics"; "mind uploading";
                // "immortality"; "cyborgization"; "side loading"]
                
                //google's main TH words
                // ["biohacking"; "CRISPR"; "Nanotechnology";
                // "Brain-Computer Interface"; "Posthuman"]
                
                //google's main immortalism words
                // ["Senescence"; "Negligible senescence";"Telomerase"; "Hayflick limit";
                //  "Digital immortality"]
                
                //ASI related words
                // ["Superintelligence"; "Large language model"; "Artifitial intelligence"
                //  "Super artifitial intelligence"; "Brain augmentation"; "Intelligence amplification"]
                // ["superintelligence"; "large language model"; "artifitial intelligence"
                //  "super artifitial intelligence"; "brain augmentation"; "intelligence amplification";
                // "neuroenhancement"; "neurohacking"; "biohacking"]
                
                //["radical life extension"; "posthumanism"; "transhumanism"; "immortalism"
                // "human augmentation"]
                |>Set.ofList|>List.ofSeq
            else
                args|>Set.ofArray|>List.ofSeq
        
        words
        |>Combine_plots.pubmed_vs_PMC_relative_vs_ngram_chart
        //|>Combine_plots.pubmed_vs_ngram_chart
            (Combine_plots.no_coeffieicnt)
            //Combine_plots.coefficient_to_equalize_graphs
        |>Chart.withTemplate ChartTemplates.lightMirrored        
        //|>Chart.withSize(1600,800)
        |>Chart.withSize(1200,600)
        |>Chart.show
        
        0