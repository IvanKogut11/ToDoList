using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList
{
    public class Change<T>
    {
        public readonly ChangeType Type;
        public readonly T Value;
        public readonly long TimeStamp;

        public Change(T value, long timeStamp)
        {
            var changeType = typeof(T);
            if (changeType == typeof(string))
                Type = ChangeType.nameChange;
            else if (changeType == typeof(EntryState))
                Type = ChangeType.stateChange;
            else if (changeType == typeof(ExistenceState))
                Type = ChangeType.existenceStateChange;
            else
                throw new System.Exception("Unexpected type of change's value");
            Value = value;
            TimeStamp = timeStamp;
        }
    }
}
