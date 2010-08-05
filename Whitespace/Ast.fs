#light
module Whitespace.Ast

type Mnemonic =
    Push of int | Dup | Copy of int | Swap | Discard | Slide |
        Add | Sub | Mul | Div | Mod |
        HeapWrite | HeapRead |
        LabelDef of int | Call of int | Jump of int | JumpZero of int | JumpNega of int | Return | Exit |
        CharOut | NumOut| CharIn | NumIn
