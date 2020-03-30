using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList
{
    public class ChangeAndUserSortedDictionary<T>
    {
        private readonly SortedDictionary<long, SortedSet<(T, int)>> sortedDict;
        private readonly IComparer<(T, int)> changesComparer;

        public ChangeAndUserSortedDictionary(IComparer<(T, int)> changesComparer = null)
        {
            this.changesComparer = changesComparer;
            sortedDict = new SortedDictionary<long, SortedSet<(T, int)>>(new ReversedCmp());
        }

        public bool ContainsChangeByUser(int userId, Change<T> change)
        {
            var timeStamp = change.TimeStamp;
            var value = change.Value;
            return sortedDict.ContainsKey(timeStamp) && sortedDict[timeStamp].Contains((value, userId));
        }

        public void ClearChange(int userId, Change<T> change)
        {
            if (!ContainsChangeByUser(userId, change))
                throw new Exception("No such change by this user");
            sortedDict[change.TimeStamp].Remove((change.Value, userId));
            if (sortedDict[change.TimeStamp].Count == 0)
                sortedDict.Remove(change.TimeStamp);
        }

        public void AddChange(int userId, Change<T> change)
        {
            if (!sortedDict.ContainsKey(change.TimeStamp))
                sortedDict[change.TimeStamp] = new SortedSet<(T, int)>(changesComparer);
            sortedDict[change.TimeStamp].Add((change.Value, userId));
        }

        public Change<T> GetLastChangeOrNull()
        {
            if (sortedDict.Count != 0)
            {
                var lastChangeValueIdPair = sortedDict.First().Value.First();
                var lastTimeStamp = sortedDict.First().Key;
                return new Change<T>(lastChangeValueIdPair.Item1, lastTimeStamp);
            }
            return null;
        }

        private class ReversedCmp : IComparer<long>
        {
            public int Compare(long x, long y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
