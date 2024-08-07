﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.StringEnums;
using System.Threading.Channels;
using СrossAppBot.Entities.Files;
using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;
using VkNet.Abstractions;
using СrossAppBot.Entities;
using СrossAppBot.Events;


namespace СrossAppBot
{
    public class VkBotClient : AbstractBotClient, IEmojiable, IAddReaction
    {
        private VkApi _api = new VkApi();
        public ulong GroupId;

        private static List<string> _emojis = new List<string> { "❤️", "🔥", "😂", "👍", "💩", "❓", "😭", "👎", "👌", "😡", "😃", "🤔", "🙏", "😘", "😍", "🎉" };

        public VkBotClient(string Token, ulong GroupId) : base("Vk", Token)
        {
            base.Token = Token;
            this.GroupId = GroupId;
        }

        private int updateTick = 1000;

        public override async Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null)
        {
            try
            {
                List<MediaAttachment> attachments = new List<MediaAttachment>();
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        string extension = Path.GetExtension(file);
                        /*UploadServerInfo server = await GetUploadServer(fileExtension);
                        string response = await UploadFile(server.UploadUrl, filePath, fileExtension);
                        IReadOnlyCollection<Photo> photo = _api.Photo.SaveMessagesPhoto(response);*/
                        var attachment = await GetAttachment(file);
                        attachments.Add(attachment);
                    }
                }
                long peerId = long.Parse(channelId);

                MessageForward messageForward = null;
                if (messageReferenceId != null)
                {
                    messageForward = new MessageForward();
                    messageForward.ConversationMessageIds = new long[]
                    { _api.Messages.GetByConversationMessageId(peerId, new ulong[] { ulong.Parse(messageReferenceId) }, fields: new List<string>()).Items.FirstOrDefault().ConversationMessageId.Value };
                    messageForward.PeerId = peerId;
                    messageForward.IsReply = true;
                }

                MessagesSendParams sendMessageParams = new MessagesSendParams
                {
                    RandomId = new Random().Next(), // Generate uploadServer random ID for the vkMessage
                    PeerId = peerId, // Specify the recipient's ID
                    Message = text, // Set the vkMessage content      
                    Attachments = attachments,
                    Forward = messageForward
                };

                await Task.Run(() => _api.Messages.SendAsync(sendMessageParams));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public override async Task StartAsync()
        {
            _api.Authorize(new ApiAuthParams
            {
                AccessToken = Token,

            });



            LongPollServerResponse longPollServer = null;
            try
            {
                longPollServer = _api.Groups.GetLongPollServer(groupId: GroupId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.ToString());
            }
            var ts = longPollServer.Ts;

            await EventManager.CallEvent(new BotConnectedEvent(this));

            while (true)
            {
                await Task.Delay(updateTick);
                BotsLongPollHistoryResponse poll = null;
                try
                {
                    poll = _api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams
                    {
                        Server = longPollServer.Server,
                        Ts = ts,
                        Key = longPollServer.Key,
                        Wait = 3
                    });

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + "\n" + exception.ToString());
                }
                if (poll == null) continue;

                ts = poll.Ts;


                List<GroupUpdate> updates = poll.Updates;
                //Console.WriteLine(updates.Count);

                List<Message> messages = updates
                     .Where(u => u.Instance is MessageNew)
                     .Select(u => ((MessageNew)u.Instance).Message)
                     .ToList();

