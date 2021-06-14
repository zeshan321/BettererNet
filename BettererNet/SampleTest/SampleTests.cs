using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BettererNet;
using NetArchTest.Rules;
using Xunit;

namespace SampleTest
{
    public class SampleTests
    {
        private readonly Assembly assembly = Assembly.GetAssembly(typeof(SampleProject.Program));

        [Fact]
        public async Task DoNotHaveInterfacePrefix_MatchesFound()
        {
            var result = Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            await new Betterer().AssertAsync(result.ToBetterer());
        }

        [Fact]
        public async Task AsyncMethodsMustEndWithSuffix_MatchesFound()
        {
            var result = new BettererResult();

            foreach (var type in Types.InAssembly(assembly).GetTypes())
            {
                var missingSuffixMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.ReturnType.IsSubclassOf(typeof(Task)) && !m.Name.EndsWith("Async"))
                    .Select(m => $"{type.FullName ?? ""}.{m.Name}");

                result.FailingTypeNames.AddRange(missingSuffixMethods);
            }

            await new Betterer().AssertAsync(result);
        }
    }
}