using System;

using Grasshopper.Kernel;

using Python.Runtime;

namespace Pythonhopper
{
    public class PythonhopperComponent : GH_Component
    {
        public PythonhopperComponent()
          : base("pythonhopper Component", "Nickname",
            "Description of component",
            "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Python Code", "Python Code", "Python Code", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pythonCode = string.Empty;
            double result = 0;
            if(!DA.GetData(0, ref pythonCode)) { return; }

            string envPath = "Path/to/PythonXXX.dll";
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", envPath, EnvironmentVariableTarget.Process);
            
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                PyModule ps = Py.CreateScope();
                ps.Exec(pythonCode);
                result = ps.Get<double>("result");
            }
            PythonEngine.Shutdown();

            DA.SetData(0, result);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("E3C14DA2-8EF7-4BBE-8231-9D8F61A8CF58");
    }
}