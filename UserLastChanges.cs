using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList
{
    public class UserLastChanges
    {
        public Change<string> NameChange { get; private set; }
        public Change<EntryState> StateChange { get; private set; }
        public Change<ExistenceState> ExistenceChange { get; private set; }
        public bool IsNameChanged => NameChange != null;
        public bool IsStateChanged => StateChange != null;
        public bool IsExistenceChanged => ExistenceChange != null;
        public UserLastChanges()
        {
            NameChange = null;
            StateChange = null;
            ExistenceChange = null;
        }

        public void UpdateNameChange(string name, long timestamp) //Maybe reduce it to one method?
        {
            if (!IsNameChanged || NameChange.TimeStamp <= timestamp)
                NameChange = new Change<string>(name, timestamp);
        }

        public void UpdateStateChange(EntryState state, long timestamp)
        {
            if (!IsStateChanged || StateChange.TimeStamp < timestamp ||
                StateChange.TimeStamp == timestamp && state == EntryState.Undone)
                StateChange = new Change<EntryState>(state, timestamp);
        }

        public void UpdateExistenceChange(ExistenceState state, long timestamp)
        {
            if (!IsExistenceChanged || ExistenceChange.TimeStamp < timestamp ||
                ExistenceChange.TimeStamp == timestamp && state == ExistenceState.Removed)
                ExistenceChange = new Change<ExistenceState>(state, timestamp);
        }
    }
}
