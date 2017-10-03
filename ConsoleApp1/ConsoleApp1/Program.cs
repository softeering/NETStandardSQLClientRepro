using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using ConsoleApp1.Contracts;
using Microsoft.Extensions.DependencyModel;

namespace ConsoleApp1
{
	class Program
	{
		static async Task Main(string[] args)
		{
			bool useDefault = false;
			Assembly pluginAssembly = null;

			if (useDefault)
			{
				pluginAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(new FileInfo(@"..\output\ClassLibrary1.Implementation.dll").FullName);
			}
			else
			{
				var loader = new RuntimeAssemblyLoader(@"..\output");
				pluginAssembly = loader.LoadFromAssemblyPath(new FileInfo(@"..\output\ClassLibrary1.Implementation.dll").FullName);
			}
			
			ITableReader implementation = Activator.CreateInstance(pluginAssembly.GetType("ClassLibrary1.Implementation.SQLServerReader", true)) as ITableReader;
			var tableList = await implementation.GetTablesAsync();

			foreach (var item in tableList)
			{
				Console.WriteLine(item);
			}

			Console.WriteLine();
			Console.WriteLine("Completed... [Press enter to terminate]");
			Console.ReadLine();
		}
	}
}
