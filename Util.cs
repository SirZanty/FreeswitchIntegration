using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace FreeswitchIntegration;

class Util
{
    public static Task WaitForEnterKeyPress()
    {
        return
            Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Where(_ => Console.KeyAvailable)
                .Select(_ => Console.ReadKey(false).Key)
                .FirstAsync(x => x == ConsoleKey.Enter)
                .ToTask();
    }
}