namespace VoiceOfReason
{
    public class PendingEventRegistry<T>
    {
        private Dictionary<string, Func<T, Task>> m_PendingEvents = new Dictionary<string, Func<T, Task>>();

        public Task AddEvent(string id, Func<T, Task> callback)
        {
            m_PendingEvents[id] = callback;
            return Task.CompletedTask;
        }

        public async Task RecieveEvent(string eventID, T obj)
        {
            if (m_PendingEvents.ContainsKey(eventID))
            {
                await m_PendingEvents[eventID].Invoke(obj);
                m_PendingEvents.Remove(eventID);
            }
        }
    }
}
