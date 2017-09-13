using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp1.Contracts;

namespace ClassLibrary1.Implementation
{
	public class SQLServerReader : ITableReader
	{
		public async Task<IEnumerable<string>> GetTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var connection = new SqlConnection("server=localhost; database=master; integrated security=true"))
			{
				await connection.OpenAsync().ConfigureAwait(false);

				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT name FROM sys.tables WITH (NOLOCK)";

					using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
					{
						var result = new List<string>();

						while (await reader.ReadAsync().ConfigureAwait(false))
						{
							result.Add(reader.GetString(0));
						}

						return result;
					}
				}
			}
		}
	}
}
