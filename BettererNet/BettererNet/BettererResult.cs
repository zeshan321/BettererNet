using System;
using System.Collections.Generic;

namespace BettererNet
{
    public class BettererResult
    {
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;
        public bool IsSuccessful { get; set; }
        public IReadOnlyList<string> FailingTypeNames { get; set; } = Array.Empty<string>();
    }
}