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

type Stack = interface
    abstract Push: int->unit
    abstract Discard: unit->unit
    abstract Slide: int->unit    
    abstract Item: int->int with get
    abstract _IsAccessible: int->bool
    abstract _Count: int with get
end

type private StackManipulator = class
    new(target) = { _target = target }

    member this.Push = this._target.Push
    member this.Discard = this._target.Discard
    member this.Slide = this._target.Slide
    member this.Dup() = this._target.Push (this.Top)
    member this.Copy i = this._target.Push (this.[i])
    member this.Swap() =
        let top = this.Pop()
        let second = this.Pop()
        this.Push(top)
        this.Push(second)
    
    member private this.Pop() =
        let top = this.Top
        this._target.Discard()
        top
    member private this.Top
        with get() = this.[0]    
    member private this.Item
        with get i =
            if this._target._IsAccessible i then this._target.[i]
            else raise StackAccessViolation    
    
    val _target: Stack
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
    new() = { _data = new List<int>() }
    interface Stack with
        member this.Discard() = this._data.RemoveAt(this.ReverseIndex 0)
        member this.Slide i = this._data.RemoveAt(this.ReverseIndex i);
        member this.Push x = this._data.Add(x)
        member this.Item with get i = this._data.[this.ReverseIndex i]
        member this._IsAccessible i = 0 <= i && i < this._data.Count
        member this._Count with get() = this._data.Count
    end
    
    member this.IsEmpty with get() = this._data.Count = 0
    member this.ReverseIndex i = this._data.Count - 1 - i
    val _data: IList<int>
    //val _data: System.Collections.Generic.Stack<int> ... not found!
end

type DefaultIO = ConsoleIO
type DefaultStack = StackImpl

let private eachWithIndex(xs: array<_>, f) =
    for i in 0..xs.Length-1 do f(xs.[i], i) done

type VirtualMachine = class
    new() = VirtualMachine(DefaultIO(), DefaultStack())
    new(io: IO, stack: Stack) = {_io = io; _stackManipulator = StackManipulator(stack) }
    
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
            let nextWhenIterative = i + 1
            let next = 
                match mnemonicArray.[i] with
                |Push number -> vm._stackManipulator.Push number; nextWhenIterative
                |Dup -> vm._stackManipulator.Dup(); nextWhenIterative
                |Copy index -> vm._stackManipulator.Copy index; nextWhenIterative
                |Swap -> vm._stackManipulator.Swap(); nextWhenIterative
                |Discard -> vm._stackManipulator.Discard(); nextWhenIterative                
                |Slide index -> vm._stackManipulator.Slide(index); nextWhenIterative;
                |_ -> raise (System.NotImplementedException()) //TODO: implementation
            if next < mnemonicArray.Length then doEval next
        
        doEval(0)
    
    val private _io : IO
    val private _stackManipulator: StackManipulator
end


