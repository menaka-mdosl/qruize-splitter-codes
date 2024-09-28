using System;
using System.Collections.Generic;

namespace MDO2.Core.Util
{
    public static class CollectionsExtensions
    {
        public static bool IsNullOrEmpty<T>(this System.Collections.Generic.ICollection<T> cl)
        {
            return cl == null || cl.Count == 0;
        }
        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }

        public static bool IsArray(this object objectToCheck)
        {
            if (objectToCheck == null) return false;
            else
            {
                return objectToCheck.GetType().IsArray;
            }
        }
        public static bool IsEnumarable(this object objectToCheck)
        {
            if (objectToCheck == null) return false;
            else
            {
                return objectToCheck is System.Collections.IEnumerable;
            }
        }
    }
}
