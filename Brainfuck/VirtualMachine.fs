#light
module Brainfuck.Kernel

open Brainfuck.Ast

type IO = interface
    abstract Put: byte->unit
    abstract Get: unit-> byte
end

type VirtualMachine = class
    new(io: IO) = { tape = Array.create 30000 (byte 0); ptr = 0; io = io }
    
    member k.Eval(node: Node) =
        for d in node.Visit(k) do
            k.Eval d
            //debug
            //k.Dump(16)
        done
    
    member private k.Eval(d: Directive) =
        match d with
        |Right     -> k.ptr <- k.ptr + 1
        |Left      -> k.ptr <- k.ptr - 1
        |Increment -> k.Current <- k.Current + 1
        |Decrement -> k.Current <- k.Current - 1
        |CharIn    -> k.Current <- int (k.io.Get())
        |CharOut   -> k.io.Put (byte k.Current)
    
    interface INodeVisitor with
        member k.Current(): int = int (k.tape.[k.ptr])
    end
    
    member private k.Current    //access as int for operation without casting
        with get(): int = int (k.tape.[k.ptr])
        and set(x: int) = k.tape.[k.ptr] <- (byte x)
    
    member private k.Dump(n: int) =
        printfn ""
        for i in 0..n-1 do
            printf "%3d" (int k.tape.[i])
            if i = k.ptr then
                printf "!"
            else
                printf ","
        printfn ""
    
    val private tape : array<byte>
    val mutable private ptr : int
    val io: IO
end
