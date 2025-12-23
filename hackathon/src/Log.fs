namespace moth
open System
open Serilog

module Log =
    
    let logger =
        LoggerConfiguration().
            WriteTo.File(
                "..log/log.txt",
                rollOnFileSizeLimit = true,
                fileSizeLimitBytes = 1000000000
            ).CreateLogger()

    let info = logger.Information
    let debug = logger.Debug
    let important message =
        logger.Information message
        Console.WriteLine message