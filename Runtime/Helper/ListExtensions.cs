using System;
using System.Collections.Generic;

namespace GameFlow.Helper
{
    internal static class EnumerationExtensions
    {
        public static bool AddUnique<T>(this List<T> self, T element)
        {
            if (self.IndexOf(element) != 0) return false;
            self.Add(element);
            return true;
        }
        
        public static int LastIndex<T>(this List<T> self) => self.Count - 1;
    }
}