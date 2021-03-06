#light
module Brainfuck.Ast

type Directive = Right | Left | Increment |Decrement | CharIn | CharOut

type INodeVisitor = interface
    abstract Current: unit->int
end

type Node = interface
    abstract Visit: INodeVisitor->seq<Directive>
end

type private TerminaterNode = class
    new() = {}
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> = seq<Directive> []
    end
end

let theTerminaterNode = TerminaterNode() :> Node

type DirectiveSequenceNode = class
    val _directives: seq<Directive>
    val _next: Node
    new (directives: Directive list, next: Node) = {
        _directives = directives
        _next = next
    }
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> = n._directives
    end
end

[<AbstractClass>]
type private ConditionalNode = class
    new() = {}
    
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> =
            if v.Current() <> 0 then
                n.Body.Visit(v)
            else
                n.Mate.Body.Visit(v)
    end
    
    abstract Body: Node with get
    abstract Mate: ConditionalNode with get
end

type private RightConditionalNode = class
    inherit ConditionalNode
    
    new(body, mate) = {
        _body = body
        _mate = mate
    }
    
    override n.Body with get() = n._body
    override n.Mate with get() = n._mate
    
    val _body: Node
    val _mate: ConditionalNode    //to avoid cyclic dependency on initialization
end

type private StubConditionalNode = class
    inherit ConditionalNode
    new () = {}
    override n.Body with get() = theTerminaterNode
    override n.Mate with get() = n :> ConditionalNode
end

let private stubConditionalNode = StubConditionalNode() :> ConditionalNode

type private LeftConditionalNode = class
    inherit ConditionalNode
    
    new() = {
        _body = theTerminaterNode
        _mate = stubConditionalNode
    }
    
    member n.Reset(createBody: Node->Node, mate) =
        n._body <- (createBody mate)
        n._mate <- mate
    
    override n.Body with get() = n._body
    override n.Mate with get() = n._mate
    
    val mutable _body: Node
    val mutable _mate: ConditionalNode
end


type ConditionalPairNode = class
    val _leftConditional: Node
    new (createBody: Node->Node, next: Node) =
        let placeHolder = stubConditionalNode
        let lCond = LeftConditionalNode()
        let rCond = RightConditionalNode(next, lCond)
        lCond.Reset(createBody, rCond)
        { _leftConditional = lCond }
        
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> = n._leftConditional.Visit v
    end
end
