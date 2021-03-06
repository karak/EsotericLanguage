#light
module Whitespace.Parser

open FParsec.Primitives
open FParsec.CharParsers
open Ast

let private negate(x) = -x
let private binaryToDecimal(bits: int list) =
    let mutable result = 0
    for b in bits do
        result <- ((result <<< 1) ||| b)
    result

let mnemonicGenerator(m: Mnemonic) = fun(_) -> m

//tokens
let pSpace = pchar ' ' <?> "space"
let pLf = pchar '\n' <?> "LF"
let pTab = pchar '\t' <?> "Tab"

//stack manip ... IMP space
let pBit     = (pSpace |>> fun(_) -> 0) <|> (pTab |>> fun(_) -> 1) <?> "bit"
let pBinary  = many1 pBit |>> binaryToDecimal <?> "binary"
let pPositiveNumberOrZero = pSpace >>. pBinary .>> pLf <?> "positive-number-or-zero"
let pNegativeNumber = pTab >>. pBinary .>> pLf |>> negate <?> "negative-number"
let pNumber = pPositiveNumberOrZero <|> pNegativeNumber <?> "number"
let pPush    = (pSpace >>. pSpace) >>. pNumber |>> Push <?> "push"
let pDup     = pSpace >>. pLf >>. pSpace |>> mnemonicGenerator Dup <?> "dup"
let pCopy    = (pSpace >>. pTab >>. pSpace) >>. pNumber |>> Copy <?> "copy"
let pSwap    = pSpace >>. pLf >>. pTab |>> mnemonicGenerator Swap
let pDiscard = pSpace >>. pLf >>. pLf |>> mnemonicGenerator Discard
let pSlide   = (pSpace >>. pTab >>. pLf) >>. pNumber |>> Slide

//arithmetic op ... IMP tab space
let pAdd = pTab >>. pSpace >>. pSpace >>. pSpace |>> mnemonicGenerator Add <?> "add"
let pSub = pTab >>. pSpace >>. pSpace >>. pTab |>> mnemonicGenerator Sub <?> "sub"
let pMul = pTab >>. pSpace >>. pSpace >>. pLf |>> mnemonicGenerator Mul <?> "mul"
let pDiv = pTab >>. pSpace >>. pTab >>. pSpace |>> mnemonicGenerator Div <?> "div"
let pMod = pTab >>. pSpace >>. pTab >>. pTab |>> mnemonicGenerator Mod <?> "mod"

//heap access ... IMP tab tab
let pHeapWrite = pTab >>. pTab >>. pSpace |>> mnemonicGenerator HeapWrite <?> "heap-write"
let pHeapRead = pTab >>. pTab >>. pTab |>> mnemonicGenerator HeapRead <?> "heap-read"

//flow control ... IMP LF
let pLabel = pNumber <?> "label"
let pLabelDef = (pLf >>. pSpace >>. pSpace) >>. pLabel |>> LabelDef <?> "label-definition"
let pCall = (pLf >>. pSpace >>. pTab) >>. pLabel |>> Call <?> "call"
let pJump = (pLf >>. pSpace >>. pLf) >>. pLabel |>> Jump <?> "jump"
let pJumpZero = (pLf >>. pTab >>. pSpace) >>. pLabel |>> JumpZero <?> "jump-zero"
let pJumpNega = (pLf >>. pTab >>. pTab) >>. pLabel |>> JumpNega <?> "jump-nega"
let pReturn = pLf >>. pTab >>. pLf |>> mnemonicGenerator Return <?> "return"
let pExit = pLf >>. pLf >>. pLf |>> mnemonicGenerator Exit <?> "exit"

//io op ... IMP tab LF
let pCharOut = pTab >>. pLf >>. pSpace >>. pSpace |>> mnemonicGenerator CharOut <?> "char-out"
let pNumOut = pTab >>. pLf >>. pSpace >>. pTab |>> mnemonicGenerator NumOut <?> "num-out"
let pCharIn = pTab >>. pLf >>. pTab >>. pSpace |>> mnemonicGenerator CharIn <?> "char-in"
let pNumIn = pTab >>. pLf >>. pTab >>. pTab |>> mnemonicGenerator NumIn <?> "num-in"

let pMnemonic = choice[pPush ; pDup ; pCopy ; pSwap ; pDiscard ; pSlide ;
                      pAdd ; pSub ; pMul ; pDiv ; pMod ;
                      pHeapWrite ; pHeapRead ;
                      pLabelDef ; pCall ; pJump ; pJumpZero ; pJumpNega; pReturn ; pExit ;
                      pCharOut ; pNumOut ; pCharIn ; pNumIn ]

let pCode = many pMnemonic

let compile(text: string) = run pCode text
