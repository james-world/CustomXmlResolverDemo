using System.Net;
using System.Xml;

public class CachingXmlResolver : XmlResolver
{
    private readonly XmlResolver _innerResolver;
    private readonly int _maxCacheSize;
    private readonly TimeSpan _cacheDuration;
    private readonly Dictionary<Uri, CacheItem> _cacheMap = new Dictionary<Uri, CacheItem>();
    private readonly LinkedList<Uri> _lruList = new LinkedList<Uri>();
    private readonly object _lock = new object();

    public CachingXmlResolver(XmlResolver innerResolver, TimeSpan cacheDuration, int maxCacheSize)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));
        _cacheDuration = cacheDuration;
        _maxCacheSize = maxCacheSize;
    }

    public override ICredentials Credentials
    {
        set { /* Not used in this resolver */ }
    }

    public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
    {
        // Ensure the expected return type is Stream
        if (ofObjectToReturn != null && ofObjectToReturn != typeof(Stream))
        {
            throw new ArgumentException("Expected return type is Stream.", nameof(ofObjectToReturn));
        }

        CacheItem? cacheItem;
        lock (_lock)
        {
            // Check if the item is in the cache
            if (_cacheMap.TryGetValue(absoluteUri, out cacheItem))
            {
                // Check if the cache item has expired
                if (DateTime.UtcNow - cacheItem.Timestamp < _cacheDuration)
                {
                    // Move the accessed item to the front of the LRU list
                    _lruList.Remove(cacheItem.Node);
                    _lruList.AddFirst(cacheItem.Node);

                    Console.WriteLine($"Cache hit: {absoluteUri}");

                    // Return a new MemoryStream over the cached data
                    return new MemoryStream(cacheItem.Data, writable: false);
                }
                else
                {
                    // Remove expired item
                    _lruList.Remove(cacheItem.Node);
                    _cacheMap.Remove(absoluteUri);
                    // No need to dispose byte array
                }
            }
            
            Console.WriteLine($"Cache miss: {absoluteUri}");
        }

        // Get the entity from the inner resolver
        var entity = _innerResolver.GetEntity(absoluteUri, role, ofObjectToReturn)
            ?? throw new XmlException($"Resource '{absoluteUri}' not found.");

        byte[] data;
        if (entity is Stream stream)
        {
            // Read the stream into a byte array
            using (stream)
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                data = ms.ToArray();
            }

            // Create a new cache item
            var node = new LinkedListNode<Uri>(absoluteUri);
            cacheItem = new CacheItem
            {
                Data = data,
                Timestamp = DateTime.UtcNow,
                Node = node
            };

            lock (_lock)
            {
                // Add the new item to the cache
                _cacheMap[absoluteUri] = cacheItem;
                _lruList.AddFirst(node);

                // Enforce maximum cache size
                if (_cacheMap.Count > _maxCacheSize)
                {
                    RemoveLeastRecentlyUsedItem();
                }
            }

            // Return a new MemoryStream over the data
            return new MemoryStream(data, writable: false);
        }
        else
        {
            // Non-stream entities are not cached
            return entity;
        }
    }

    private void RemoveLeastRecentlyUsedItem()
    {
        // Remove the last item from the LRU list
        var lruUri = _lruList.Last!.Value;
        _lruList.RemoveLast();

        if (_cacheMap.TryGetValue(lruUri, out var cacheItem))
        {
            _cacheMap.Remove(lruUri);
            // No need to dispose byte array
        }
    }

    private class CacheItem
    {
        public required byte[] Data { get; set; }
        public DateTime Timestamp { get; set; }
        public required LinkedListNode<Uri> Node { get; set; }
    }
}
