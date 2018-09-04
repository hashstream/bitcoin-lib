using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_node_lib;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node
{
    public class HashStore<V> : IDictionary<Hash, V>, IDisposable where V : IHash, IStreamable, new()
    {
        private string Database { get; set; }
        private UInt32 Changes { get; set; }
        private Task FlushTask { get; set; }
        private SemaphoreSlim FileLock { get; set; } = new SemaphoreSlim(1);
        private bool Exiting { get; set; }

        private ConcurrentDictionary<Hash, V> Store { get; set; } = new ConcurrentDictionary<Hash, V>();

        public ICollection<Hash> Keys => ((IDictionary<Hash, V>)Store).Keys;

        public ICollection<V> Values => ((IDictionary<Hash, V>)Store).Values;

        public int Count => ((IDictionary<Hash, V>)Store).Count;

        public bool IsReadOnly => ((IDictionary<Hash, V>)Store).IsReadOnly;

        public V this[Hash key] { get => ((IDictionary<Hash, V>)Store)[key]; set => ((IDictionary<Hash, V>)Store)[key] = value; }

        public HashStore(string database)
        {
            Database = database;
            Load().Wait();

            FlushTask = FlushJob();
        }

        private async Task FlushJob()
        {
            while (!Exiting)
            {
                await Task.Delay(10000);
                await Flush();
            }
        }

        public async Task Load()
        {
            Store.Clear();

            var fn = $"{Database}.dat";
            if (File.Exists(fn))
            {
                using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
#if NETCOREAPP2_1
                    using (var ms = new MessageStream(fs))
                    {
                        while (true)
                        {
                            var header = await ms.ReadMessage<HashStoreHeader>(HashStoreHeader.StaticSize);
                            if(header == null)
                            {
                                break;
                            }

                            var msg = await ms.ReadMessage<V>((int)header.MsgLen);
                            if (msg != null)
                            {
                                if (msg.Hash == header.Hash)
                                {
                                    Store.TryAdd(header.Hash, msg);
                                }
                            }
                            else
                            {
                                var m2 = await ms.ReadMessage<VerAck>((int)header.MsgLen);
                                if(m2 == null)
                                {
                                    break;
                                }
                            }
                        }
                    }
#else

#endif
                }
            }
        }

        public async Task Flush()
        {
            await FileLock.WaitAsync();
            var fn = $"{Database}.dat";
            using (var fs = new FileStream(fn, FileMode.Create, FileAccess.ReadWrite))
            {
                foreach (var x in Store)
                {
                    var mh = new HashStoreHeader();
                    mh.Hash = x.Key;
                    mh.MsgLen = (uint)x.Value.Size;

#if NETCOREAPP2_1
                    await fs.WriteAsync(mh.ToArray());
                    await fs.WriteAsync(x.Value.ToArray());
#else
                    await fs.WriteAsync(mh.ToArray(), 0, mh.Size);
                    await fs.WriteAsync(x.Value.ToArray(), 0, x.Value.Size);
#endif
                }
            }

            FileLock.Release();
        }

        public void Add(Hash key, V value)
        {
            ((IDictionary<Hash, V>)Store).Add(key, value);
        }

        public bool ContainsKey(Hash key)
        {
            return ((IDictionary<Hash, V>)Store).ContainsKey(key);
        }

        public bool Remove(Hash key)
        {
            return ((IDictionary<Hash, V>)Store).Remove(key);
        }

        public bool TryGetValue(Hash key, out V value)
        {
            return ((IDictionary<Hash, V>)Store).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<Hash, V> item)
        {
            ((IDictionary<Hash, V>)Store).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<Hash, V>)Store).Clear();
        }

        public bool Contains(KeyValuePair<Hash, V> item)
        {
            return ((IDictionary<Hash, V>)Store).Contains(item);
        }

        public void CopyTo(KeyValuePair<Hash, V>[] array, int arrayIndex)
        {
            ((IDictionary<Hash, V>)Store).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<Hash, V> item)
        {
            return ((IDictionary<Hash, V>)Store).Remove(item);
        }

        public IEnumerator<KeyValuePair<Hash, V>> GetEnumerator()
        {
            return ((IDictionary<Hash, V>)Store).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<Hash, V>)Store).GetEnumerator();
        }

        public void Dispose()
        {
            Exiting = true;
            Flush().Wait();
        }
    }

    internal class HashStoreHeader : IStreamable
    {
        public Hash Hash { get; set; }

        public UInt32 MsgLen { get; set; }

        public int Size => StaticSize;

        public static int StaticSize => 40;


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out Hash tKey)
                 .ReadAndSlice(out UInt32 tLen);

            Hash = tKey;
            MsgLen = tLen;

            return next;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Hash)
                .WriteAndSlice(MsgLen);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            Hash = data.ReadFromBuffer<Hash>(ref roffset);
            MsgLen = data.ReadUInt32FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Hash, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(MsgLen), ref woffset);

            return ret;
        }
#endif
    }
}
