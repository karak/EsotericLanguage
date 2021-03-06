#light
module Brainfuck.Interpreter

open FParsec.CharParsers    //for parse and ParseResult
open Parser
open Kernel

//run testing
type ConsoleIO = class
    new() = {}
    interface IO with
        member this.Put(b: byte) = System.Console.Write (char b)
        member this.Get() = byte (System.Console.Read())
    end
end

let executeWithIO(text: string, io: IO): int = 
    let result = parse text
    match result with
    |Success(ast,_,_) -> VirtualMachine(io).Eval(ast); 0
    |Failure(msg, err, _) -> printfn "Parse Failure!\n%s" msg; 1

let execute(text: string) = executeWithIO(text, ConsoleIO())

[<EntryPoint>]
let main args =
    if args.Length <> 1 then
        printfn "run with 1 argument(source code)"
        1
    else
        let retval = execute args.[0]
        ignore(System.Console.Read())
        retval