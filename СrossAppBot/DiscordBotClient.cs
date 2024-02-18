using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using VkNet.Model;
using СrossAppBot.Entities;
using СrossAppBot.Entities.Files;
using СrossAppBot.Events;
using Emoji = Discord.Emoji;

namespace СrossAppBot
{
    public class DiscordBotClient : AbstractBotClient, IEmojiable, IAddReaction
    {

        private DiscordSocketClient _client;
        private DiscordSocketConfig _config;

        public DiscordBotClient(string Token) : base("Discord", Token)
        {
            base.Token = Token;
        }

        public override async Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null)
        {
            try
            {
                SocketTextChannel channel = (SocketTextChannel)_client.GetChannel(ulong.Parse(channelId));
                MessageReference messageReference = null;
                if (messageReferenceId != null)
                {
                    messageReference = new MessageReference(ulong.Parse(messageReferenceId));
                }
                if (files != null)
                {
                    List<FileAttachment> attachments = new List<FileAttachment>();
                    List<FileStream> streams = new List<FileStream>();
                    foreach (string file in files)
                    {
                        FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                        attachments.Add(new FileAttachment(stream, file));
                    }

                    await channel.SendFilesAsync(attachments: attachments, text: text, messageReference: messageReference);

                    await Task.Run(() =>
                    {
                        foreach (FileStream stream in streams)
                        {
                            stream.Close();
                        }
                    });
                }
                else
                {
                    await channel.SendMessageAsync(text: text, messageReference: messageReference);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public override async Task StartAsync()
        {


            _config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.All };
            _client = new DiscordSocketClient(_config);
            //_client.Log += LogAsync;           
            _client.Connected += OnBotConnected;
            _client.Disconnected += OnBotDisconnected;
            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += MessageUpdatedAsync;
            //base.Id = _client.CurrentUser.Id.ToString();

            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task OnBotConnected()
        {
            await EventManager.CallEvent(new BotConnectedEvent(this));
        }

        public async Task OnBotDisconnected(Exception exception)
        {
            await EventManager.CallEvent(new BotConnectedEvent(this));
        }

        private async Task MessageReceivedAsync(SocketMessage originalMessage)
        {
            SocketGuild discordGuild = (originalMessage.Channel as SocketGuildChannel).Guild;

            ChatGuild chatGuild = ConvertDiscordGuildToChatGuild(discordGuild);
            ChatMessage message = ConvertDiscordMessageToChatMessage(originalMessage);
            await EventManager.CallEvent(new MessageReceivedEvent(message));
        }
        private async Task MessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            SocketGuild discordGuild = (after.Channel as SocketGuildChannel).Guild;
            var beforeMessage = before.HasValue ? before.Value as SocketUserMessage : null;

            if (beforeMessage != null && after != null)
            {
                if (beforeMessage.Content != after.Content)
                {
                    ChatMessage message = ConvertDiscordMessageToChatMessage(after);
                    await EventManager.CallEvent(new MessageEditedEvent(message, null));
                }
            }
        }

        public override string Mention(ChatUser user)
        {
            return $"<@{user.Id}>";
        }

        public override string Mention(string localUserId)
        {
            return $"<@{localUserId}>";
        }

        public override bool TextIsMention(ChatMessage message, string mention)
        {
            bool isMention = false;
            if (mention.StartsWith("<@") & mention[mention.Length - 1].Equals(">"))
            {
                isMention = GetUserByMention(message, mention) != null;
            }
            return isMention;
        }

        public override Task<ChatUser> GetUserAsync(string userId, ChatGuild guild)
        {
            ChatUser user = null;
            SocketGuildUser discordUser = null;
            ulong id = 0;
            ulong.TryParse(userId, out id);

            if (id != 0)
            {
                discordUser = _client.GetUser(id) as SocketGuildUser;
                if (discordUser != null)
                {
                    user = ConvertDiscordUserToChatUser(discordUser);
                }
            }
            return Task.FromResult(user);
        }

        public override Task<ChatUser> GetUserByMention(ChatMessage message, string mention)
        {
            ChatUser user = null;
            if (mention.StartsWith("<@") && mention.EndsWith(">"))
            {
                string id = mention.Replace("<@", "");
                id = id.Replace(">", "");

                SocketGuildUser discordUser = GetDiscordGuild(message.Guild).GetUser(ulong.Parse(id));
                if (discordUser != null)
                {
                    user = ConvertDiscordUserToChatUser(discordUser);
                }
            }
            return Task.FromResult(user);
        }

        //is not async
        public override Task<ChatChannel> GetChannelAsync(string localChanneId)
        {
            ChatChannel channel = null;
            SocketChannel discordChannel = null;
            ulong id = 0;
            ulong.TryParse(localChanneId, out id);

            if (id != 0)
            {
                discordChannel = _client.GetChannel(id);
                if (discordChannel != null)
                {
                    channel = ConvertDiscordChannelToChatChannel(discordChannel as ISocketMessageChannel);
                }
            }
            return Task.FromResult(channel);
        }

        //is not async 
        public override Task<ChatGuild> GetGuildAsync(string guildId)
        {
            ChatGuild guild = null;
            SocketGuild discordGuild = null;
            ulong id = 0;
            ulong.TryParse(guildId, out id);

            if (id != 0)
            {
                discordGuild = _client.GetGuild(id);
                if (discordGuild != null)
                {
                    guild = ConvertDiscordGuildToChatGuild(discordGuild);
                }
            }

            return Task.FromResult(guild);
        }

        public async Task AddReaction(ChatMessage message, string reaction)
        {
            ISocketMessageChannel channel = _client.GetChannel(ulong.Parse(message.Channel.Id)) as ISocketMessageChannel;
            IMessage iMessage = await channel.GetMessageAsync(ulong.Parse(message.Id));

            if (Emoji.TryParse(reaction, out Emoji emoji))
            {
                await iMessage.AddReactionAsync(emoji);
            }
            else if (Emote.TryParse(reaction, out Emote emote))
            {
                await iMessage.AddReactionAsync(emote);
            }
            else
            {
                throw new InvalidCastException();
            }
        }


        private ChatUser ConvertDiscordUserToChatUser(SocketGuildUser discordUser)
        {
            ulong discordUserId = discordUser.Id;
            return new ChatUser(discordUserId.ToString(), discordUser.Username, this, discordUser, discordUser.GuildPermissions.Administrator, discordUser.Guild.OwnerId == discordUserId);
        }

        private ChatGuild ConvertDiscordGuildToChatGuild(SocketGuild discordGuild)
        {
            return new ChatGuild(discordGuild.Id.ToString(), this, discordGuild.Name, discordGuild)
            {
                Id = discordGuild.Id.ToString(),
                Name = discordGuild.Name,
                Client = this
            };
        }

        private ChatChannel ConvertDiscordChannelToChatChannel(ISocketMessageChannel discordChannel)
        {
            return new ChatChannel(discordChannel.Name, discordChannel.Name, ConvertDiscordGuildToChatGuild((discordChannel as SocketGuildChannel).Guild), discordChannel as SocketGuildChannel)
            {
                Id = discordChannel.Id.ToString(),
                Name = discordChannel.Name
            };
        }

        private ChatMessage ConvertDiscordMessageToChatMessage(SocketMessage discordMessage)
        {
            SocketGuild discordGuild = (discordMessage.Channel as SocketGuildChannel).Guild;
            List<ChatMessageFile> files = new List<ChatMessageFile>();
            foreach (Discord.Attachment attachment in discordMessage.Attachments)
            {
                string url = attachment.Url;
                ChatMessageFile file = null;
                switch (GetFileExtensionFromUrl(url))
                {
                    case ".mp3":
                    case ".wav":
                    case ".ogg":
                        file = new ChatAudio(url);
                        break;
                    case ".mp4":
                    case ".webm":
                    case ".mkv":
                        file = new ChatVideo(url, (int)attachment.Width, (int)attachment.Height);
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".JPG":
                    case ".JPEG":
                    case ".png":
                    case ".PNG":
                    case ".gif":
                    case ".gifv":
                        file = new ChatPicture(url, (int)attachment.Width, (int)attachment.Height);
                        break;
                    default:
                        file = new ChatFile(url);
                        break;
                }
                files.Add(file);
            }

            return new ChatMessage(discordMessage.Id.ToString(), ConvertDiscordUserToChatUser(discordGuild.GetUser(discordMessage.Author.Id)),
                ConvertDiscordGuildToChatGuild(discordGuild), this, ConvertDiscordChannelToChatChannel(discordMessage.Channel), discordMessage, text: discordMessage.Content, files: files);
        }

        private SocketGuildUser GetDiscordUser(ChatUser user, ChatGuild guild)
        {
            return GetDiscordGuild(guild).GetUser(ulong.Parse(user.Id));
        }
        public bool IsReactableEmoji(string content)
        {
            Emoji.TryParse(content, out Emoji emoji);
            if (emoji != null)
            {
                return true;
            }
            Emote.TryParse(content, out Emote emote);
            if (emote != null)
            {
                return true;
            }
            return false;
        }
        private SocketGuild GetDiscordGuild(ChatGuild guild)
        {
            return _client.GetGuild(ulong.Parse(guild.Id));
        }

        private static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }
    }
}
