using System.Collections;
using System.Collections.Generic;
using System;

namespace ToDoList
{
    public class ToDoList : IToDoList
    {
        private readonly Dictionary<int, Dictionary<int, UserLastChanges>> usersLastChangesToEntry = new Dictionary<int, Dictionary<int, UserLastChanges>>();
        private readonly Dictionary<int, bool> isUserAllowed = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> isEntryAdded = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> isEntryDone = new Dictionary<int, bool>();
        private readonly Dictionary<int, EntryChanges> entriesChanges = new Dictionary<int, EntryChanges>();


        private void UpdateEntryState(int entryId)
        {
            var x = entriesChanges[entryId].StateChanges.GetLastChangeOrNull();
            isEntryDone[entryId] = x != null && x.Value == EntryState.Done;
        }
        private void UpdateEntryExistence(int entryId)
        {
            var x = entriesChanges[entryId].ExistenceChanges.GetLastChangeOrNull();
            var prevState = isEntryAdded[entryId];
            isEntryAdded[entryId] = x != null && x.Value == ExistenceState.Added;
            if (prevState && !isEntryAdded[entryId])
                Count--;
            else if (!prevState && isEntryAdded[entryId])
                Count++;
        }

        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            if (IsNewEntry(entryId))
                InitializeNewEntry(entryId);
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            UpdateUserExistenceChangeToEntry(entryId, userId, ExistenceState.Added, timestamp);
            UpdateUserNameChangeToEntry(entryId, userId, name, timestamp);
            if (isUserAllowed[userId])
            {
                var userLastChanges = usersLastChangesToEntry[userId][entryId];
                entriesChanges[entryId].ExistenceChanges.AddChange(userId, userLastChanges.ExistenceChange);
                entriesChanges[entryId].NameChanges.AddChange(userId, userLastChanges.NameChange);
                UpdateEntryExistence(entryId);
            }
        }

        public void RemoveEntry(int entryId, int userId, long timestamp)
        {
            if (IsNewEntry(entryId))
                InitializeNewEntry(entryId);
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            UpdateUserExistenceChangeToEntry(entryId, userId, ExistenceState.Removed, timestamp);
            if (isUserAllowed[userId])
            {
                var userLastChanges = usersLastChangesToEntry[userId][entryId];
                entriesChanges[entryId].ExistenceChanges.AddChange(userId, userLastChanges.ExistenceChange);
                UpdateEntryExistence(entryId);
            }
        }

        public void MarkDone(int entryId, int userId, long timestamp)
        {
            if (IsNewEntry(entryId))
                InitializeNewEntry(entryId);
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            UpdateUserStateChangeToEntry(entryId, userId, EntryState.Done, timestamp);
            if (isUserAllowed[userId])
            {
                var userLastChanges = usersLastChangesToEntry[userId][entryId];
                entriesChanges[entryId].StateChanges.AddChange(userId, userLastChanges.StateChange);
                UpdateEntryState(entryId);
            }
        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            if (IsNewEntry(entryId))
                InitializeNewEntry(entryId);
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            UpdateUserStateChangeToEntry(entryId, userId, EntryState.Undone, timestamp);
            if (isUserAllowed[userId])
            {
                var userLastChanges = usersLastChangesToEntry[userId][entryId];
                entriesChanges[entryId].StateChanges.AddChange(userId, userLastChanges.StateChange);
                UpdateEntryState(entryId);
            }
        }

        public void DismissUser(int userId)
        {
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            isUserAllowed[userId] = false;
            if (!usersLastChangesToEntry.ContainsKey(userId))
                return;
            foreach (var entryId in usersLastChangesToEntry[userId].Keys)
            {
                var userChanges = usersLastChangesToEntry[userId][entryId];
                if (userChanges.IsStateChanged && entriesChanges[entryId].StateChanges.ContainsChangeByUser(userId, userChanges.StateChange))
                {
                    entriesChanges[entryId].StateChanges.ClearChange(userId, userChanges.StateChange);
                    UpdateEntryState(entryId);
                }
                if (userChanges.IsExistenceChanged && entriesChanges[entryId].ExistenceChanges.ContainsChangeByUser(userId, userChanges.ExistenceChange))
                {
                    entriesChanges[entryId].ExistenceChanges.ClearChange(userId, userChanges.ExistenceChange);
                    UpdateEntryExistence(entryId);
                }
                if (userChanges.IsNameChanged && entriesChanges[entryId].NameChanges.ContainsChangeByUser(userId, userChanges.NameChange))
                    entriesChanges[entryId].NameChanges.ClearChange(userId, userChanges.NameChange);
            }
        }

