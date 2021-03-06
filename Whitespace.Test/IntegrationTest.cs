using NUnit.Framework;

namespace Whitespace.Test
{
    [TestFixture]
    public class InterpreterTest
    {
        [Test]
        public void printHelloworld()
        {        
            var code = @"""   	  	   
	
     		  	 	
	
     		 		  
	
     		 		  
	
     		 				
	
     	 		  
	
     	     
	
     			 			
	
     		 				
	
     			  	 
	
     		 		  
	
     		  	  
	
     	    	
	
     	 	 
	
  


";
                TestPrinting("Hello, world!", code);
        }
        
        void TestPrinting(string expectedPrinted, string code)
        {
            var io = new StubIO();
            Interpreter.executeWithIO(code, io);
            Assert.That(io.Printed, Is.EqualTo(expectedPrinted));
        }
    }
}