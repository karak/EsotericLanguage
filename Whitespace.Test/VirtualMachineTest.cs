using System;
using System.Text;
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
            var doubleLabel = AstFactory.Code(AstFactory.LabelDef(0), AstFactory.LabelDef(0));
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
                _vm.Eval(AstFactory.Code(AstFactory.Push(expected)));
                Assert.That(_stack.Top, Is.EqualTo(expected));
            }
        }
        
        [Test]
        public void ThrowExceptionIfDupEmptyStack()
        {
            Assert.That(delegate() { _vm.Eval(AstFactory.Code(AstFactory.Dup())); }, Throws.Exception.TypeOf(typeof (Kernel.StackAccessViolation)));
        }
        
        [Test]
        public void TopIsDuplicatedAfterDup()
        {
            const int item = 1;
            _vm.Eval(AstFactory.Code(AstFactory.Push(item), AstFactory.Dup()));
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
            Assert.That(_stack.Top, Is.EqualTo(0));
        }
    }
}
