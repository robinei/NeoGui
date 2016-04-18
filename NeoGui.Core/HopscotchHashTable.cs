using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NeoGui.Core
{
    public class HopscotchHashTable<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        private const int HopRange = 32;
        private const int AddRange = 64;

        private const uint OccupiedBit = 1u << 31;
        private const uint HashMask = ~OccupiedBit;

        private struct Bucket
        {
            public uint HopMask;
            public uint HashField;
            public TKey Key;
            public TValue Value;

            public uint Hash => HashField & HashMask;
            public bool IsOccupied => (HashField & OccupiedBit) != 0;
            public void ClearOccupied() { HashField &= ~OccupiedBit; }
        }

        private Bucket[] buckets = new Bucket[193];
        private int tableSizeIndex = 1;
        private int count;

        private void Rehash()
        {
            var oldBuckets = buckets;

            tableSizeIndex += 2;
            if (tableSizeIndex >= HopscotchUtils.TableSizes.Length) {
                throw new Exception("hash table too big");
            }
            var size = HopscotchUtils.TableSizes[tableSizeIndex];
            buckets = new Bucket[size];
            count = 0;

            for (var oldBucket = 0; oldBucket < oldBuckets.Length; ++oldBucket) {
                var old = oldBuckets[oldBucket];
                if (old.IsOccupied) {
                    var hash = old.Hash;
                    var newBucket = AllocBucket(old.Key, hash, MapHashToBucket(hash));
                    buckets[newBucket].Value = old.Value;
                }
            }
        }

        private int AllocBucket(TKey key, uint hash, int startBucket)
        {
            var freeBucket = startBucket;
            var freeDistance = 0;
            while (freeDistance < AddRange) {
                if (!buckets[freeBucket].IsOccupied) {
                    break;
                }
                ++freeDistance;
                freeBucket = startBucket + freeDistance;
                if (freeBucket >= Capacity) {
                    freeBucket -= Capacity;
                }
            }
            if (freeDistance < AddRange) {
                do {
                    if (freeDistance < HopRange) {
                        buckets[startBucket].HopMask |= (1u << freeDistance);
                        buckets[freeBucket].HashField = hash | OccupiedBit;
                        buckets[freeBucket].Key = key;
                        ++count;
                        return freeBucket;
                    }
                    freeBucket = FindCloserFreeBucket(freeBucket, ref freeDistance);
                } while (freeBucket >= 0);
            }
            Rehash();
            return AllocBucket(key, hash, MapHashToBucket(hash));
        }

        private int FindCloserFreeBucket(int freeBucket, ref int freeDistance)
        {
            for (var bucketsBackward = HopRange - 1; bucketsBackward > 0; --bucketsBackward) {
                var moveBucket = freeBucket - bucketsBackward;
                if (moveBucket < 0) {
                    moveBucket += Capacity;
                }
                var hopMask = buckets[moveBucket].HopMask;
                if (hopMask != 0) {
                    var moveBucketOffset = HopscotchUtils.FirstSetBitPosition(hopMask);
                    if (moveBucketOffset < bucketsBackward) {
                        var newFreeBucket = moveBucket + moveBucketOffset;
                        if (newFreeBucket >= Capacity) {
                            newFreeBucket -= Capacity;
                        }
                        Debug.Assert(buckets[newFreeBucket].IsOccupied);
                        Debug.Assert(!buckets[freeBucket].IsOccupied);
                        buckets[newFreeBucket].ClearOccupied();
                        buckets[freeBucket].HashField = buckets[newFreeBucket].HashField | OccupiedBit;
                        buckets[freeBucket].Key = buckets[newFreeBucket].Key;
                        buckets[freeBucket].Value = buckets[newFreeBucket].Value;
                        buckets[moveBucket].HopMask = (hopMask | (1u << bucketsBackward)) & ~(1u << moveBucketOffset);
                        freeDistance -= bucketsBackward - moveBucketOffset;
                        return newFreeBucket;
                    }
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindBucket(TKey key, uint hash, int startBucket)
        {
            var hopMask = buckets[startBucket].HopMask;
            while (hopMask != 0) {
                var i = HopscotchUtils.FirstSetBitPosition(hopMask);
                var bucket = startBucket + i;
                if (bucket >= Capacity) {
                    bucket -= Capacity;
                }
                Debug.Assert(buckets[bucket].IsOccupied);
                if (hash == buckets[bucket].Hash && key.Equals(buckets[bucket].Key)) {
                    return bucket;
                }
                hopMask &= ~(1u << i);
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindBucket(TKey key)
        {
            var hash = CalcHash(key);
            return FindBucket(key, hash, MapHashToBucket(hash));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindOrAllocBucket(TKey key)
        {
            var hash = CalcHash(key);
            var startBucket = MapHashToBucket(hash);
            var bucket = FindBucket(key, hash, startBucket);
            if (bucket >= 0) {
                return bucket;
            }
            return AllocBucket(key, hash, startBucket);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RemoveKey(TKey key)
        {
            var hash = CalcHash(key);
            var startBucket = MapHashToBucket(hash);
            var hopMask = buckets[startBucket].HopMask;
            while (hopMask != 0) {
                var i = HopscotchUtils.FirstSetBitPosition(hopMask);
                var bucket = startBucket + i;
                if (bucket >= Capacity) {
                    bucket -= Capacity;
                }
                Debug.Assert(buckets[bucket].IsOccupied);
                Debug.Assert(count > 0);
                if (hash == buckets[bucket].Hash && key.Equals(buckets[bucket].Key)) {
                    buckets[startBucket].HopMask &= ~(1u << i);
                    buckets[startBucket].ClearOccupied();
                    --count;
                    return bucket;
                }
                hopMask &= ~(1u << i);
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint CalcHash(TKey key)
        {
            return unchecked((uint)key.GetHashCode()) & HashMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MapHashToBucket(uint hash)
        {
            return unchecked((int)(hash % (uint)Capacity));
        }



        

        public int Capacity => buckets.Length;


        public delegate T ValueFunc<out T>(ref TValue value, bool found);

        private readonly TValue[] dummyValue = new TValue[1];

        public T WithValue<T>(TKey key, ValueFunc<T> func)
        {
            var bucket = FindOrAllocBucket(key);
            if (bucket >= 0) {
                return func(ref buckets[bucket].Value, true);
            }
            return func(ref dummyValue[0], false);
        }



        public struct TableEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Bucket[] buckets;
            private readonly int count;
            private int pos;

            public TableEnumerator(HopscotchHashTable<TKey, TValue> table)
            {
                buckets = table.buckets;
                count = table.count;
                pos = -1;
            }

            public bool MoveNext()
            {
                if (count > 0) {
                    while (++pos < buckets.Length) {
                        if (buckets[pos].IsOccupied) {
                            return true;
                        }
                    }
                }
                return false;
            }

            public void Reset()
            {
                pos = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (pos < 0) {
                        throw new InvalidOperationException();
                    }
                    return new KeyValuePair<TKey, TValue>(buckets[pos].Key, buckets[pos].Value);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
        


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new TableEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            var bucket = FindOrAllocBucket(item.Key);
            buckets[bucket].Value = item.Value;
        }

        public void Clear()
        {
            Array.Clear(buckets, 0, buckets.Length);
            count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var writeIndex = arrayIndex;
            if (count > 0) {
                for (var i = 0; i < buckets.Length; ++i) {
                    if (buckets[i].IsOccupied) {
                        array[writeIndex++] = new KeyValuePair<TKey, TValue>(buckets[i].Key, buckets[i].Value);
                    }
                }
            }
            Debug.Assert(writeIndex - arrayIndex == count);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => RemoveKey(item.Key) >= 0;

        public int Count => count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            var bucket = FindOrAllocBucket(key);
            buckets[bucket].Value = value;
        }

        public bool ContainsKey(TKey key) => FindBucket(key) >= 0;

        public bool Remove(TKey key) => RemoveKey(key) >= 0;

        public bool TryGetValue(TKey key, out TValue value)
        {
            var bucket = FindBucket(key);
            if (bucket >= 0) {
                value = buckets[bucket].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                var bucket = FindBucket(key);
                if (bucket >= 0) {
                    return buckets[bucket].Value;
                }
                throw new KeyNotFoundException();
            }
            set
            {
                var bucket = FindOrAllocBucket(key);
                buckets[bucket].Value = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>();
                if (count > 0) {
                    for (var i = 0; i < buckets.Length; ++i) {
                        if (buckets[i].IsOccupied) {
                            keys.Add(buckets[i].Key);
                        }
                    }
                }
                Debug.Assert(keys.Count == count);
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>();
                if (count > 0) {
                    for (var i = 0; i < buckets.Length; ++i) {
                        if (buckets[i].IsOccupied) {
                            values.Add(buckets[i].Value);
                        }
                    }
                }
                Debug.Assert(values.Count == count);
                return values;
            }
        }
    }


    public static class HopscotchUtils
    {
        public static readonly int[] TableSizes = {
            97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593, 49157,
            98317, 196613, 393241, 786433, 1572869, 3145739, 6291469,
            12582917, 25165843, 50331653, 100663319, 201326611, 402653189,
            805306457, 1610612741
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FirstSetBitPosition(uint v)
        {
            return MultiplyDeBruijnBitPosition32[unchecked(((v & (~v + 1u)) * 0x077CB531u) >> 27)];
        }
        // ReSharper disable once StaticMemberInGenericType
        private static readonly int[] MultiplyDeBruijnBitPosition32 = {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FirstSetBitPosition(ulong v)
        {
            return MultiplyDeBruijnBitPosition64[unchecked(((v ^ (v-1)) * 0x03F79D71B4CB0A89u) >> 58)];
        }
        // ReSharper disable once StaticMemberInGenericType
        private static readonly int[] MultiplyDeBruijnBitPosition64 = {
            0, 47, 1, 56, 48, 27, 2, 60, 57, 49, 41, 37, 28, 16, 3, 61,
            54, 58, 35, 52, 50, 42, 21, 44, 38, 32, 29, 23, 17, 11, 4, 62,
            46, 55, 26, 59, 40, 36, 15, 53, 34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30, 9, 24, 13, 18, 8, 12, 7, 6, 5, 63
        };
    }
}
