using System;
using System.Collections.Generic;

using Python.Runtime;

namespace ConsolePythonnet
{
    class Program
    {
        static void Main()
        {
            string envPath = "Path/to/PythonXXX.dll";
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", envPath, EnvironmentVariableTarget.Process);

            RunNumpy();
            RunModules();
        }

        private static void RunNumpy()
        {
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
        }

        private static void RunModules()
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                PyModule ps = Py.CreateScope();

                //---------------------------------------
                ps.Set("a", 1);
                var result1 = ps.Eval<int>("a + 2");
                Console.WriteLine(result1);

                //---------------------------------------
                ps.Set("bb", 100);
                ps.Set("cc", 10);
                ps.Exec("aa = bb + cc + 3");
                var result2 = ps.Get<int>("aa");
                Console.WriteLine(result2);

                //---------------------------------------
                ps.Set("bb", 100);
                ps.Set("cc", 10);
                ps.Exec(
                    "def func1():\n" +
                    "    return cc + bb\n");

                using (PyModule scope = ps.NewScope())
                {
                    scope.Exec(
                        "def func2():\n" +
                        "    return func1() - cc - bb\n");
                    dynamic func2 = scope.Get("func2");

                    var result31 = func2().As<int>();
                    Console.WriteLine(result31);

                    scope.Set("cc", 20);
                    var result32 = func2().As<int>();
                    Console.WriteLine(result32);
                    scope.Set("cc", 10);

                    ps.Set("cc", 20);
                    var result33 = func2().As<int>();
                    Console.WriteLine(result33);
                }

                //---------------------------------------
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
                Console.WriteLine(result41);

                obj1.update(10);
                var result42 = ps.Get<int>("bb");
                Console.WriteLine(result42);
            }
            PythonEngine.Shutdown();
        }
    }
}