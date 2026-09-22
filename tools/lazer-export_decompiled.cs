using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Realms;
using Realms.Dynamic;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
[assembly: AssemblyCompany("lazer-export")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+8331b0ffb841cc9e0f5e6b756bcf2bba2a9465c0")]
[assembly: AssemblyProduct("lazer-export")]
[assembly: AssemblyTitle("lazer-export")]
[assembly: WovenAssembly]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
namespace LazerBridge;

internal class Program
{
	private const int SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 2;

	[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, nint lpSecurityAttributes);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

	private static string Sanitize(string name)
	{
		char[] invalidChars = Path.GetInvalidFileNameChars();
		string text = new string(name.Select((char c) => (!invalidChars.Contains(c)) ? c : '_').ToArray()).Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			return "unknown";
		}
		if (text.Length > 120)
		{
			text = text.Substring(0, 120);
		}
		return text;
	}

	private static void Main(string[] args)
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
		string text2 = Path.Combine(text, "Songs");
		string path = text;
		string text3 = text2;
		bool flag = false;
		string text4 = "";
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "--lazer-dir" && i + 1 < args.Length)
			{
				path = args[++i];
			}
			else if (args[i] == "--target-dir" && i + 1 < args.Length)
			{
				text3 = args[++i];
			}
			else if (args[i] == "--hash" && i + 1 < args.Length)
			{
				flag = true;
				text4 = args[++i].Trim().ToLowerInvariant();
			}
		}
		string text5 = Path.Combine(path, "client.realm");
		string text6 = Path.Combine(path, "files");
		if (!File.Exists(text5))
		{
			Console.Error.WriteLine("Error: Realm database not found at " + text5);
			Environment.Exit(1);
		}
		Console.WriteLine("Reading Lazer library: " + text5);
		Console.WriteLine("Target Songs folder:   " + text3);
		Directory.CreateDirectory(text3);
		using Realm realm = Realm.GetInstance(new RealmConfiguration(text5)
		{
			IsReadOnly = true,
			IsDynamic = true
		});
		IQueryable<IRealmObject> queryable = realm.DynamicApi.All("BeatmapSet");
		bool flag2 = string.Equals(Path.GetPathRoot(Path.GetFullPath(text6)), Path.GetPathRoot(Path.GetFullPath(text3)), StringComparison.OrdinalIgnoreCase);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (DynamicRealmObject item in queryable)
		{
			IList<DynamicRealmObject> list = item.DynamicApi.GetList<DynamicRealmObject>("Beatmaps");
			if (flag)
			{
				bool flag3 = false;
				foreach (DynamicRealmObject item2 in list)
				{
					string text7 = item2.DynamicApi.Get<string>("MD5Hash")?.ToLowerInvariant() ?? "";
					string text8 = item2.DynamicApi.Get<string>("Hash")?.ToLowerInvariant() ?? "";
					if (text7 == text4 || text8 == text4)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					continue;
				}
			}
			int num4 = 0;
			try
			{
				num4 = item.DynamicApi.Get<int>("OnlineID");
			}
			catch
			{
			}
			string text9 = "Unknown Artist";
			string text10 = "Unknown Title";
			if (list.Count > 0)
			{
				try
				{
					DynamicRealmObject dynamicRealmObject2 = list[0].DynamicApi.Get<DynamicRealmObject>("Metadata");
					if (dynamicRealmObject2 != null)
					{
						text9 = dynamicRealmObject2.DynamicApi.Get<string>("Artist") ?? text9;
						text10 = dynamicRealmObject2.DynamicApi.Get<string>("Title") ?? text10;
					}
				}
				catch
				{
				}
			}
			string path2 = ((num4 > 0) ? $"{num4} {Sanitize(text9)} - {Sanitize(text10)}" : (Sanitize(text9) + " - " + Sanitize(text10)));
			string text11 = Path.Combine(text3, path2);
			Directory.CreateDirectory(text11);
			foreach (DynamicEmbeddedObject item3 in item.DynamicApi.GetList<DynamicEmbeddedObject>("Files"))
			{
				string text12 = item3.DynamicApi.Get<string>("Filename");
				DynamicRealmObject dynamicRealmObject3 = item3.DynamicApi.Get<DynamicRealmObject>("File");
				if (dynamicRealmObject3 == null)
				{
					continue;
				}
				string text13 = dynamicRealmObject3.DynamicApi.Get<string>("Hash");
				if (string.IsNullOrEmpty(text12) || string.IsNullOrEmpty(text13))
				{
					continue;
				}
				string text14 = Path.Combine(text6, text13.Substring(0, 1), text13.Substring(0, 2), text13);
				if (!File.Exists(text14))
				{
					num3++;
					continue;
				}
				string text15 = Path.Combine(text11, text12);
				string directoryName = Path.GetDirectoryName(text15);
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				if (File.Exists(text15))
				{
					num2++;
					continue;
				}
				bool flag4 = false;
				if (flag2)
				{
					flag4 = CreateHardLink(text15, text14, IntPtr.Zero);
				}
				if (!flag4)
				{
					flag4 = CreateSymbolicLink(text15, text14, 2);
				}
				if (!flag4)
				{
					try
					{
						File.Copy(text14, text15, overwrite: true);
						flag4 = true;
					}
					catch
					{
					}
				}
				if (flag4)
				{
					num2++;
				}
				else
				{
					num3++;
				}
			}
			num++;
			if (num % 500 == 0)
			{
				Console.WriteLine($"Progress: {num} beatmap sets mapped ({num2} files linked)...");
			}
		}
		Console.WriteLine($"Done! {num} beatmap sets mapped. {num2} files linked, {num3} failed.");
	}
}
internal class lazer-export_ProcessedByFody
{
	internal const string FodyVersion = "6.9.1.0";

	internal const string Realm = "20.1.0.0";
}
