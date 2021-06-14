using BettererNet;
using NetArchTest.Rules;

namespace SampleTest
{
    public static class Converter
    {
        public static BettererResult ToBetterer(this TestResult testResult)
        {
            var result = new BettererResult();
            result.FailingTypeNames.AddRange(testResult.FailingTypeNames);

            return result;
        }
    }
}