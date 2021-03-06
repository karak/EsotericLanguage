#light 
module Whitespace.Interpreter

open FParsec.CharParsers
open Parser
open Kernel

let executeWithIO text io =
    match compile text with
    |Success (code, _, _) ->
        let vm = VirtualMachine(io, DefaultStack())
        vm.Eval code
        0
    |Failure (msg, err, _) ->
        1

let execute text = executeWithIO text (ConsoleIO())
