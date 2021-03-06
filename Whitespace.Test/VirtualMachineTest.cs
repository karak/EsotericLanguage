using System;
using System.Text;
using System.Collections.Generic;
using Whitespace;
using NUnit.Framework;

namespace Whitespace.Test
{
    class StubIO : Kernel.IO
    {
        public char GetChar()
        {
            throw new NotImplementedException("no expected call: IO.GetChar()");
        }
        public int GetInt()
        {
            throw new NotImplementedException("no expected call: IO.GetInt()");
        }

        public void PutChar(char c)
        {
            _printed.Append(c);
        }

        public void PutInt(int i)
        {
            _printed.Append(i);
        }

        public string Printed
        {
            get { return _printed.ToString(); }
        }
        StringBuilder _printed = new StringBuilder();
    }
    
    [TestFixture]
    public class VirtualMachineTest
    {
        Kernel.Stack _stack;
        StubIO _io;
        Kernel.VirtualMachine _vm;

        [SetUp]
        public void SetUp()
        {
            _stack = new Kernel.StackImpl();
            _io = new StubIO();
            _vm = new Kernel.VirtualMachine(_io, _stack);
        }

        //FlowControlTests
        [Test]
        public void ThrowExceptionIfLabelIsDupulicated()
        {
            var doubleLabel = FP.Enumerate(AstFactory.LabelDef(0), AstFactory.LabelDef(0));
            Assert.That(delegate() { _vm.Eval(doubleLabel); }, Throws.Exception.TypeOf(typeof(Kernel.LabelDuplication)));
        }

        //StackManipTests
        [Test]
        public void InitialStackIsEmpty()
        {
            Assert.That(_stack._IsAccessible(0), Is.False);
        }
        
        [Test]
        public void TopIsEqualToWhatIsPushed()
        {
            foreach (var expected in FP.Generate(0, 10))
            {
                _vm.Eval(FP.Enumerate(AstFactory.Push(expected)));
                Assert.That(_stack[0], Is.EqualTo(expected));
            }
        }
        
        [Test]
        public void OrderIsReversibleToThatOfPush()
        {
            const int n = 10;
            var numbers = FP.Generate(0, n);
            _vm.Eval(PushAll(numbers));
            foreach (int i in numbers)
            {
                var pos = n - 1 - i;
                Assert.That(_stack[pos], Is.EqualTo(i), String.Format("miss at {0}", pos));
            }
        }
        
        [Test]
        public void ThrowExceptionIfDupEmptyStack()
        {
            Assert.That(delegate() { _vm.Eval(FP.Enumerate(AstFactory.Dup())); }, Throws.Exception.TypeOf(typeof (Kernel.StackAccessViolation)));
        }
        
        [Test]
        public void TopIsDuplicatedAfterDup()
        {
            const int item = 1;
            _vm.Eval(FP.Enumerate(AstFactory.Push(item), AstFactory.Dup()));
            Assert.That(_stack[0], Is.EqualTo(item));
            Assert.That(_stack[1], Is.EqualTo(item));
        }
        
        [Test]
        public void NthItemIsDuplicatedAfterCopy()
        {
            const int n = 10;
            var push0ToN = FP.Map<int, Ast.Mnemonic>(FP.Generate(0, n), AstFactory.Push);
            var copyBottom = FP.Enumerate(AstFactory.Copy(n-1));
            _vm.Eval(FP.Concat(push0ToN, copyBottom));
            Assert.That(_stack[0], Is.EqualTo(0));
        }
        
        [Test]
        public void Top2AreSwappedAfterSwap()
        {
            const int first = 0;
            const int second = 1;
            const int third = 2;
            _vm.Eval(FP.Enumerate(AstFactory.Push(first), AstFactory.Push(third), AstFactory.Push(second), AstFactory.Swap()));
            Assert.That(_stack[0], Is.EqualTo(third) , "0 is invalid!");
            Assert.That(_stack[1], Is.EqualTo(second), "1 is invalid!");
            Assert.That(_stack[2], Is.EqualTo(first) , "2 is invalid!");
        }
        
        [Test]
        public void TopIsRemovedAfterDiscard()
        {
            const int first = 0;
            const int second = 1;
            _vm.Eval(FP.Enumerate(AstFactory.Push(first), AstFactory.Push(second), AstFactory.Discard()));
            Assert.That(_stack._IsAccessible(1), Is.False);
            Assert.That(_stack[0], Is.EqualTo(first));
        }
        
        [Test]
        public void NthIsRemovedAfterSlide()
        {
            var numbers = new int[]{1, 2, 3, 4, 5};
            var removeIndex = 2;
            var expected = new int[]{5, 4, 2, 1};
            _vm.Eval(FP.Concat(PushAll(numbers), FP.Enumerate(AstFactory.Slide(removeIndex))));
            
            Assert.That(_stack._Count, Is.EqualTo(expected.Length));
            for (int i = 0; i < _stack._Count; i++)
                Assert.That(_stack[i], Is.EqualTo(expected[i]));
        }
        
        public IEnumerable<Ast.Mnemonic> PushAll(IEnumerable<int> numbers)
        {
            return FP.Map<int, Ast.Mnemonic>(numbers, AstFactory.Push);
        }
    }
}
