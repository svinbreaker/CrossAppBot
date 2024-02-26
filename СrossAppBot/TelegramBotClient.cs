﻿using System;
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


        List<string> imageFormats = new List<string>
        {
            ".jpg", ".jpeg", ".png", ".tiff", ".bmp", ".webp"
        };

        List<string> videoFormats = new List<string>
        {
            ".mp4", ".mov"
        };

        List<string> audioFormats = new List<string>
        {
            ".mp3", ".m4a", ".ogg", ".flac", ".wav"
        };
        List<string> animationFormats = new List<string>
        {
            ".gif"
        };

        List<string> documentFormats = new List<string>
        {
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".rtf", ".zip"
        };

        public TelegramBotClient(string Token, string dataFilePath) : base("Telegram", Token)
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
            base.Id = (await bot.GetMeAsync()).Id.ToString();

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
            long chatId = long.Parse(channelId);
            int? messageReference = null;
            if (messageReferenceId != null)
            {
                messageReference = int.Parse(messageReferenceId);
            }
            List<string> fileExtensions = files?.Select(file => Path.GetExtension(file))?.ToList();

            if (files == null)
            {
                await bot.SendTextMessageAsync(chatId: chatId, text: text, replyToMessageId: messageReference);
            }
            else if (!fileExtensions.Any(ext => audioFormats.Contains(ext)) ||
                         fileExtensions.All(ext => audioFormats.Contains(ext)) ||
                         fileExtensions.All(ext =>
                             imageFormats.Contains(ext) || videoFormats.Contains(ext) ||
                             audioFormats.Contains(ext) || animationFormats.Contains(ext)))
            {
                IAlbumInputMedia[] album = new IAlbumInputMedia[files.Count];

                var streams = new List<FileStream>();
                var memoryStreams = new List<MemoryStream>();

                for (int i = 0; i < album.Length; i++)
                {
                    string filePath = files[i];
                    string extension = Path.GetExtension(filePath);
                    IAlbumInputMedia inputMedia;

                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    streams.Add(fileStream);
                    
                    byte[] fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                    var memoryStream = new MemoryStream(fileBytes);

                    if (imageFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaPhoto(InputFile.FromStream(memoryStream, filePath));
                    }
                    else if (audioFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaAudio(InputFile.FromStream(memoryStream, filePath));
                    }
                    else if (videoFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaVideo(InputFile.FromStream(memoryStream, filePath));
                    }
                    else
                    {
                        inputMedia = new InputMediaDocument(InputFile.FromStream(memoryStream, filePath));
                    }
                    album[i] = inputMedia;
                }
                await bot.SendMediaGroupAsync(chatId: chatId, media: album, replyToMessageId: messageReference);
                streams.ForEach(s => s.Close());
                memoryStreams.ForEach(s => s.Close());
            }
            else
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var filePath = files[i];
                    string extension = Path.GetExtension(filePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] fileBytes = new byte[fileStream.Length];
                        await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);

                        using (var memoryStream = new MemoryStream(fileBytes))
                        {
                            InputFile inputFile = InputFile.FromStream(memoryStream, filePath);
                            if (imageFormats.Contains(extension))
                            {
                                await bot.SendPhotoAsync(chatId: chatId, caption: text, photo: inputFile, replyToMessageId: messageReference);
                            }
                            else if (videoFormats.Contains(extension))
                            {
                                await bot.SendVideoAsync(chatId: chatId, caption: text, video: inputFile, replyToMessageId: messageReference);
                            }
                            else if (audioFormats.Contains(extension))
                            {
                                await bot.SendAudioAsync(chatId: chatId, caption: text, audio: inputFile, replyToMessageId: messageReference);
                            }
                            else if (animationFormats.Contains(extension))
                            {
                                await bot.SendAnimationAsync(chatId: chatId, caption: text, animation: inputFile, replyToMessageId: messageReference);
                            }
                            else
                            {
                                await bot.SendDocumentAsync(chatId: chatId, caption: text, document: inputFile, replyToMessageId: messageReference);
                            }
                            if (i > 0) text = null;
                        }
                    }
                }
            }

            /*else if (files.Count == 1)
            {
                var filePath = files[0];
                string extension = Path.GetExtension(filePath);

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    InputFile inputFile = InputFile.FromStream(fileStream, filePath);
                    if (imageFormats.Contains(extension))
                    {
                        await bot.SendPhotoAsync(chatId: chatId, caption: text, photo: inputFile, replyToMessageId: messageReference);
                    }
                    else if (videoFormats.Contains(extension))
                    {
                        await bot.SendVideoAsync(chatId: chatId, caption: text, video: inputFile, replyToMessageId: messageReference);
                    }
                    else if (audioFormats.Contains(extension))
                    {
                        await bot.SendAudioAsync(chatId: chatId, caption: text, audio: inputFile, replyToMessageId: messageReference);
                    }
                    else if (animationFormats.Contains(extension))
                    {
                        await bot.SendAnimationAsync(chatId: chatId, caption: text, animation: inputFile, replyToMessageId: messageReference);
                    }
                    else
                    {
                        await bot.SendDocumentAsync(chatId: chatId, caption: text, document: inputFile, replyToMessageId: messageReference);
                    }
                }
            }
            else
            {
                IAlbumInputMedia[] album = new IAlbumInputMedia[files.Count];
                List<FileStream> streams = new List<FileStream>();
                for (int i = 0; i < album.Length; i++)
                {
                    string filePath = files[i];
                    string extension = Path.GetExtension(filePath);
                    IAlbumInputMedia inputMedia;

                    streams.Add(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    FileStream fileStream = streams[i];

                    if (imageFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaPhoto(InputFile.FromStream(fileStream, filePath));
                    }
                    else if (audioFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaAudio(InputFile.FromStream(fileStream, filePath));
                    }
                    else if (videoFormats.Contains(extension))
                    {
                        inputMedia = new InputMediaVideo(InputFile.FromStream(fileStream, filePath));
                    }
                    else
                    {
                        inputMedia = new InputMediaDocument(InputFile.FromStream(fileStream, filePath));
                    }
                    album[i] = inputMedia;
                }
                try
                {
                    await bot.SendMediaGroupAsync(chatId: chatId, media: album, replyToMessageId: messageReference);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                streams.ForEach(s => s.Close());
            }*/
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
                        ChatMessage message = await ConvertTelegramMessageToChatMessage(originalMessage);
                        await EventManager.CallEvent(new MessageReceivedEvent(message));
                        break;
                }

                if (update.EditedMessage != null)
                {
                    Message originalMessage = update.EditedMessage;
                    ChatMessage message = await ConvertTelegramMessageToChatMessage(originalMessage);

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
                ChatMember telegramUser;
                try
                {
                    telegramUser = await bot.GetChatMemberAsync(message.Channel.Id, userId); //GetChatMemberAsync throws an exception when user not found instead of returning null
                }
                catch
                {
                    telegramUser = null;
                }

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
            ChatGuild guild = null;
            bool isPrivate = telegramChat.Type == ChatType.Private;
            if (!isPrivate) 
            {
                guild = ConvertTelegramGuildToChatGuild(telegramChat);
            }
            return new ChatChannel(telegramChat.Id.ToString(), telegramChat.Title, isPrivate, telegramChat, guild: guild);
        }

        private ChatUser ConvertTelegramUserToChatUser(User telegramUser, Chat telegramChat)
        {
            return new ChatUser(telegramUser.Id.ToString(), telegramUser.Username, this, telegramUser,
            isOwner: bot.GetChatMemberAsync(telegramChat.Id, telegramUser.Id).Result.Status == ChatMemberStatus.Creator,
            isAdmin: (bot.GetChatMemberAsync(telegramChat.Id, telegramUser.Id).Result.Status == ChatMemberStatus.Administrator)
            );
        }

        private async Task<ChatMessage> ConvertTelegramMessageToChatMessage(Message message)
        {
            List<ChatMessageFile> files = new List<ChatMessageFile>();
            if (message.Photo != null && message.Photo.Length > 0)
            {
                // Retrieve photos
                if (message.Photo != null)
                {
                    foreach (PhotoSize photo in message.Photo)
                    {
                        // Access photo information (file_id, width, height, etc.)
                        string fileId = photo.FileId;
                        int width = photo.Width;
                        int height = photo.Height;
                        string fileUrl = await GetTelegramFileUrlById(fileId);
                        files.Add(new ChatPicture(fileUrl, width, height));
                    }
                }
            }

            if (message.Video != null)
            {
                // Retrieve video
                string fileId = message.Video.FileId;
                int width = message.Video.Width;
                int height = message.Video.Height;
                string fileUrl = await GetTelegramFileUrlById(fileId);
                files.Add(new ChatPicture(fileUrl, width, height));
            }

            if (message.Audio != null)
            {
                // Retrieve audio
                string audioFileId = message.Audio.FileId;
                int duration = message.Audio.Duration;
            }

            if (message.Document != null)
            {
                // Retrieve document (filePath)
                string fileId = message.Document.FileId;
                string mimeType = message.Document.MimeType;
                string fileUrl = await GetTelegramFileUrlById(fileId);
                files.Add(new ChatFile(fileUrl));
            }

            ChatMessage messageReference = null;

            // Forwarded message's message ID
            int? forwardedMessageId = message.ForwardFromMessageId;

            if (forwardedMessageId != null)
            {
                long forwardedChatId = message.ForwardFromChat.Id;
                Message forwardedMessage = await bot.ForwardMessageAsync(message.Chat.Id, forwardedChatId, forwardedMessageId.Value);
                messageReference = await ConvertTelegramMessageToChatMessage(forwardedMessage);
            }
            Chat chat = message.Chat;
            return new ChatMessage(message.MessageId.ToString(), ConvertTelegramUserToChatUser(message.From, chat), ConvertTelegramGuildToChatGuild(chat), this, ConvertTelegramChannelToChatChannel(chat),
                message, text: message.Text, messageReference: messageReference, files: files);
        }

        private async Task<string> GetTelegramFileUrlById(string id)
        {
            var file = await bot.GetFileAsync(id);
            if (file == null)
            {
                return null;
            }
            else
            {
                return $"https://api.telegram.org/filePath/bot{this.Token}/{file.FilePath}";
            }
        }

        private class TelegramUsers
        {
            private string filePath;

            public TelegramUsers(string filePath)
            {
                this.filePath = filePath;
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.WriteAllText(filePath, "{}");
                    /*using (FileStream fileStream = System.IO.File.Create(filePath))
                    {
                        byte[] emptyJsonObject = System.Text.Encoding.UTF8.GetBytes("{}");
                        fileStream.Write(emptyJsonObject, 0, emptyJsonObject.Length);
                    }*/
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
