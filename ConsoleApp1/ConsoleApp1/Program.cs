using System;
using System.Collections.Generic;
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
		private readonly IList<DependencyContext> _contexts;

		public AssemblyLoader(string folderPath)
		{
			this._contexts = new List<DependencyContext>()
			{
				DependencyContext.Default
			};
			this._folderPath = folderPath;
			this.LoadContexts();
		}

		private void LoadContexts()
		{
			if (!Directory.Exists(this._folderPath))
				return;

			foreach (var depsFile in Directory.GetFiles(this._folderPath, "*.deps.json"))
			{
				this._contexts.Add(this.GetDependencyContextFromDepsFile(depsFile));
			}

			var libs = (from deps in this._contexts
						from compiled in deps.RuntimeLibraries
							// from assembly in compiled.Assemblies
						select compiled.Name).ToArray();
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			var res = (from deps in this._contexts
					   from lib in deps.CompileLibraries
					   where lib.Name.Contains(assemblyName.Name)
					   select lib).ToArray();

			var libs = (from deps in this._contexts
						from compiled in deps.RuntimeLibraries
						where compiled.Name.StartsWith(assemblyName.Name)
						// from assembly in compiled.Assemblies
						select compiled).ToArray();

			if (res.Length > 0)
			{
				return Assembly.Load(new AssemblyName(res.First().Name));
			}
			else if (libs.Length > 0)
			{
				var foundDlls = Directory.GetFileSystemEntries(this._folderPath, $"{assemblyName.Name}.dll", SearchOption.AllDirectories);
				if (foundDlls.Any())
				{
					return Assembly.Load(new AssemblyName(foundDlls[0]));
				}

				return Assembly.Load(new AssemblyName(libs.First().Name));
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

		private DependencyContext GetDependencyContextFromDepsFile(string depsFile)
		{
			using (var reader = new DependencyContextJsonReader())
			using (var stream = File.OpenRead(depsFile))
			{
				return reader.Read(stream);
			}
		}
	}
}
