using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Realms;
using Realms.Dynamic;

namespace LazerBridge;

public class LazerBeatmapEntry
{
    public string MD5 { get; set; } = "";
    public string Hash { get; set; } = "";
    public int OnlineID { get; set; }
    public string Difficulty { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleUnicode { get; set; } = "";
    public string Artist { get; set; } = "";
    public string ArtistUnicode { get; set; } = "";
    public string Creator { get; set; } = "";
    public string AudioFile { get; set; } = "";
    public string BackgroundFile { get; set; } = "";
    public int SetOnlineID { get; set; }
    public string SetHash { get; set; } = "";
    public string OsuFile { get; set; } = "";
    public string OsuFileStoragePath { get; set; } = "";
    public Dictionary<string, string> StorageFiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class LazerSkinEntry
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Creator { get; set; } = "";
    public string Hash { get; set; } = "";
    public bool Protected { get; set; }
    public Dictionary<string, string> StorageFiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

class Program
{
    static string GetLazerStoragePath(string filesDir, string hash)
    {
        if (string.IsNullOrEmpty(hash) || hash.Length < 2) return "";
        return Path.Combine(filesDir, hash.Substring(0, 1), hash.Substring(0, 2), hash);
    }

    static void Main(string[] args)
    {
        string lazerDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
        string filterMd5 = "";
        int filterId = -1;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--lazer-dir" && i + 1 < args.Length) lazerDir = args[++i];
            else if (args[i] == "--md5" && i + 1 < args.Length) filterMd5 = args[++i].Trim().ToLowerInvariant();
            else if (args[i] == "--id" && i + 1 < args.Length && int.TryParse(args[++i], out var idVal)) filterId = idVal;
        }

        string realmPath = Path.Combine(lazerDir, "client.realm");
        string filesDir = Path.Combine(lazerDir, "files");

        if (!File.Exists(realmPath))
        {
            Console.Error.WriteLine($"Error: Realm database not found at {realmPath}");
            Environment.Exit(1);
        }

        var config = new RealmConfiguration(realmPath)
        {
            IsReadOnly = true,
            IsDynamic = true
        };

        using var realm = Realm.GetInstance(config);

        bool dumpSchema = args.Contains("--dump-schema");
        if (dumpSchema)
        {
            foreach (var schema in realm.Schema)
            {
                Console.WriteLine($"TABLE: {schema.Name}");
                foreach (var prop in schema)
                {
                    Console.WriteLine($"  {prop.Name} ({prop.Type})");
                }
            }
            return;
        }

        string skinFilter = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--skin" && i + 1 < args.Length) skinFilter = args[++i].Trim();
        }

        bool listSkins = args.Contains("--list-skins") || !string.IsNullOrEmpty(skinFilter);
        if (listSkins)
        {
            var skins = realm.DynamicApi.All("Skin");
            var skinEntries = new List<LazerSkinEntry>();

            foreach (var s in skins)
            {
                try
                {
                    bool deletePending = false;
                    try { deletePending = s.DynamicApi.Get<bool>("DeletePending"); } catch {}
                    if (deletePending) continue;

                    string sName = s.DynamicApi.Get<string>("Name") ?? "";
                    string sCreator = s.DynamicApi.Get<string>("Creator") ?? "";
                    string sHash = s.DynamicApi.Get<string>("Hash") ?? "";
                    bool isProtected = false;
                    try { isProtected = s.DynamicApi.Get<bool>("Protected"); } catch {}

                    if (!string.IsNullOrEmpty(skinFilter))
                    {
                        if (!sName.Equals(skinFilter, StringComparison.OrdinalIgnoreCase) &&
                            !sName.Contains(skinFilter, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    var fileDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    var fileList = s.DynamicApi.GetList<DynamicEmbeddedObject>("Files");
                    foreach (var f in fileList)
                    {
                        var fn = f.DynamicApi.Get<string>("Filename");
                        var fo = f.DynamicApi.Get<DynamicRealmObject>("File");
                        var fh = fo?.DynamicApi.Get<string>("Hash");
                        if (!string.IsNullOrEmpty(fn) && !string.IsNullOrEmpty(fh))
                        {
                            string storagePath = GetLazerStoragePath(filesDir, fh);
                            fileDict[fn] = storagePath;
                        }
                    }

                    skinEntries.Add(new LazerSkinEntry
                    {
                        ID = s.DynamicApi.Get<Guid>("ID").ToString(),
                        Name = sName,
                        Creator = sCreator,
                        Hash = sHash,
                        Protected = isProtected,
                        StorageFiles = fileDict
                    });
                }
                catch {}
            }

            var skinJson = JsonSerializer.Serialize(skinEntries, new JsonSerializerOptions { WriteIndented = false });
            Console.WriteLine(skinJson);
            return;
        }
        var beatmapSets = realm.DynamicApi.All("BeatmapSet");

        var entries = new List<LazerBeatmapEntry>();

        foreach (var s in beatmapSets)
        {
            int setOnlineId = 0;
            try { setOnlineId = s.DynamicApi.Get<int>("OnlineID"); } catch {}
            string setHash = "";
            try { setHash = s.DynamicApi.Get<string>("Hash") ?? ""; } catch {}

            var fileDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var fileList = s.DynamicApi.GetList<DynamicEmbeddedObject>("Files");
            foreach (var f in fileList)
            {
                var fn = f.DynamicApi.Get<string>("Filename");
                var fo = f.DynamicApi.Get<DynamicRealmObject>("File");
                var fh = fo?.DynamicApi.Get<string>("Hash");
                if (!string.IsNullOrEmpty(fn) && !string.IsNullOrEmpty(fh))
                {
                    string storagePath = GetLazerStoragePath(filesDir, fh);
                    fileDict[fn] = storagePath;
                }
            }

            var bmaps = s.DynamicApi.GetList<DynamicRealmObject>("Beatmaps");
            foreach (var b in bmaps)
            {
                string md5 = b.DynamicApi.Get<string>("MD5Hash")?.ToLowerInvariant() ?? "";
                if (!string.IsNullOrEmpty(filterMd5) && md5 != filterMd5)
                {
                    continue;
                }

                int bmapOnlineId = 0;
                try { bmapOnlineId = b.DynamicApi.Get<int>("OnlineID"); } catch {}

                if (filterId > -1 && bmapOnlineId != filterId)
                {
                    continue;
                }

                var entry = new LazerBeatmapEntry
                {
                    MD5 = md5,
                    Hash = b.DynamicApi.Get<string>("Hash") ?? "",
                    OnlineID = bmapOnlineId,
                    Difficulty = b.DynamicApi.Get<string>("DifficultyName") ?? "",
                    SetOnlineID = setOnlineId,
                    SetHash = setHash,
                    StorageFiles = fileDict
                };

                var meta = b.DynamicApi.Get<DynamicRealmObject>("Metadata");
                if (meta != null)
                {
                    entry.Title = meta.DynamicApi.Get<string>("Title") ?? "";
                    entry.TitleUnicode = meta.DynamicApi.Get<string>("TitleUnicode") ?? entry.Title;
                    entry.Artist = meta.DynamicApi.Get<string>("Artist") ?? "";
                    entry.ArtistUnicode = meta.DynamicApi.Get<string>("ArtistUnicode") ?? entry.Artist;
                    entry.AudioFile = meta.DynamicApi.Get<string>("AudioFile") ?? "";
                    entry.BackgroundFile = meta.DynamicApi.Get<string>("BackgroundFile") ?? "";
                    try { entry.Creator = meta.DynamicApi.Get<DynamicRealmObject>("Author")?.DynamicApi.Get<string>("Username") ?? ""; } catch {}
                }

                // Match .osu file in storage files
                string expectedHash = entry.Hash;
                foreach (var kvp in fileDict)
                {
                    if (kvp.Key.EndsWith(".osu", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Path.GetFileName(kvp.Value).Equals(expectedHash, StringComparison.OrdinalIgnoreCase) ||
                            kvp.Key.Contains(entry.Difficulty, StringComparison.OrdinalIgnoreCase))
                        {
                            entry.OsuFile = kvp.Key;
                            entry.OsuFileStoragePath = kvp.Value;
                            break;
                        }
                    }
                }

                entries.Add(entry);
            }
        }

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = false });
        Console.WriteLine(json);
    }
}
