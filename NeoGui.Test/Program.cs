using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeoGui.Core;

namespace NeoGui.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                var table = new HopscotchHashTable<int, int>();
                Debug.Assert(!table.ContainsKey(123));
                Debug.Assert(table.Count == 0);

                table.Add(123, 99);
                Debug.Assert(table.ContainsKey(123));
                Debug.Assert(table.Count == 1);

                Debug.Assert(table.Remove(123));
                Debug.Assert(!table.ContainsKey(123));
                Debug.Assert(table.Count == 0);

                table[55] = 2;
                table[53] = 4;
                Debug.Assert(table.Count == 2);

                var keys = table.Keys.ToArray();
                Array.Sort(keys);
                Debug.Assert(keys[0] == 53);
                Debug.Assert(keys[1] == 55);

                var values = table.Values.ToArray();
                Array.Sort(values);
                Debug.Assert(values[0] == 2);
                Debug.Assert(values[1] == 4);

                var enumerator = table.GetEnumerator();
                Debug.Assert(enumerator.MoveNext());
                var p = enumerator.Current;
                Debug.Assert(p.Key == 53 || p.Key == 55);
                Debug.Assert(p.Value == 2 || p.Value == 4);
                Debug.Assert(enumerator.MoveNext());
                p = enumerator.Current;
                Debug.Assert(p.Key == 53 || p.Key == 55);
                Debug.Assert(p.Value == 2 || p.Value == 4);
                Debug.Assert(!enumerator.MoveNext());


            }
            return;
            /*const int count = 24;
            var seed = 0;
            while (true) {
                var rnd = new Random(++seed);
                var keySet = new HashSet<int>();
                while (keySet.Count < count) {
                    keySet.Add(rnd.Next());
                }
                var keys = keySet.ToArray();
                
                var table = new HopscotchHashTable<int, int>();
                for (var i = 0; i < count; ++i) {
                    var key = keys[i];
                    table[key] = key;
                }
                for (var i = 0; i < count; ++i) {
                    var key = keys[i];
                    if (table[key] != key) {
                        throw new Exception("noo");
                    }
                }
            }*/


            /*for (var i = 0; i < 32; ++i) {
                var v = 1u << i;
                var j = FirstSetBitPosition(v);
                Debug.WriteLine($"{i}: {j} ({i == j})");
            }
            return;*/

            /*{
                var rnd = new Random(1);
                for (var iter = 0; iter < 1000000; ++iter) {
                    var v = (ulong)rnd.Next(2000000000);
                    if (v == 0) {
                        continue;
                    }
                    
                    var i = HopscotchUtils.FirstSetBitPosition(v);
                    var j = 0;
                    for (; j < 32; ++j) {
                        if ((v & (1u << j)) != 0) {
                            break;
                        }
                    }
                    if (i != j) {
                        throw new Exception("mismatch");
                    }
                }
                Debug.WriteLine("OK");
            }
            return;*/


            {
                const int count = 5000000;
                int[] keys;
                {
                    var rnd = new Random(1234);
                    var keySet = new HashSet<int>();
                    while (keySet.Count < count) {
                        keySet.Add(rnd.Next(100000000));
                    }
                    keys = keySet.ToArray();
                    //keys = keySet.Select(x => x.ToString()).ToArray();
                }
                /*keys = new int[count];
                for (var i = 0; i < count; ++i) {
                    keys[i] = i;
                }*/

                // HopscotchHashTable
                // Dictionary
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    HopscotchHashTable<int, int> table = null;
                    for (var k = 0; k < 1; ++k) {
                        table = new HopscotchHashTable<int, int>();
                        if (table.Count != 0) {
                            throw new Exception("noooo");
                        }
                        for (var i = 0; i < count; ++i) {
                            var key = keys[i];
                            table[key] = key;
                        }
                        if (table.Count != count) {
                            throw new Exception("noooo");
                        }
                    }
                    for (var k = 0; k < 10; ++k) {
                        for (var i = 0; i < count; ++i) {
                            var key = keys[i];
                            if (table[key] != key) {
                                throw new Exception("noooo");
                            }
                        }
                    }
                    watch.Stop();
                    Console.WriteLine("Hopscotch time: " + (watch.ElapsedMilliseconds / 1000.0));
                }
            }
        }
    }
}
