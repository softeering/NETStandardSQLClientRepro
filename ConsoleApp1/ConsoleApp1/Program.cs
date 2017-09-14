using System;
using System.IO;
using System.Linq;
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
			var loader = new AssemblyLoader(@"..\output");
			var pluginAssembly = loader.LoadFromAssemblyPath(new FileInfo(@"..\output\ClassLibrary1.Implementation.dll").FullName);
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

	public class AssemblyLoader : AssemblyLoadContext
	{
		private readonly string _folderPath;

		public AssemblyLoader(string folderPath)
		{
			this._folderPath = folderPath;
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			var deps = DependencyContext.Default;
			var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
			if (res.Count > 0)
			{
				return Assembly.Load(new AssemblyName(res.First().Name));
			}
			else
			{
				var apiApplicationFileInfo = new FileInfo(Path.Combine(this._folderPath, $"{assemblyName.Name}.dll"));
				if (File.Exists(apiApplicationFileInfo.FullName))
				{
					var asl = new AssemblyLoader(apiApplicationFileInfo.DirectoryName);
					return asl.LoadFromAssemblyPath(apiApplicationFileInfo.FullName);
				}
			}
			return Assembly.Load(assemblyName);
		}
	}
}
