using System.Collections.Concurrent;
using System.Threading;

namespace im.sdk
{
    public sealed class SeqGenerator
    {
        private readonly ConcurrentQueue<int> _seqPool;
        private int _seed;
        public static readonly SeqGenerator Instance = new SeqGenerator();

        private SeqGenerator()
        {
            _seqPool = new ConcurrentQueue<int>();
        }

        public int GetSeq()
        {
            if (_seqPool.TryDequeue(out int seq))
            {
                return seq;
            }
            return Interlocked.Increment(ref _seed);
        }

        public void FreeSeq(int seq)
        {
            _seqPool.Enqueue(seq);
        }
    }
}
