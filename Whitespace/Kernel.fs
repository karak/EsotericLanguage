#light
module Whitespace.Kernel

open System.Collections.Generic
open Ast

exception StackAccessViolation

type IO = interface
    abstract PutChar: char->unit    
    abstract PutInt: int->unit
    abstract GetChar: unit->char
    abstract GetInt: unit->int
end

[<AbstractClass>]
type Stack = class
    new() = {}
    abstract Push: int->unit
    member this.Dup() = this.Push (this.Top)
    member this.Copy i = this.Push (this.[i])
    member this.Top
        with get() = this.[0]
    member this.Item
        with get i =
            if this._IsAccessible i then this._GetItem i
            else raise StackAccessViolation    
    abstract _GetItem: int->int
    abstract _IsAccessible: int->bool
end

exception LabelDuplication of int * int

type private LabelMap = class
    new() = { _labelToIndex = Dictionary<int, int>() }
    
    member this.Item
        with get i = this._labelToIndex.[i]
        and set i x =
            if (this._labelToIndex.ContainsKey i) then raise (LabelDuplication(i, this._labelToIndex.[i]))
            this._labelToIndex.[i] <- x
    
    val _labelToIndex: IDictionary<int, int>
end

type ConsoleIO = class
    new() = {}
    interface IO with
        member this.PutChar(c: char): unit = System.Console.Write c    
        member this.PutInt (i: int):  unit = System.Console.Write i
        member this.GetChar(): char = char (System.Console.Read ())
        member this.GetInt():  int = System.Int32.Parse (System.Console.ReadLine())
    end
end

type StackImpl = class
    inherit Stack
    
    new() = { _data = [] }

    override this.Push x = this._data <- x :: this._data
    override this._GetItem i = this._data.[i]
    override this._IsAccessible i = 0 <= i && i < this._data.Length
    
    member this.IsEmpty with get() = this._data.IsEmpty
    
    val mutable private _data: int list
end

type DefaultIO = ConsoleIO
type DefaultStack = StackImpl

let private eachWithIndex(xs: array<_>, f) =
    for i in 0..xs.Length-1 do f(xs.[i], i) done

type VirtualMachine = class
    new() = VirtualMachine(DefaultIO(), DefaultStack())
    new(io: IO, stack: Stack) = {_io = io; _stack = stack}
    
    member vm.Eval(code: seq<Mnemonic>): unit = 
        let labelMap = LabelMap()
        let mnemonicArray = [| for m in code -> m |]
        
        //preprocess. register label to map
        let registerLabel(m:Mnemonic, i:int) = 
            match m with
            |LabelDef l -> labelMap.[l] <- i
            |_ -> ()
        
        eachWithIndex(mnemonicArray, registerLabel)
        
        let rec doEval(i: int) =
            let next = 
                match mnemonicArray.[i] with
                |Push number -> vm._stack.Push number; i + 1
                |Dup -> vm._stack.Dup(); i + 1
                |Copy index -> vm._stack.Copy index; i + 1
                |_ -> raise (System.NotImplementedException()) //TODO: implementation
            if next < mnemonicArray.Length then doEval next
        
        doEval(0)
    
    val private _io : IO
    val private _stack : Stack
end


