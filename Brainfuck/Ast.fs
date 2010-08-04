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

type private ConditionalNode = class
    val mutable _trueClause: Node
    val mutable _falseClause: Node
    
    new() = {
        _trueClause = theTerminaterNode
        _falseClause = theTerminaterNode
    }
    
    member n.Reset(trueClause: Node, falseClause: Node) =
        n._trueClause <- trueClause
        n._falseClause <- falseClause
    
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> =
            if v.Current() <> 0 then
                n._trueClause.Visit(v)
            else
                n._falseClause.Visit(v)
    end
end

//
//type ConditionalNode = class //inherit Node
//    new (next: Node, pair: ConditionalNodePair) = {}
//    
type ConditionalPairNode = class
    val _leftConditional: Node
    new (createBody: Node->Node, next: Node) =
        let lCond = ConditionalNode()
        let rCond = ConditionalNode()
        let body = createBody(rCond)
        lCond.Reset(body, rCond)
        rCond.Reset(next, lCond)
        { _leftConditional = lCond }
    interface Node with
        member n.Visit(v: INodeVisitor): seq<Directive> = n._leftConditional.Visit v
    end
end
