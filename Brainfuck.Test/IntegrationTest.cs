using System;
using System.Text;
using Brainfuck;
using NUnit.Framework;


namespace Brainfuck.Test
{
    class StubIO : Brainfuck.Kernel.IO
    {
        public byte Get()
        {
            throw new NotImplementedException("no expected call: IO.Get()");
        }
        
        public void Put(byte b) {
            _printed.Append((char)b);
        }
        
        public string Printed
        {
            get { return _printed.ToString(); }
        }
        StringBuilder _printed = new StringBuilder();
    }
    
    [TestFixture]
    public class InterpreterTest
    {
        [Test]
        public void PrintASimple()
        {
            TestPrinting("A", "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.");
        }
        
        [Test]
        public void PrintAWithLoop()
        {
            var code = @"+++++
                         [> ++++++++++ < -]
                         > +++++ .";
            TestPrinting("A", code);
        }
        
        [Test]
        public void NeverJumpIfZero()
        {
            TestPrinting(new string(new char[1] {(char)1}), "[+.].");
        }
        
        [Test]
        public void JumpIfNonZero()
        {
            TestPrinting(new string(new char[1] {(char)1}), "+[+.].");
        }
        
        [Test]
        public void PrintHelloWorld()
        {
            TestPrinting("Hello, world!", "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.");
        }
        
        void TestPrinting(string expectedPrinted, string source)
        {
            var result = Execute(source);
            Assert.That(result, Is.EqualTo(expectedPrinted));            
        }
        string Execute(string source)
        {
            var io = new StubIO();
            Interpreter.executeWithIO(source, io);
            return io.Printed;
        }
    }
}