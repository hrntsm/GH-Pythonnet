using System;
using System.Drawing;

using Grasshopper.Kernel;

namespace Pythonhopper
{
    public class PythonhopperInfo : GH_AssemblyInfo
    {
        public override string Name => "pythonhopper Info";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("0916CD54-F022-4C3A-B15D-5F76DDB2EA60");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}