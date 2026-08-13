using System.Threading;

namespace BlazorWasmMonolith
{
    public sealed class AppStatistics
    {
        private int _hits;
        private int _misses;
        private int _puts;
        private int _gets;
        private int _deletes;
        private int _replications;
        private int _evictions;
        private int _expirations;

        public int Hits { get { return _hits; } }
        public int Misses { get { return _misses; } }
        public int Puts { get { return _puts; } }
        public int Gets { get { return _gets; } }
        public int Deletes { get { return _deletes; } }
        public int Replications { get { return _replications; } }
        public int Evictions { get { return _evictions; } }
        public int Expirations { get { return _expirations; } }

        public void RecordHit() { Interlocked.Increment(ref _hits); }
        public void RecordMiss() { Interlocked.Increment(ref _misses); }
        public void RecordPut() { Interlocked.Increment(ref _puts); }
        public void RecordGet() { Interlocked.Increment(ref _gets); }
        public void RecordDelete() { Interlocked.Increment(ref _deletes); }
        public void RecordReplication(int count) { Interlocked.Add(ref _replications, count); }
        public void RecordEviction(int count) { Interlocked.Add(ref _evictions, count); }
        public void RecordExpiration(int count) { Interlocked.Add(ref _expirations, count); }
    }
}