        public void AllowUser(int userId)
        {
            if (IsNewUser(userId))
                InitializeNewUser(userId);
            isUserAllowed[userId] = true;
            if (!usersLastChangesToEntry.ContainsKey(userId))
                return;
            foreach (var changedByUserEntry in usersLastChangesToEntry[userId].Keys)
            {
                var userLastChangesToEntry = usersLastChangesToEntry[userId][changedByUserEntry];
                ImplementUserChanges(changedByUserEntry, userId, userLastChangesToEntry);
            }
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            foreach(var entryId in entriesChanges.Keys)
            {
                if (isEntryAdded[entryId])
                    yield return GetEntry(entryId);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; private set; } = 0;

        private bool IsNewUser(int userId) => !isUserAllowed.ContainsKey(userId);

        private void InitializeNewUser(int userId) => isUserAllowed[userId] = true;

        private bool IsNewEntry(int entryId) => !entriesChanges.ContainsKey(entryId);

        private void InitializeNewEntry(int entryId)
        {
            isEntryDone[entryId] = false;
            entriesChanges[entryId] = new EntryChanges();
            isEntryAdded[entryId] = false;
        }

        private bool HasUserDoneAnyChangesToEntry(int entryId, int userId)
            => usersLastChangesToEntry.ContainsKey(userId) && usersLastChangesToEntry[userId].ContainsKey(entryId);

        private void InitializeUserChangesToEntry(int entryId, int userId)
        {
            if (!usersLastChangesToEntry.ContainsKey(userId))
                usersLastChangesToEntry[userId] = new Dictionary<int, UserLastChanges>();
            if (!usersLastChangesToEntry[userId].ContainsKey(entryId))
                usersLastChangesToEntry[userId][entryId] = new UserLastChanges();
        }

        private Entry GetEntry(int entryId)
        {
            var lastStateChange = isEntryDone[entryId] ? EntryState.Done : EntryState.Undone;
            var lastNameChange = entriesChanges[entryId].NameChanges.GetLastChangeOrNull();
            return new Entry(entryId, lastNameChange.Value, lastStateChange);
        }

        private void UpdateUserStateChangeToEntry(int entryId, int userId, EntryState entryState, long timestamp)
        {
            if (IsNewUser(userId))
                throw new Exception(string.Format("Can't update changes of not initialized user {}", userId));
            if (!HasUserDoneAnyChangesToEntry(entryId, userId))
                InitializeUserChangesToEntry(entryId, userId);
            if (usersLastChangesToEntry[userId][entryId].IsStateChanged &&
                    entriesChanges[entryId].StateChanges.ContainsChangeByUser(userId, usersLastChangesToEntry[userId][entryId].StateChange)) //BAD!!!
                entriesChanges[entryId].StateChanges.ClearChange(userId, usersLastChangesToEntry[userId][entryId].StateChange);
            usersLastChangesToEntry[userId][entryId].UpdateStateChange(entryState, timestamp);
        }

        private void UpdateUserExistenceChangeToEntry(int entryId, int userId, ExistenceState existenceState, long timestamp)
        {
            if (IsNewUser(userId))
                throw new Exception(string.Format("Can't update changes of not initialized user {}", userId));
            if (!HasUserDoneAnyChangesToEntry(entryId, userId))
                InitializeUserChangesToEntry(entryId, userId);
            if (usersLastChangesToEntry[userId][entryId].IsExistenceChanged &&
                    entriesChanges[entryId].ExistenceChanges.ContainsChangeByUser(userId, usersLastChangesToEntry[userId][entryId].ExistenceChange)) //BAD!!!!
                entriesChanges[entryId].ExistenceChanges.ClearChange(userId, usersLastChangesToEntry[userId][entryId].ExistenceChange);
            usersLastChangesToEntry[userId][entryId].UpdateExistenceChange(existenceState, timestamp);
        }

        private void UpdateUserNameChangeToEntry(int entryId, int userId, string name, long timestamp)
        {
            if (IsNewUser(userId))
                throw new Exception(string.Format("Can't update changes of not initialized user {}", userId));
            if (!HasUserDoneAnyChangesToEntry(entryId, userId))
                InitializeUserChangesToEntry(entryId, userId);
            if (usersLastChangesToEntry[userId][entryId].IsNameChanged &&
                    entriesChanges[entryId].NameChanges.ContainsChangeByUser(userId, usersLastChangesToEntry[userId][entryId].NameChange)) //BAD!!!!
                entriesChanges[entryId].NameChanges.ClearChange(userId, usersLastChangesToEntry[userId][entryId].NameChange);
            usersLastChangesToEntry[userId][entryId].UpdateNameChange(name, timestamp);
        }

        private void ImplementUserChanges(int entryId, int userId, UserLastChanges changes)
        {
            if (changes.IsNameChanged)
                AddEntry(entryId, userId, changes.NameChange.Value, changes.NameChange.TimeStamp);
            if (changes.IsStateChanged)
            {
                if (changes.StateChange.Value == EntryState.Done)
                    MarkDone(entryId, userId, changes.StateChange.TimeStamp);
                else
                    MarkUndone(entryId, userId, changes.StateChange.TimeStamp);
            }
            if (changes.IsExistenceChanged)
            {
                if (changes.ExistenceChange.Value == ExistenceState.Added)
                    AddEntry(entryId, userId, changes.NameChange.Value, changes.ExistenceChange.TimeStamp);
                else
                    RemoveEntry(entryId, userId, changes.ExistenceChange.TimeStamp);
            }
        }
    }
}