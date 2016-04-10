using System;

namespace NeoGui.Core
{
    public struct Pair<T1, T2> : IComparable<Pair<T1, T2>>, IEquatable<Pair<T1, T2>>
        where T1: IComparable<T1>, IEquatable<T1>
        where T2: IComparable<T2>, IEquatable<T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        // aliases
        public T1 Key => Item1;
        public T2 Value => Item2;

        public Pair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public int CompareTo(Pair<T1, T2> other)
        {
            var res = Item1.CompareTo(other.Item1);
            if (res < 0) {
                return -1;
            }
            if (res > 0) {
                return 1;
            }
            return Item2.CompareTo(other.Item2);
        }

        public bool Equals(Pair<T1, T2> other)
        {
            return Item1.Equals(other.Item1) && Item2.Equals(other.Item2);
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> && Equals((Pair<T1, T2>)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (Item1.GetHashCode() * 397) ^ Item2.GetHashCode();
            }
        }
        
        public static bool operator ==(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return !a.Equals(b);
        }

        public static bool operator <(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return a.CompareTo(b) > 0;
        }
    }
}
