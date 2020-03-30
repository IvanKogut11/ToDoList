using System.Collections.Generic;
using System.Linq;

namespace ToDoList
{
    public class EntryChanges
    {
        public readonly ChangeAndUserSortedDictionary<ExistenceState> ExistenceChanges;
        public readonly ChangeAndUserSortedDictionary<EntryState> StateChanges;
        public readonly ChangeAndUserSortedDictionary<string> NameChanges;
        public EntryChanges()
        {
            ExistenceChanges = new ChangeAndUserSortedDictionary<ExistenceState>(new ExistenceStateCmp());
            StateChanges = new ChangeAndUserSortedDictionary<EntryState>(new StateCmp());
            NameChanges = new ChangeAndUserSortedDictionary<string>(new NameCmp());
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
