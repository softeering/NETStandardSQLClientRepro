using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;

namespace ConsoleApp1
{
	public static class PlatformHelper
	{
		public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		
		private static DependencyContext[] _dependencyContexts;
		public static DependencyContext[] LoadDependencyContexts(string rootFolder = null)
		{
			if (_dependencyContexts == null)
			{
				if (rootFolder == null)
					rootFolder = Directory.GetCurrentDirectory();

				var depFiles = Directory.GetFiles(rootFolder, "*.deps.json");
				_dependencyContexts = new DependencyContext[depFiles.Length];

				for (int i = 0; i < depFiles.Length; i++)
				{
					_dependencyContexts[i] = GetDependencyContextFromDepsFile(depFiles[i]);
				}
			}

			return _dependencyContexts;
		}

		private static DependencyContext GetDependencyContextFromDepsFile(string depsFile)
		{
			using (var reader = new DependencyContextJsonReader())
			using (var stream = File.OpenRead(depsFile))
			{
				return reader.Read(stream);
			}
		}
	}

	public class RuntimeAssemblyLoader : AssemblyLoadContext
	{
		private readonly string _folderPath;
		private readonly List<DependencyContext> _contexts;

		public RuntimeAssemblyLoader(string folderPath)
		{
			if (string.IsNullOrWhiteSpace(folderPath))
				throw new ArgumentNullException(nameof(folderPath));

			if (!Directory.Exists(folderPath))
				throw new ArgumentException($"{folderPath} folder does not exist");

			this._folderPath = folderPath;
			this._contexts = new List<DependencyContext>() { DependencyContext.Default };
			this.LoadContexts();
		}

		private void LoadContexts()
		{
			this._contexts.AddRange(PlatformHelper.LoadDependencyContexts(this._folderPath));

			this._compiledLibraries = (from deps in this._contexts
									   from lib in deps.CompileLibraries
									   select lib).ToArray();

			this._runtimeLibraries = (from deps in this._contexts
									  from lib in deps.RuntimeLibraries
									  select lib).ToArray();
		}

		private CompilationLibrary[] _compiledLibraries;
		private Library[] _runtimeLibraries;

		protected override Assembly Load(AssemblyName assemblyName)
		{
			var compiled = (from lib in this._compiledLibraries
							where lib.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)
							select lib).ToArray();

			if (compiled.Length > 0)
				return Assembly.Load(new AssemblyName(compiled.First().Name));

			var runtime = (from lib in this._runtimeLibraries
						   where lib.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)
						   select lib).ToArray();

			if (runtime.Length > 0)
			{
				var foundDlls = Directory.GetFileSystemEntries(this._folderPath, $"{assemblyName.Name}.dll", SearchOption.AllDirectories);
				var count = foundDlls.Count();
				if (count == 0)
					return Assembly.Load(new AssemblyName(runtime.First().Name));
				else if (count == 1)
					return base.LoadFromAssemblyPath(new FileInfo(foundDlls[0]).FullName);

				string platformSpecificPath = this.GetPathForCurrentPlatform(foundDlls);
				return base.LoadFromAssemblyPath(new FileInfo(platformSpecificPath).FullName);
			}

			var apiApplicationFileInfo = new FileInfo(Path.Combine(this._folderPath, $"{assemblyName.Name}.dll"));
			if (File.Exists(apiApplicationFileInfo.FullName))
			{
				var asl = new RuntimeAssemblyLoader(apiApplicationFileInfo.DirectoryName);
				return asl.LoadFromAssemblyPath(apiApplicationFileInfo.FullName);
			}

			return Assembly.Load(assemblyName);
		}

		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			var foundDlls = Directory.GetFileSystemEntries(this._folderPath, unmanagedDllName, SearchOption.AllDirectories);

			if (foundDlls.Length > 0)
			{
				string path = this.GetPathForCurrentPlatform(foundDlls);
				return base.LoadUnmanagedDllFromPath(new FileInfo(path).FullName);
			}
			else
				return base.LoadUnmanagedDll(unmanagedDllName);
		}

		private string GetPathForCurrentPlatform(string[] dllPaths)
		{
			if (dllPaths == null || dllPaths.Length < 1)
				return null;

			if (dllPaths.Length == 1)
				return dllPaths[0];

			string result = null;
			var query = dllPaths.AsQueryable();

			if (PlatformHelper.IsMacOS())
				query = query.Where(i => i.Contains("macos"));
			else if (PlatformHelper.IsLinux())
				query = query.Where(i => i.Contains("linux"));			
			else
				query = query.Where(i => i.Contains("win"));

			if (query.Count() > 1)
			{
				if (Environment.Is64BitProcess)
					query = query.Where(i => i.Contains("x64"));
				else
					query = query.Where(i => i.Contains("x86"));
			}

			result = query.FirstOrDefault();

			if (result == null)
				result = dllPaths.First();

			return result;
		}
	}
}
