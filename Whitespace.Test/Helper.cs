using System;
using System.Collections.Generic;
using Whitespace;

namespace Whitespace.Test
{    
    //functional programming helper
    static class FP
    {
        public static IEnumerable<int> Generate(int start, int end)
        {
            for (int i = start; i < end; ++i)
                yield return i;
        }
        
        public static IEnumerable<T> Enumerate<T>(params T[] xs)
        {
            foreach (var x in xs)
                yield return x;
        }
        
        public static IEnumerable<T> Concat<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (var x in first)
                yield return x;
            foreach (var x in second)
                yield return x;
        }
        
        public static IEnumerable<U> Map<T, U>(IEnumerable<T> xs, Func<T, U> f)
        {
            foreach (var x in xs)
                yield return f(x);
        }
    }
    
    static class AstFactory
    {
        public static Ast.Mnemonic LabelDef(int id)
        {
            return Ast.Mnemonic.NewLabelDef(id);
        }
        
        public static Ast.Mnemonic Push(int x)
        {
            return Ast.Mnemonic.NewPush(x);
        }
        
        public static Ast.Mnemonic Dup()
        {
            return Ast.Mnemonic.Dup;
        }
        
        public static Ast.Mnemonic Copy(int x)
        {
            return Ast.Mnemonic.NewCopy(x);
        }
        
        public static IEnumerable<Ast.Mnemonic> Code(params Ast.Mnemonic[] mnemonics)
        {
            return mnemonics;
        }
    }
}