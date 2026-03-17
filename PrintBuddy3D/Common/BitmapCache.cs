using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PrintBuddy3D.Common;

public static class BitmapCache
{
    private const int Capacity = 30;

    private static readonly LinkedList<string> LruOrder = new();
    private static readonly Dictionary<string, (Bitmap Bitmap, LinkedListNode<string> Node)> Cache = new();

    public static Bitmap Get(string path)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(path, out var entry))
            {
                LruOrder.Remove(entry.Node);
                var node = LruOrder.AddFirst(path);
                Cache[path] = (entry.Bitmap, node);
                return entry.Bitmap;
            }

            if (Cache.Count >= Capacity)
                Evict();

            var bitmap = Load(path);
            var newNode = LruOrder.AddFirst(path);
            Cache[path] = (bitmap, newNode);
            return bitmap;
        }
    }

    private static void Evict()
    {
        var oldest = LruOrder.Last;
        if (oldest == null) return;
        LruOrder.RemoveLast();
        if (Cache.TryGetValue(oldest.Value, out var entry))
        {
            entry.Bitmap.Dispose();
            Cache.Remove(oldest.Value);
        }
    }

    private static Bitmap Load(string path)
    {
        if (path.StartsWith("avares://", StringComparison.Ordinal))
        {
            using var stream = AssetLoader.Open(new Uri(path));
            return new Bitmap(stream);
        }
        return new Bitmap(path);
    }
}
