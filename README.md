# GH-Pythonnet

## Introduction

In contrast to C#, Python has a rich library for machine learning and numerical computation, which we would like to handle in C#.
Therefore, we will introduce how to use Pythonnet, which allows you to call Python from C#.

This technology is one of the core technologies to realize Tunny, an optimization component we recently released.

Please refer to the code in this repository.

## About Pythonnet

Pythonnet is available on GitHub.

- https://github.com/pythonnet/pythonnet

The README reads as follows

> Python.NET is a package that gives Python programmers nearly seamless integration with the .NET Common Language Runtime (CLR) and provides a powerful application scripting tool for .NET developers. It allows Python code to interact with the CLR, and may also be used to embed Python into a .NET application.

As written, the purpose of the library is to connect seamlessly, and this is what is actually achieved. RhinoInside CPython, RhinoCode, and Dynamo's Python scripting capabilities also use this library.

The feature is that it runs real Python, not pseudo. Please note that Pythonnet does not work by itself, but requires the Python runtime itself in order for this library to function.

### Run Numpy in C#

The Pythonnet README contains an example of running Numpy, so check to see if you can run it.
First, install numpy on the python you want to use.

```python
python -m pip install numpy
```

Once the Python environment is ready, create a C# console application.

If you are using the dotnet cli, here is what you need to do

```
dotnet new console
```

Once csproj has been created, install Pythonnet's nuget package. Be careful with the version since we are using a **pre-release** version here. This article uses 3.0.0-preview2022-04-11.

First, you must pass through the Python path to the Python you want to use, where XXX in pythonXXX.dll is the Python version. For Python 3.10, this would be Python310.dll.

```cs
using System;

class Program
{
    static void Main()
    {
        string envPath = @"Path\to\pythonXXX.dll";
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", envPath, EnvironmentVariableTarget.Process);
    }
}
```

The contents here set the path to the PYTHONNET_PYDLL environment variable only during the execution process. If you use it only in your environment, you can set your own environment variable directly.

Next, we will write the part that executes Numpy.

```cs
PythonEngine.Initialize();
using (Py.GIL())
{
    dynamic np = Py.Import("numpy");
    Console.WriteLine(np.cos(np.pi * 2));

    dynamic sin = np.sin;
    Console.WriteLine(sin(5));

    double c = (double)(np.cos(5) + sin(5));
    Console.WriteLine(c);

    dynamic a = np.array(new List<float> { 1, 2, 3 });
    Console.WriteLine(a.dtype);

    dynamic b = np.array(new List<float> { 6, 5, 4 }, dtype: np.int32);
    Console.WriteLine(b.dtype);

    Console.WriteLine(a * b);
}
PythonEngine.Shutdown();
```

If this is done without problems, the following output is shown in the console.

```
1.0
-0.9589242746631385
-0.6752620891999122
float64
int32
[ 6. 10. 12.]
```

Here is an example of a simple Pythonnet run. You could call Python libraries from C# very seamlessly.
Note that code hints do not work, although this may seem obvious.

### Run any python code

In the above, we have run Python in a relatively code-like style, but it can also be run as if it were a Python console.

For example The following example creates a scope, creates a variable called a in it, and evaluates a computation that adds 2 to it, which is displayed as 3 in the console.

```cs
using (Py.GIL())
{
    PyModule ps = Py.CreateScope();
    ps.Set("a", 1);
    var result = ps.Eval<int>("a + 2");
    Console.WriteLine(result);
}
// 3
```

The following is an example of defining two variables and performing a calculation using them. The result is 113.

```cs
using (Py.GIL())
{
    PyModule ps = Py.CreateScope();
    ps.Set("bb", 100);
    ps.Set("cc", 10);
    ps.Exec("aa = bb + cc + 3");
    var result2 = ps.Get<int>("aa");
    Console.WriteLine(result2);
}
// 113
```

Functions can be defined and scopes can be nested.

```cs
using (Py.GIL())
{
    PyModule ps = Py.CreateScope();
    ps.Set("bb", 100);
    ps.Set("cc", 10);
    ps.Exec(
        "def func1():\n" +
        "    return cc + bb\n"
    );

    using (PyModule scope = ps.NewScope())
    {
        scope.Exec(
            "def func2():\n" +
            "    return func1() - cc - bb\n"
        );
        dynamic func2 = scope.Get("func2");

        var result31 = func2().As<int>();
        Console.WriteLine(result31); // 0

        scope.Set("cc", 20);
        var result32 = func2().As<int>();
        Console.WriteLine(result32); //-10
        scope.Set("cc", 10);

        ps.Set("cc", 20);
        var result33 = func2().As<int>();
        Console.WriteLine(result33); //10
    }
}
```

It is also possible to define class.

```cs
using (Py.GIL())
{
    PyModule ps = Py.CreateScope();
    dynamic ps2 = ps;
    ps2.bb = 100;
    ps.Exec(
        "class Class1():\n" +
        "    def __init__(self, value):\n" +
        "        self.value = value\n" +
        "    def call(self, arg):\n" +
        "        return self.value + bb + arg\n" +
        "    def update(self, arg):\n" +
        "        global bb\n" +
        "        bb = self.value + arg\n"
    );
    dynamic obj1 = ps2.Class1(20);
    var result41 = obj1.call(10).As<int>();
    Console.WriteLine(result41); //130

    obj1.update(10);
    var result42 = ps.Get<int>("bb");
    Console.WriteLine(result42); //30
}
```

As you can see, if you enter a string of proper Python code, Pythonnet will perform the operation. With this in mind, let's create a Grasshopper component that takes Python code text as input.

## Creating Python executable component

From what we have seen so far, it will be easy to make. We will skip the basics of creating Grasshopper components and introduce only the necessary parts.

First, we want the input to be Python code, i.e., text, so here it is

```cs
protected override void RegisterInputParams(GH_InputParamManager pManager)
{
    pManager.AddTextParameter("Python Code", "Python Code", "Python Code", GH_ParamAccess.item);
}
```

The output should be Number, since we want it to be the result of a numerical calculation.

```cs
protected override void RegisterOutputParams(GH_OutputParamManager pManager)
{
    pManager.AddNumberParameter("Output", "Output", "Output", GH_ParamAccess.item);
}
```

SolveInstance is below, taking into account what we did above.

```cs
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
```

Since we are outputting the variable name "result" as a double, the input Python code must also contain the variable "result".

If you build the code here, you should be able to create a component that executes the input code as shown below.

![pygh](https://user-images.githubusercontent.com/23289252/163717982-72228839-5528-48a2-bea6-cf15d29fbbf9.gif)
