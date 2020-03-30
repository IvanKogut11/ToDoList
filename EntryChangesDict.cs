using System.Collections.Generic;
using System.Linq;

namespace ToDoList
{
    public class EntryChangesDict
    {
        private readonly SortedDictionary<long, SortedSet<(ExistenceState, int)>> existenceDict;
        private readonly SortedDictionary<long, SortedSet<(EntryState, int)>> stateDict;
        private readonly SortedDictionary<long, SortedSet<(string, int)>> nameDict;

        private class ReversedCmp : IComparer<long>
        {
            public int Compare(long x, long y)
            {
                return y.CompareTo(x);
            }
        }

        public EntryChangesDict()
        {
            existenceDict = new SortedDictionary<long, SortedSet<(ExistenceState, int)>>(new ReversedCmp());
            stateDict = new SortedDictionary<long, SortedSet<(EntryState, int)>>(new ReversedCmp());
            nameDict = new SortedDictionary<long, SortedSet<(string, int)>>(new ReversedCmp());
        }

        public bool ContainsExistenceChangeByUser(int userId, Change<ExistenceState> existenceChange)
        {
            var timeStamp = existenceChange.TimeStamp;
            var existenceState = existenceChange.Value;
            return existenceDict.ContainsKey(timeStamp) && existenceDict[timeStamp].Contains((existenceState, userId));
        }

        public bool ContainsStateChangeByUser(int userId, Change<EntryState> stateChange)
        {
            var timeStamp = stateChange.TimeStamp;
            var state = stateChange.Value;
            return stateDict.ContainsKey(timeStamp) && stateDict[timeStamp].Contains((state, userId));
        }
        public bool ContainsNameChangeByUser(int userId, Change<string> nameChange)
        {
            var timeStamp = nameChange.TimeStamp;
            var name = nameChange.Value;
            return nameDict.ContainsKey(timeStamp) && nameDict[timeStamp].Contains((name, userId));
        }

        public void ClearStateChange(int userId, Change<EntryState> stateChange)
        {
            stateDict[stateChange.TimeStamp].Remove((stateChange.Value, userId));
            if (stateDict[stateChange.TimeStamp].Count == 0)
                stateDict.Remove(stateChange.TimeStamp);
        }//Change

        public void ClearExistenceChange(int userId, Change<ExistenceState> existenceChange)
        {
            existenceDict[existenceChange.TimeStamp].Remove((existenceChange.Value, userId));
            if (existenceDict[existenceChange.TimeStamp].Count == 0)
                existenceDict.Remove(existenceChange.TimeStamp);
        }//Change

        public void ClearNameChange(int userId, Change<string> nameChange)
        {
            nameDict[nameChange.TimeStamp].Remove((nameChange.Value, userId));
            if (nameDict[nameChange.TimeStamp].Count == 0)
                nameDict.Remove(nameChange.TimeStamp);
        }//Change

        public void AddExistenceChange(int userId, Change<ExistenceState> existenceChange)
        {
            if (!existenceDict.ContainsKey(existenceChange.TimeStamp))
                existenceDict[existenceChange.TimeStamp] = new SortedSet<(ExistenceState, int)>(new ExistenceStateCmp());
            existenceDict[existenceChange.TimeStamp].Add((existenceChange.Value, userId));
        }

        public void AddNameChange(int userId, Change<string> nameChange)
        {
            if (!nameDict.ContainsKey(nameChange.TimeStamp))
                nameDict[nameChange.TimeStamp] = new SortedSet<(string, int)>(new NameCmp());
            nameDict[nameChange.TimeStamp].Add((nameChange.Value, userId));
        }

        public void AddStateChange(int userId, Change<EntryState> stateChange)
        {
            if (!stateDict.ContainsKey(stateChange.TimeStamp))
                stateDict[stateChange.TimeStamp] = new SortedSet<(EntryState, int)>(new StateCmp());
            stateDict[stateChange.TimeStamp].Add((stateChange.Value, userId));
        }
        public Change<ExistenceState> GetLastExistenceChangeOrNull()
        {
            if (existenceDict.Count != 0)
            {
                var lastExistence = existenceDict.First().Value.First();
                var lastTimeStamp = existenceDict.First().Key;
                return new Change<ExistenceState>(lastExistence.Item1, lastTimeStamp);
            }
            return null;
        }

        public Change<EntryState> GetLastStateChangeOrNull()
        {
            if (stateDict.Count != 0)
            {
                var lastState = stateDict.First().Value.First();
                var lastTimeStamp = stateDict.First().Key;
                return new Change<EntryState>(lastState.Item1, lastTimeStamp);
            }
            return null;
        }

        public Change<string> GetLastNameChangeOrNull()
        {
            if (nameDict.Count != 0)
            {
                var lastName = nameDict.First().Value.First();
                var lastTimeStamp = nameDict.First().Key;
                return new Change<string>(lastName.Item1, lastTimeStamp);
            }
            return null;
        }

        private class ExistenceStateCmp : IComparer<(ExistenceState, int)>
        {
            public int Compare((ExistenceState, int) x, (ExistenceState, int) y)
            {
                if (x.Item1 != y.Item1)
                {
                    if (x.Item1 == ExistenceState.Removed)
                        return -1;
                    return 1;
                }
                return x.Item2.CompareTo(y.Item2);
            }
        }

        private class StateCmp : IComparer<(EntryState, int)>
        {
            public int Compare((EntryState, int) x, (EntryState, int) y)
            {
                if (x.Item1 != y.Item1)
                {
                    if (x.Item1 == EntryState.Undone)
                        return -1;
                    return 1;
                }
                return x.Item2.CompareTo(y.Item2);
            }
        }

        private class NameCmp : IComparer<(string, int)>
        {
            public int Compare((string, int) x, (string, int) y)
            {
                if (x.Item2.CompareTo(y.Item2) != 0)
                    return x.Item2.CompareTo(y.Item2);
                return x.Item1.CompareTo(y.Item1);
            }
        }
    }
}
