using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Contracts
{
    public interface ITableReader
    {
		Task<IEnumerable<string>> GetTablesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
