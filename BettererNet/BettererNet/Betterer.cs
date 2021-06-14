using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BettererNet
{
    public class Betterer
    {
        private readonly string _fullPath;

        public Betterer([CallerMemberName] string methodName = "")
        {
            var separator = Path.DirectorySeparatorChar;
            var directory =
                $"{Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.FullName.Split($"{separator}bin")[0]}{separator}BettererResults";
            _fullPath = Path.Combine(directory, $"{methodName}.json");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task AssertAsync(BettererResult testResult, bool allowFirstFailure = false)
        {
            var isSuccessful = !testResult.FailingTypeNames.Any();
            if (isSuccessful)
            {
                File.Delete(_fullPath);
                return;
            }

            var result = GetResult();
            if (result == null)
            {
                if (!allowFirstFailure)
                {
                    Assert.Empty(testResult.FailingTypeNames);
                }

                await SaveResultAsync(testResult);
                return;
            }

            var newFails = testResult.FailingTypeNames.Where(n => !result.FailingTypeNames.Contains(n)).ToList();
            Assert.Empty(newFails);

            await SaveResultAsync(testResult);
        }

        private BettererResult? GetResult()
        {
            if (!File.Exists(_fullPath))
            {
                return null;
            }

            var text = File.ReadAllText(_fullPath);
            return JsonSerializer.Deserialize<BettererResult>(text);
        }

        private async Task SaveResultAsync(BettererResult result)
        {
            await using FileStream createStream = File.Create(_fullPath);
            await JsonSerializer.SerializeAsync(createStream, result,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            await createStream.DisposeAsync();
        }
    }
}