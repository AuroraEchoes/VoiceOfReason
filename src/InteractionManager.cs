using Discord;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class InteractionManager
    {
        private DiscordSocketClient m_Client;
        private PendingEventRegistry<SocketModal> m_PendingModals = new PendingEventRegistry<SocketModal>();
        private PendingEventRegistry<SocketMessageComponent> m_PendingButtons = new PendingEventRegistry<SocketMessageComponent>();


        public InteractionManager(DiscordSocketClient client)
        {
            m_Client = client;
            m_Client.ModalSubmitted += OnModalSubmittedEvent;
            m_Client.ButtonExecuted += OnButtonInteractionEvent;
        }

        ~InteractionManager()
        {
            m_Client.ModalSubmitted -= OnModalSubmittedEvent;
            m_Client.ButtonExecuted -= OnButtonInteractionEvent;
        }

        public string CreateCustomID()
        {
            Guid modalID = Guid.NewGuid();
            return modalID.ToString();
        }

        public async Task AddModalCallback(string modalID, Func<SocketModal, Task> callback)
        {
            await m_PendingModals.AddEvent(modalID, callback);
        }

        private async Task OnModalSubmittedEvent(SocketModal modal)
        {
            await m_PendingModals.RecieveEvent(modal.Data.CustomId, modal);
        }

        public async Task AddButtonCallback(string buttonID, Func<SocketMessageComponent, Task> callback)
        {
            await m_PendingButtons.AddEvent(buttonID, callback);
        }

        private async Task OnButtonInteractionEvent(SocketMessageComponent component)
        {
            await m_PendingButtons.RecieveEvent(component.Data.CustomId, component);
        }

        public async Task<SocketTextChannel> GetChannel(ulong channelID)
        {
            // NOTE: This could cause problems if we’re ever using anything other than text channels
            return (SocketTextChannel)await m_Client.GetChannelAsync(channelID);
        }
    }
}
