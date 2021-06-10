using System;
using System.Reflection;
using System.Threading.Tasks;
using BettererNet;
using NetArchTest.Rules;
using Xunit;

namespace SampleTest
{
    public class SampleTests
    {
        [Fact]
        public async Task DoNotHaveInterfacePrefix_MatchesFound()
        {
            var result = Types.InAssembly(Assembly.GetAssembly(typeof(SampleProject.Program)))
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();
            
            await new Betterer().AssertAsync(result.ToBetterer());
        }
    }
}