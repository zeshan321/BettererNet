using System;
using System.Collections.Generic;

namespace BettererNet
{
    public class BettererResult
    {
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;
        public List<string> FailingTypeNames { get; set; } = new();
    }
}