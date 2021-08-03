using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CommonTools
{
    public class CodeGenerationUtilities
    {
        public object Eval(string sCSCode)
        {
            CSharpCodeProvider c = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("system.windows.forms.dll");
            cp.ReferencedAssemblies.Add("system.drawing.dll");

            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;
            StringBuilder sb = new StringBuilder("");

            sb.Append("using System;\n");
            sb.Append("using System.Xml;\n");
            sb.Append("using System.Data;\n");
            sb.Append("using System.Data.SqlClient;\n");
            sb.Append("using System.Windows.Forms;\n");
            sb.Append("using System.Drawing;\n");

            sb.Append("namespace CSCodeEvaler{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("public object EvalCode(){\n");
            sb.Append("return " + sCSCode + "; \n");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");

            CompilerResults cr = c.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.Count > 0)
            {
                MessageBox.Show("ERROR: " + cr.Errors[0].ErrorText, "Error evaluating cs code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            System.Reflection.Assembly a = cr.CompiledAssembly;
            object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");
            Type t = o.GetType();
            MethodInfo mi = t.GetMethod("EvalCode");
            object s = mi.Invoke(o, null);
            return s;
        }

        public object EvalWithParam(string sCSCode, object oParam)
        {
            CSharpCodeProvider c = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("system.windows.forms.dll");
            cp.ReferencedAssemblies.Add("system.drawing.dll");
            //cp.ReferencedAssemblies.Add("EvalCSCode.exe");

            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;
            StringBuilder sb = new StringBuilder("");

            sb.Append("using System;\n");
            sb.Append("using System.Xml;\n");
            sb.Append("using System.Data;\n");
            sb.Append("using System.Data.SqlClient;\n");
            sb.Append("using System.Windows.Forms;\n");
            sb.Append("using System.Drawing;\n");
            //sb.Append("using EvalCSCode;\n");

            sb.Append("namespace CSCodeEvaler{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("public object EvalCode(object oParam){\n");
            sb.Append("MessageBox.Show(oParam.ToString()); \n");
            sb.Append("return " + sCSCode + "; \n");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");
            //Debug.WriteLine(sb.ToString())// ' look at this to debug your eval string

            CompilerResults cr = c.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.Count > 0)
            {
                MessageBox.Show("ERROR: " + cr.Errors[0].ErrorText, "Error evaluating cs code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            System.Reflection.Assembly a = cr.CompiledAssembly;
            object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");
            Type t = o.GetType();
            MethodInfo mi = t.GetMethod("EvalCode");
            object[] oParams = new object[1];
            oParams[0] = oParam;
            object s = mi.Invoke(o, oParams);
            return s;
        }

        public object EvalWithRef(string sCSCode)
        {
            CSharpCodeProvider c = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("system.windows.forms.dll");
            cp.ReferencedAssemblies.Add("system.drawing.dll");
            cp.ReferencedAssemblies.Add("EvalCSCode.exe");
            cp.ReferencedAssemblies.Add("JasHandExperiment.Configuration.dll");

            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;
            StringBuilder sb = new StringBuilder("");

            sb.Append("using System;\n");
            sb.Append("using System.Xml;\n");
            sb.Append("using System.Data;\n");
            sb.Append("using System.Data.SqlClient;\n");
            sb.Append("using System.Windows.Forms;\n");
            sb.Append("using System.Drawing;\n");
            sb.Append("using EvalCSCode;\n");

            sb.Append("namespace CSCodeEvaler{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("public object EvalCode(){\n");
            sb.Append("object o = new object(); \n");
            sb.Append(sCSCode + " \n");
            sb.Append("return o; \n");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");
            //Debug.WriteLine(sb.ToString())// ' look at this to debug your eval string

            CompilerResults cr = c.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.Count > 0)
            {
                MessageBox.Show("ERROR: " + cr.Errors[0].ErrorText, "Error evaluating cs code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            System.Reflection.Assembly a = cr.CompiledAssembly;
            object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");
            Type t = o.GetType();
            MethodInfo mi = t.GetMethod("EvalCode");
            object s = mi.Invoke(o, null);
            string aa = s.ToString();
            return s;
        }

    }
}
