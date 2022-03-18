using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace gh_pythonnet
{
  public class gh_pythonnetInfo : GH_AssemblyInfo
  {
    public override string Name => "gh-pythonnet Info";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("72ADA423-C4A4-44A3-A5E3-CF3B050C8FAA");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";
  }
}