                if (messages.Count > 0)
                {
                    foreach (Message originalMessage in messages)
                    {
                        ChatMessage message = ConvertVkMessageToChatMessage(originalMessage);
                        await EventManager.CallEvent(new MessageReceivedEvent(message));
                    }
                }
            }
        }


        public override string Mention(ChatUser user)
        {
            return $"[id{user.Id}|{user.Name}]";
        }

        public override string Mention(string userId)
        {
            User user = GetVkUserById(long.Parse(userId));
            return $"[id{userId}|{user.FirstName}]";
        }

        public override bool TextIsMention(ChatMessage message, string mention)
        {
            string pattern = @"\[(id)(\d+)\|.*\]";
            return Regex.IsMatch(mention, pattern);
        }

        public override async Task<ChatUser> GetUserAsync(string userId, ChatGroup guild)
        {
            User vkUser = GetVkUserById(long.Parse(userId));
            Conversation vkGuild = GetConversationById(guild.Id);

            return ConvertVkUserToChatUser(vkUser, vkGuild);
        }

        public override async Task<ChatUser> GetUserByMention(ChatMessage message, string mention)
        {

            //[id{ user.Id}|{ user.Name}]

            long userId = long.Parse(mention.Split('|')[0].Substring(3));
            User chatUser = _api.Users.Get(new[] { userId }, ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Nickname).FirstOrDefault();
            var conversations = _api.Messages.GetConversationsById(peerIds: new[] { long.Parse(message.Channel.Id) }, extended: true);
            Conversation chat = conversations.Items.FirstOrDefault();
            return ConvertVkUserToChatUser(chatUser, chat);
        }

        public override async Task<ChatChannel> GetChannelAsync(string channelId)
        {
            return ConvertVkChannelToChatChannel(GetConversationById(channelId));
        }

        public override async Task<ChatGroup> GetGuildAsync(string guildId)
        {
            return ConvertVkGuildToChatGuild(GetConversationById(guildId));
        }

        private ChatGroup ConvertVkGuildToChatGuild(Conversation conversation)
        {
            string conversationName = null;
            ConversationChatSettings settings = conversation.ChatSettings;
            if (settings != null)
            {
                conversationName = settings.Title;
            }
            return new ChatGroup(conversation.Peer.ToString(), this, conversationName, conversation);
        }

        private ChatChannel ConvertVkChannelToChatChannel(Conversation conversation)
        {
            ChatGroup guild = null;
            bool isPrivate = conversation.Peer.Id < 2000000000;
            if (!isPrivate)
            {
                guild = ConvertVkGuildToChatGuild(conversation);
            }
            string conversationName = null;
            ConversationChatSettings settings = conversation.ChatSettings;
            if (settings != null) //The settings are null if the bot is not have administrator rights
            {
                conversationName = settings.Title;
            }

            return new ChatChannel(conversation.Peer.Id.ToString(), conversationName, isPrivate, conversation, guild);
        }

        private ChatUser ConvertVkUserToChatUser(User vkUser, Conversation conversation)
        {
            long vkUserId = vkUser.Id;
            bool isAdmin = false;
            bool isOwner = false;
            ConversationChatSettings chatSettings = conversation.ChatSettings;
            if (chatSettings != null)
            {
                if (chatSettings.AdminIds != null & chatSettings.AdminIds.Count > 0)
                {
                    isAdmin = chatSettings.AdminIds.Contains(vkUser.Id);
                }
            }

            return new ChatUser(vkUserId.ToString(), vkUser.FirstName, this, conversation);
        }

        private ChatMessage ConvertVkMessageToChatMessage(Message message)
        {
            long? id = message.ConversationMessageId.Value;
            Conversation conversation = GetConversationByMessage(message);
            User author = GetVkUserById(message.FromId);
            List<ChatMessageFile> files = new List<ChatMessageFile>();
            if (message.Attachments.Count > 0)
            {
                foreach (Attachment attachment in message.Attachments)
                {
                    if (attachment.Instance is Photo photo)
                    {
                        PhotoSize largestPhoto = photo.Sizes[photo.Sizes.Count - 1];
                        string photoUrl = largestPhoto.Url.ToString();

                        files.Add(new ChatPicture(photoUrl, (int)largestPhoto.Width, (int)largestPhoto.Height));
                    }
                }
            }

            ChatGroup guild = ConvertVkGuildToChatGuild(conversation);
            return new ChatMessage(id.ToString(), ConvertVkUserToChatUser(author, conversation), guild,
                this, ConvertVkChannelToChatChannel(conversation), message,
                text: message.Text, files: files);
        }

        private Conversation GetConversationByMessage(Message message)
        {
            return _api.Messages.GetConversationsById(peerIds: new[] { message.PeerId.Value }, extended: true).Items.FirstOrDefault();
        }

        private Conversation GetConversationById(string conversationId)
        {
            long? id = long.Parse(conversationId);

            List<long> conversationIds = new List<long> { (long)id };
            return _api.Messages.GetConversationsById(conversationIds).Items.FirstOrDefault();
        }

        private User GetVkUserById(long? userId)
        {
            User user = null;
            if (userId.HasValue)
            {
                user = _api.Users.Get(new long[] { (long)userId }).FirstOrDefault();
            }
            return user;
        }

        private async Task<MediaAttachment> GetAttachment(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);
            switch (fileExtension)
            {
                case ".png":
                case ".jpg":
                case ".gif":
                case ".tif":
                    var uploadServer = await _api.Photo.GetMessagesUploadServerAsync(0);
                    // Получение массива байтов из файла
                    var data = File.ReadAllBytes(filePath);

                    // Создание запроса на загрузку файла на сервер
                    using (var client = new HttpClient())
                    {
                        var requestContent = new MultipartFormDataContent();
                        var content = new ByteArrayContent(data);
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        requestContent.Add(content, "filePath", $"filePath.{fileExtension}");

                        var response = client.PostAsync(uploadServer.UploadUrl, requestContent).Result;

                        var result = Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
                        var photo = await _api.Photo.SaveMessagesPhotoAsync(result);
                        return photo[0];
                    }
                case ".avi":
                case ".mp4":
                case ".3gp":
                case ".mpeg":
                case ".mov":
                case ".flv":
                case ".f4v":
                case ".wmv":
                case ".mkv":
                case ".webm":
                case ".vob":
                case ".rm":
                case ".rmvb":
                case ".m4v":
                case ".mpg":
                case ".ogv":
                case ".ts":
                case ".m2ts":
                case ".mts":
                /*var video = await _api.Video.SaveAsync(new VideoSaveParams
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    Description = null,
                    Wallpost = false, 
                    GroupId = null,
                    IsPrivate = true,
                });
                return video;*/

                default:
                    throw new ArgumentException("File format is not supported: " + fileExtension);
            }
        }


        private async Task<string> UploadFile(string serverUrl, string filePath, string fileExtension)
        {
            // Получение массива байтов из файла
            var data = File.ReadAllBytes(filePath);

            // Создание запроса на загрузку файла на сервер
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var content = new ByteArrayContent(data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(content, "filePath", $"filePath.{fileExtension}");

                var response = client.PostAsync(serverUrl, requestContent).Result;
                return Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }

        public bool IsReactableEmoji(string content)
        {
            return _emojis.Contains(content);
        }

        public Task AddReaction(ChatMessage message, string reaction)
        {
            throw new NotImplementedException();
        }

        public override Task<List<UserRight>> GetUserRights(ChatUser user, ChatGroup guild)
        {
            Conversation vkChannel = GetConversationById(guild.Id);
            User vkUser = GetVkUserById(long.Parse(user.Id));

            List<UserRight> rights = new List<UserRight>();

            ConversationChatSettings chatSettings = vkChannel.ChatSettings;
            if (chatSettings == null) return null;


            if (chatSettings.AdminIds != null & chatSettings.AdminIds.Count > 0)
            {
                if (chatSettings.AdminIds.Contains(vkUser.Id))
                {
                    rights.Add(UserRight.Administrator);
                }
            }
            if (chatSettings.OwnerId == vkUser.Id)
            {
                rights.Add(UserRight.Administrator);
            }
            return Task.FromResult(rights);
        }
    }
}
