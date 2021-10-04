using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace CodeProviders {
    /*public class RunningCodeEasy {
        public object RunCode(string userCode) {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), userCode);
            Type classType = results.CompiledAssembly.GetType("MyClass");
            System.Reflection.MethodInfo method = classType.GetMethod("MyMethod");

            return method.Invoke(null, null);
        }
    }*/
}
