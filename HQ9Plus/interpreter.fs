#light 
///HQ9+ interpreter using FParsec
module HQ9Plus.Interpreter

open FParsec.Primitives
open FParsec.CharParsers

type private Counter = class
  val mutable private x:int
  member c.Increment() = c.x <- c.x + 1
  member c.Value with get() = c.x
  new() = { x = 0 }
end

let private capitalize(s:string) : string = 
    //simple version, no care for first spaces
    let toUpper(c: char) = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToUpper c
    let head = toUpper (s.[0])
    let tail = s.[1..]
    (string head) + tail

let private print99BottlesOfBeer() =
    let showBottleCount = function
        |0 -> "no more bottles"
        |1 -> "1 bottle"
        |x -> x.ToString() + " bottles"
    
    let showAction n =
        if n > 0 then "Take one down and pass it around"
        else "Go to the store and buy some more"
    
    let showNextBottleCount n = if n > 0 then showBottleCount (n-1) else showBottleCount 99
    
    for n in [99..-1..0] do
        let bottleCount = showBottleCount n
        printfn "%s of beer on the wall, %s of beer." (capitalize bottleCount) bottleCount
        printfn "%s, %s." (showAction n) (showNextBottleCount n)
        if n <> 0 then printfn ""
    done

let execute(text: string): unit = 
    //grammer
    let theCounter = Counter()
    let pHelloworld= skipChar 'H' |>> fun() -> printfn "Hello, world!"
    let pNinetynineBottlesOfBeer =  skipChar '9' |>> print99BottlesOfBeer
    let pQuine = skipChar 'Q' |>> fun() -> printf "%s" text
    let pIncrement = skipChar '+' |>> theCounter.Increment
    let pMeaningful = choice [pHelloworld; pNinetynineBottlesOfBeer; pQuine; pIncrement]
    let pMeaningless = noneOf "H9Q+" |>> ignore <?> "meaningless character"
    let pCode = (many pMeaningless) >>. (many1 (pMeaningful .>> many pMeaningless))

    //launcher
    let result = run pCode text
    match result with
    |Success(_, _, _) -> ()
    |Failure(msg, err, _) -> printfn "Parse Failure!\n%s" msg


//run testing
execute "9 Q HH ++;"
ignore(System.Console.Read())
