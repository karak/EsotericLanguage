#light
module Brainfuck.Parser

open FParsec.Primitives
open FParsec.CharParsers
open Ast

let rec private foldNodeCreateFunction(fs: (Node->Node) list): Node->Node =
    if fs.Tail.IsEmpty then
        fs.Head
    else
        let tailFolded = foldNodeCreateFunction fs.Tail
        fun(next) -> fs.Head (tailFolded next)

let parse(text:string) =
    let pMeaningless = noneOf "<>+-,.[]" |>> ignore <?> "meaningless character"
    let ws = many pMeaningless |>> ignore
    let ch c = skipChar c .>> ws //helper for char parser eat whitespace
    let pMoveRight = ch '>' |>> fun() -> Right
    let pMoveLeft = ch '<' |>> fun() -> Left
    let pIncrement = ch '+' |>> fun() -> Increment
    let pDecrement = ch '-' |>> fun() -> Decrement
    let pCharOut = ch '.' |>> fun() -> CharOut
    let pCharIn = ch ',' |>> fun() -> CharIn
    let pLBracket = ch '['
    let pRBracket = ch ']'
    
    let pExpr, pExprRef = createParserForwardedToRef()    //escape recursion
    let pDirective = choice [pMoveRight; pMoveLeft; pIncrement; pDecrement; pCharOut; pCharIn]
    let pDirectiveSeq = 
        parse {let! ds = many1 pDirective
               return fun(next: Node) -> DirectiveSequenceNode(ds, next) :> Node}
    let ParenthesizedExpr = 
        parse {let! expr = (pLBracket >>. pExpr .>> pRBracket)
               return fun(next: Node) ->ConditionalPairNode(expr, next) :> Node}
    let pFactor = pDirectiveSeq <|> ParenthesizedExpr
    pExprRef := many1 pFactor |>> foldNodeCreateFunction
    let pCode = ws >>. pExpr |>> fun(createNode) -> createNode theTerminaterNode
    
    run pCode text

