using Discord;
using Discord.WebSocket;

namespace VoiceOfReason
{
    public class MultiPageModal
    {
        const int MODAL_MAX_COMPONENTS = 5;

        private InteractionManager m_InteractionManager;
        private List<Modal> m_Modals;
        private int m_PagesCompleted;
        private int m_TotalPages => m_Modals.Count;
        private string m_ModalName;
        private Dictionary<string, string> m_FieldIDNameMap = new Dictionary<string, string>();
        private Dictionary<string, string> m_FieldIDDataMap = new Dictionary<string, string>();
        private TaskCompletionSource<Dictionary<string, string>> m_NameDataCompletionSource = new TaskCompletionSource<Dictionary<string, string>>();

        public MultiPageModal(string name, List<Field> fields, InteractionManager interactionManager)
        {
            m_InteractionManager = interactionManager;
            m_ModalName = name;
            List<TextInputBuilder> textComponents = CollateModalFields(fields);
            m_Modals = ChunkTextComponentsToModals(textComponents);
        }

        public async Task<Dictionary<string, string>> BeginSendingMultiPageModal(IDiscordInteraction interaction)
        {
            Modal currModal = m_Modals.First();
            await m_InteractionManager.AddModalCallback(currModal.CustomId, OnModalPageRecievedEvent);
            await interaction.RespondWithModalAsync(currModal);
            return await m_NameDataCompletionSource.Task;
        }

        private List<TextInputBuilder> CollateModalFields(List<Field> fields)
        {
            List<TextInputBuilder> textComponents = new List<TextInputBuilder>();
            foreach (Field field in fields)
            {
                AddFieldToModalRecursive(field, textComponents, "");
            }
            return textComponents;
        }

        private List<Modal> ChunkTextComponentsToModals(List<TextInputBuilder> textComponents)
        {
            List<Modal> modals = textComponents.Chunk(MODAL_MAX_COMPONENTS).Select((compChunk, index) =>
            {
                string pageNumber = "", pageID = "";
                if (textComponents.Count > MODAL_MAX_COMPONENTS)
                {
                    string pageCount = textComponents.Count.DivCeil(MODAL_MAX_COMPONENTS).ToString();
                    pageNumber = $"Page {index + 1} of {pageCount}";
                    pageID = pageCount;
                }
                ModalBuilder builder = new ModalBuilder()
                    .WithTitle($"{m_ModalName} {pageNumber}")
                    .WithCustomId(m_InteractionManager.CreateCustomID());
                foreach (TextInputBuilder field in compChunk)
                    builder.AddTextInput(field);
                return builder.Build();
            }).ToList();
            return modals;
        }

        private List<TextInputBuilder> AddFieldToModalRecursive
        (
            Field field,
            List<TextInputBuilder> workingList,
            string prefix
        )
        {

            if (field.Subfields is not null)
                foreach (Field subField in field.Subfields)
                {
                    AddFieldToModalRecursive(subField, workingList, $"{prefix}{field.Label} → ");
                }
            else
            {
                string id = m_InteractionManager.CreateCustomID();
                m_FieldIDNameMap[id] = field.id;
                workingList.Add(new TextInputBuilder()
                    .WithCustomId(id)
                    .WithLabel($"{prefix}{field.Label}")
                    .WithStyle(TextInputStyle.Short)
                );
            }
            return workingList;
        }

        private async Task OnModalPageRecievedEvent(SocketModal modalData)
        {
            m_PagesCompleted += 1;
            foreach (SocketMessageComponentData component in modalData.Data.Components)
                m_FieldIDDataMap[component.CustomId] = component.Value;
            if (m_PagesCompleted < m_TotalPages)
            {
                Embed embed = BuildNextPageEmbed();
                string buttonID;
                MessageComponent button = BuildNextPageButton(out buttonID);
                await m_InteractionManager.AddButtonCallback(buttonID, OnNextPageInteractedEvent);
                await modalData.RespondAsync(embed: embed, components: button, ephemeral: true);
            }
            else
            {
                Embed embed = BuildFinalPageEmbed();
                await modalData.RespondAsync(embed: embed, ephemeral: true);
                Dictionary<string, string> modalResults = m_FieldIDNameMap.Join
                (
                    m_FieldIDDataMap, name => name.Key, data => data.Key,
                    (name, data) => new KeyValuePair<string, string>(name.Value, data.Value)
                ).ToDictionary();
                m_NameDataCompletionSource.SetResult(modalResults);
            }
        }

        private async Task OnNextPageInteractedEvent(SocketMessageComponent buttonData)
        {
            Modal currModal = m_Modals[m_PagesCompleted];
            await m_InteractionManager.AddModalCallback(currModal.CustomId, OnModalPageRecievedEvent);
            await buttonData.RespondWithModalAsync(currModal);
        }

        private Embed BuildNextPageEmbed()
        {
            return new EmbedBuilder()
                .WithTitle($"Page {m_PagesCompleted} of {m_TotalPages} Completed!")
                .WithDescription("This is a multi-page form. Press **Next Page** to continue.")
                .Build();
        }

        private MessageComponent BuildNextPageButton(out string id)
        {
            id = m_InteractionManager.CreateCustomID();
            return new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Next Page")
                    .WithStyle(ButtonStyle.Success)
                    .WithCustomId(id)
                )
                .Build();
        }

        private Embed BuildFinalPageEmbed()
        {
            return new EmbedBuilder()
                .WithTitle($"{m_ModalName} completed!")
                .Build();
        }
    }
}
