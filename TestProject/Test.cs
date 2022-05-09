using MyDll;

namespace TestProject;

public class Test
{
    public static void TestMethod()
    {
        On.MyDll.Class1.TestMethodToBeDetoured += Class1OnTestMethodToBeDetoured;
    }
    private static bool Class1OnTestMethodToBeDetoured(On.MyDll.Class1.orig_TestMethodToBeDetoured orig, Class1 self)
    {
        throw new NotImplementedException();
    }
}
