using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Discord.WebSocket;
using СrossAppBot.Entities.Files;
using СrossAppBot.Entities;
using СrossAppBot.Events;


namespace СrossAppBot
{
    public class TelegramBotClient : AbstractBotClient
    {
        private Telegram.Bot.TelegramBotClient bot;
        private TelegramUsers telegramUsers;
        

        public TelegramBotClient(string Token, string dataFilePath = null) : base("Telegram", Token)
        {
            if (dataFilePath != null)
            {
                telegramUsers = new TelegramUsers(dataFilePath);
            }
        }

        public override async Task StartAsync()
        {
            bot = new Telegram.Bot.TelegramBotClient(Token);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            //base.Id =  bot.GetMeAsync().Id.ToString();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } 
            };


            await Task.Run(() =>
            {
                bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
            });

            await EventManager.CallEvent(new BotConnectedEvent(this));

            await Task.Delay(-1);
        }

        public override async Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null)
        {
            await bot.SendTextMessageAsync(chatId: long.Parse(channelId), text: text);
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        Message originalMessage = update.Message;
                        ChatMessage message = ConvertTelegramMessageToChatMessage(originalMessage);
                        await EventManager.CallEvent(new MessageReceivedEvent(message));
                        break;
                }

                if (update.EditedMessage != null) 
                {
                    Message originalMessage = update.EditedMessage;
                    ChatMessage message = ConvertTelegramMessageToChatMessage(originalMessage);

                    await EventManager.CallEvent(new MessageEditedEvent(message, null));
                }
            }
            );
        }

        public override string Mention(ChatUser user)
        {
            return $@"@{user.Name}";
        }

        public override string Mention(string localUserId)
        {
            return $@"@{localUserId}";
        }

        public override bool TextIsMention(ChatMessage message, string mention)
        {
            throw new NotImplementedException();
        }

        public override async Task<ChatUser> GetUserAsync(string userId, ChatGuild guild)
        {
            ChatUser user = null;
            Chat telegramGuild = null;
            long id = 0;
            long.TryParse(userId, out id);

            if (id != 0)
            {
                telegramGuild = await bot.GetChatAsync(new ChatId(id));
                if (telegramGuild != null)
                {
                    ChatMember telegramMember = await bot.GetChatMemberAsync(id, long.Parse(guild.Id));
                    User telegramUser = null;
                    if (telegramMember != null) 
                    {
                        telegramUser = telegramMember.User;
                    }
                    if (telegramUser != null) 
                    {
                        user = ConvertTelegramUserToChatUser(telegramUser, telegramGuild);
                    }
                }
            }

            return user;
        }

        public override async Task<ChatUser> GetUserByMention(ChatMessage message, string mention)
        {
            ChatUser user = null;

            if (mention.StartsWith("@"))
            {
                string username = mention.Substring(1);
                Chat chat = await bot.GetChatAsync(message.Channel.Id);
                long userId = telegramUsers.GetId(username);
                ChatMember telegramUser = await bot.GetChatMemberAsync(message.Channel.Id, userId); //GetChatMemberAsync

                if (telegramUser != null)
                {
                    user = ConvertTelegramUserToChatUser(telegramUser.User, chat);
                }
            }
            return user;
        }

        public override async Task<ChatChannel> GetChannelAsync(string channeId)
        {
            ChatChannel channel = null;
            Chat telegramChannel = null;
            long id = 0;
            long.TryParse(channeId, out id);

            if (id != 0)
            {
                telegramChannel = await bot.GetChatAsync(new ChatId(id));
                if (telegramChannel != null)
                {
                    channel = ConvertTelegramChannelToChatChannel(telegramChannel);
                }
            }

            return channel;
        }

        public override async Task<ChatGuild> GetGuildAsync(string guildId)
        {
            ChatGuild guild = null;
            Chat telegramGuild = null;
            long id = 0;
            long.TryParse(guildId, out id);

            if (id != 0)
            {
                telegramGuild = await bot.GetChatAsync(new ChatId(id));
                if (telegramGuild != null)
                {
                    guild = ConvertTelegramGuildToChatGuild(telegramGuild);
                }
            }

            return guild;
        }

        private ChatGuild ConvertTelegramGuildToChatGuild(Chat telegramChat)
        {       
            return new ChatGuild(telegramChat.Id.ToString(), this, telegramChat.Title, telegramChat);
        }

        private ChatChannel ConvertTelegramChannelToChatChannel(Chat telegramChat)
        {
            return new ChatChannel(telegramChat.Id.ToString(), telegramChat.Title, ConvertTelegramGuildToChatGuild(telegramChat), telegramChat);
        }

        private ChatUser ConvertTelegramUserToChatUser(User telegramUser, Chat telegramChat)
        {
            return new ChatUser(telegramUser.Id.ToString(), telegramUser.Username, this, telegramUser,
            isOwner: bot.GetChatMemberAsync(telegramChat.Id, telegramUser.Id).Result.Status == ChatMemberStatus.Creator,
            isAdmin: (bot.GetChatMemberAsync(telegramChat.Id, telegramUser.Id).Result.Status == ChatMemberStatus.Administrator)
            );
        }

        private ChatMessage ConvertTelegramMessageToChatMessage(Message message)
        {
            List<ChatMessageFile> files = new List<ChatMessageFile>();
            if (message.Photo != null && message.Photo.Length > 0)
            {
                // Retrieve photos
                foreach (var photo in message.Photo)
                {
                    // Access photo information (file_id, width, height, etc.)
                    string fileId = photo.FileId;
                    int width = photo.Width;
                    int height = photo.Height;
                    var file = bot.GetFileAsync(photo.FileId).Result;
                    string fileUrl = $"https://api.telegram.org/file/bot{this.Token}/{file.FilePath}";
                    files.Add(new ChatPicture(fileUrl, width, height));
                    // Process or store photo information as needed
                }
            }

            if (message.Video != null)
            {
                // Retrieve video
                string videoFileId = message.Video.FileId;
                int width = message.Video.Width;
                int height = message.Video.Height;
                // Process or store video information as needed
            }

            if (message.Audio != null)
            {
                // Retrieve audio
                string audioFileId = message.Audio.FileId;
                int duration = message.Audio.Duration;
                // Process or store audio information as needed
            }

            if (message.Document != null)
            {
                // Retrieve document (file)
                string documentFileId = message.Document.FileId;
                string mimeType = message.Document.MimeType;
                // Process or store document information as needed
            }

            Chat chat = message.Chat;
            return new ChatMessage(message.MessageId.ToString(), ConvertTelegramUserToChatUser(message.From, chat), ConvertTelegramGuildToChatGuild(chat), this, ConvertTelegramChannelToChatChannel(chat),
                message, text: message.Text, files: files);
        }

        private class TelegramUsers
        {
            private string filePath;

            public TelegramUsers(string filePath)
            {
                this.filePath = filePath;
                if (!System.IO.File.Exists(filePath)) 
                {
                    System.IO.File.Create(filePath);
                }
            }

            public string GetUsername(long id)
            {
                Dictionary<long, string> data = LoadData();
                if (data.ContainsKey(id))
                {
                    return data[id];
                }
                else
                {
                    return null;
                }
            }

            public List<long> GetIds(string username) 
            {
                string jsonString = System.IO.File.ReadAllText(filePath);
                Dictionary<long, string> data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<long, string>>(jsonString);
               
                return data.Keys.Where(key => key.ToString() == username).ToList();
            }

            public long GetId(string username)
            {
                string jsonString = System.IO.File.ReadAllText(filePath);
                Dictionary<long, string> data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<long, string>>(jsonString);

                return data.Keys.Where(key => key.ToString() == username).FirstOrDefault();
            }

            public void AddOrUpdateUser(long id, string username)
            {
                Dictionary<long, string> data = LoadData();
                if (data[id] != username)
                {
                    data[id] = username;
                    SaveData(data);
                }
            }

            private Dictionary<long, string> LoadData()
            {
                if (System.IO.File.Exists(filePath))
                {
                    string jsonData = System.IO.File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData);
                }
                else
                {
                    return new Dictionary<long, string>();
                }
            }

            private void SaveData(Dictionary<long, string> data)
            {
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, jsonData);
            }
        }
    }
}
