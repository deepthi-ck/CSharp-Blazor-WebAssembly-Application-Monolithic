using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmMonolith
{
    public sealed class AppManager
    {
        private readonly AppConfiguration _configuration;
        private readonly PartitionRouter _router;
        private readonly ReplicationManager _replication;
        private readonly ExpirationManager _expiration;
        private readonly EvictionManager _eviction;
        private readonly AppStatistics _statistics;

        public AppManager(AppConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _configuration = configuration;
            _statistics = new AppStatistics();
            _replication = new ReplicationManager();
            _expiration = new ExpirationManager();
            _eviction = new EvictionManager();

            List<StoreNode> nodes = new List<StoreNode>();
            for (int i = 1; i <= configuration.NodeCount; i++)
            {
                nodes.Add(new StoreNode("node-" + i));
            }

            _router = new PartitionRouter(nodes);
        }

        public PartitionRouter Router { get { return _router; } }
        public ReplicationManager Replication { get { return _replication; } }
        public AppStatistics Statistics { get { return _statistics; } }
        public AppConfiguration Configuration { get { return _configuration; } }

        public AppResponse Put(string key, string value, TimeSpan? ttl)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset? expiration = _expiration.ComputeExpiration(ttl, _configuration.DefaultTtl, now);
            ResourceEntry entry = new ResourceEntry(key, value, now, expiration, 1);
            StoreNode primary = _router.PrimaryFor(key);
            Sweep(primary.Store, now);
            primary.Store.Put(entry);
            int replicas = _replication.ReplicatePut(entry, _router.ReplicasFor(key));
            int evicted = 0;
            foreach (StoreNode node in _router.AllNodes())
            {
                evicted += _eviction.EvictToCapacity(node.Store, _configuration.Capacity);
            }
            _statistics.RecordPut();
            _statistics.RecordReplication(replicas);
            _statistics.RecordEviction(evicted);
            return AppResponse.Success(key, value, primary.Id);
        }

        public AppResponse Get(string key)
        {
            _statistics.RecordGet();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            StoreNode primary = _router.PrimaryFor(key);
            Sweep(primary.Store, now);

            ResourceEntry entry;
            if (primary.Store.TryGet(key, out entry) && !entry.IsExpired(now))
            {
                _statistics.RecordHit();
                return AppResponse.Success(key, entry.Value, primary.Id);
            }

            foreach (StoreNode replica in _router.ReplicasFor(key))
            {
                Sweep(replica.Store, now);
                if (replica.Store.TryGet(key, out entry) && !entry.IsExpired(now))
                {
                    _statistics.RecordHit();
                    return AppResponse.Success(key, entry.Value, replica.Id);
                }
            }

            _statistics.RecordMiss();
            return AppResponse.NotFound(key);
        }

        public AppResponse Delete(string key)
        {
            StoreNode primary = _router.PrimaryFor(key);
            bool deleted = primary.Store.Delete(key);
            _replication.ReplicateDelete(key, _router.ReplicasFor(key));
            _statistics.RecordDelete();
            if (!deleted)
            {
                return AppResponse.NotFound(key);
            }

            return AppResponse.Success(key, null, primary.Id);
        }

        public int EntryCount()
        {
            return _router.AllNodes().Sum(n => n.Store.Count);
        }

        public int NodeCount()
        {
            return _router.AllNodes().Count;
        }

        private void Sweep(InMemoryStore store, DateTimeOffset now)
        {
            int before = store.Count;
            _expiration.Sweep(store, now);
            int expired = before - store.Count;
            if (expired > 0)
            {
                _statistics.RecordExpiration(expired);
            }
        }
    }
}
