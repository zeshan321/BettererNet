using BettererNet;
using NetArchTest.Rules;

namespace SampleTest
{
    public static class Converter
    {
        public static BettererResult ToBetterer(this TestResult testResult)
        {
            return new BettererResult
            {
                IsSuccessful = testResult.IsSuccessful,
                FailingTypeNames = testResult.FailingTypeNames
            };
        }
    }
}