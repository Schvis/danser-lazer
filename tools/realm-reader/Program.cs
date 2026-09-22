using System;
using System.IO;
using System.Linq;
using Realms;
using Realms.Dynamic;

class Program
{
    static void Main()
    {
        string realmPath = @"C:\Users\Schvis\AppData\Roaming\osu\client.realm";
        var config = new RealmConfiguration(realmPath)
        {
            IsReadOnly = true,
            IsDynamic = true
        };

        using var realm = Realm.GetInstance(config);
        var beatmaps = realm.DynamicApi.All("Beatmap");
        Console.WriteLine($"Total Beatmaps in client.realm: {beatmaps.Count()}");
        foreach (var b in beatmaps.Take(5))
        {
            var md5 = b.DynamicApi.Get<string>("MD5Hash");
            var diff = b.DynamicApi.Get<string>("DifficultyName");
            var hash = b.DynamicApi.Get<string>("Hash");
            Console.WriteLine($"MD5: {md5}, Diff: {diff}, Hash: {hash}");
        }
    }
}
