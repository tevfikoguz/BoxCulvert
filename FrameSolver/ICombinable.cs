using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public interface ICombinable<T>
    {
        bool IsCompatibleWith(T other);

        T Abs();

        T Min(T other);
        T Max(T other);

        T Multiply(double factor);
        T Add(T other);
        T MultiplyAndAdd(double factor, T other);
    }

    public class CombinableList<T> : List<T> where T : ICombinable<T>
    {
        public CombinableList<T> Min(CombinableList<T> other)
        {
            if (Count == 0) return other;
            
            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].Min(other[i]));
            }
            return result;
        }

        public CombinableList<T> Max(CombinableList<T> other)
        {
            if (Count == 0) return other;

            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].Max(other[i]));
            }
            return result;
        }

        public CombinableList<T> AbsMin(CombinableList<T> other)
        {
            if (Count == 0) return other;

            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].Abs().Min(other[i].Abs()));
            }
            return result;
        }

        public CombinableList<T> AbsMax(CombinableList<T> other)
        {
            if (Count == 0) return other;

            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].Abs().Max(other[i].Abs()));
            }
            return result;
        }

        public CombinableList<T> Multiply(double factor)
        {
            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                result.Add(this[i].Multiply(factor));
            }
            return result;
        }

        public CombinableList<T> Add(CombinableList<T> other)
        {
            if (Count == 0) return other;

            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].Add(other[i]));
            }
            return result;
        }

        public CombinableList<T> MultiplyAndAdd(double factor, CombinableList<T> other)
        {
            if (Count == 0) return other.Multiply(factor);

            CombinableList<T> result = new CombinableList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsCompatibleWith(other[i])) throw new ArgumentException("Lists are not compatible.");
                result.Add(this[i].MultiplyAndAdd(factor, other[i]));
            }
            return result;
        }
    }
}
