using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Threading.Tasks;
using VerifyCS = Toendering.RoslynAnalyzer.Akka.Test.CSharpAnalyzerVerifier<
    Toendering.RoslynAnalyzer.Akka.ToenderingRoslynAnalyzerAkkaPropsAnalyzer>;

namespace Toendering.RoslynAnalyzer.Akka.Test
{
    [TestClass]
    public class ToenderingRoslynAnalyzerAkkaPropsUnitTest
    {
        ReferenceAssemblies _referenceAssemblies = ReferenceAssemblies.Default
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Akka", "1.4.39")
                ))
            .AddAssemblies(ImmutableArray.Create(
                "Akka"
            ));


        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using Akka.Actor;

class TestActor : ReceiveActor
{
    public TestActor(int affinity, string name, bool b1) {}
    public TestActor(int affinity, string name) { }

    public static Props CreateProps() => 
        {|TRD001:Props.Create<TestActor>(0, false, false)|};
}
";
            var spec = new VerifyCS.Test
            {

                ReferenceAssemblies = _referenceAssemblies,
                TestState =
                {
                    Sources = { test }
                }
            };

            await spec.RunAsync();
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            var test = @"
using Akka.Actor;

class TestActor : ReceiveActor
{
    public TestActor(int affinity, string name, bool b1) {}
    public TestActor(int affinity, string name) { }
    public TestActor(int affinity, bool b0, bool b1) {}

    public static Props CreateProps() => 
        Props.Create<TestActor>(0, false, false);
}
";
            var spec = new VerifyCS.Test
            {

                ReferenceAssemblies = _referenceAssemblies,
                TestState =
                {
                    Sources = { test }
                }
            };

            await spec.RunAsync();

        }

        [TestMethod]
        public async Task TestMethod4()
        {
            var test = @"
using Akka.Actor;

class TestActor : ReceiveActor
{
    private TestActor(int affinity) {}

    public static Props CreateProps() => 
        {|TRD001:Props.Create<TestActor>(0)|};
}
";
            var spec = new VerifyCS.Test
            {

                ReferenceAssemblies = _referenceAssemblies,
                TestState =
                {
                    Sources = { test }
                }
            };

            await spec.RunAsync();
        }

    }
}
