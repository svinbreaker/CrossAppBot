using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using СrossAppBot.Entities;
using СrossAppBot.Entities.Files;
<<<<<<< HEAD
using СrossAppBot.Events;
=======
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
using Emoji = Discord.Emoji;

namespace СrossAppBot
{
<<<<<<< HEAD
    public class DiscordBotClient : AbstractBotClient, IEmojiable, IAddReaction
=======
    public class DiscordBotClient : AbstractBotClient, IAddReaction
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
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
<<<<<<< HEAD

                        FileStream stream = new FileStream(file, FileMode.Open);
                        attachments.Add(new FileAttachment(stream, file));

=======
                        
                            FileStream stream = new FileStream(file, FileMode.Open);
                            attachments.Add(new FileAttachment(stream, file));
                        
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
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
<<<<<<< HEAD
            }
            catch (Exception e)
=======
            }catch(Exception e) 
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
            {
                Console.WriteLine(e.ToString());
            }
        }


        public override async Task StartAsync()
        {


            _config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.All };
            _client = new DiscordSocketClient(_config);
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.Connected += OnBotConnected;
            _client.Disconnected += OnDiscordDisconnected;

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
            Console.WriteLine("Discord запустился...");
        }

        public async Task OnDiscordDisconnected(Exception exception)
        {
            Console.WriteLine("Discord отключился...");
        }

        public async Task OnBotDisconnected()
        {
            OnDiscordDisconnected(null);
        }


<<<<<<< HEAD
        private async Task MessageReceivedAsync(SocketMessage discordMessage)
        {
            SocketGuild discordGuild = (discordMessage.Channel as SocketGuildChannel).Guild;
=======
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            SocketGuild discordGuild = (message.Channel as SocketGuildChannel).Guild;
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
            ChatGuild chatGuild = ConvertDiscordGuildToChatGuild(discordGuild);



            /* List<AppPicture> pictures = new List<AppPicture>();

<<<<<<< HEAD
             foreach (Attachment attachment in discordMessage.Attachments)
=======
             foreach (Attachment attachment in message.Attachments)
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
             {

                 if (attachment.Width.HasValue && attachment.Height.HasValue)
                 {
                     pictures.Add(new AppPicture(attachment.Url, attachment.Width.Value, attachment.Height.Value));
                 }
             }*/

<<<<<<< HEAD

            ChatMessage message = ConvertDiscordMessageToChatMessage(discordMessage);
            await EventManager.CallEvent(new MessageReceivedEvent(message));

=======
            await Task.Run(() =>
            {
                CallOnMessageReceived(ConvertDiscordMessageToChatMessage(message));
            });
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
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

<<<<<<< HEAD
                //SocketGuildUser discordUser = _client.GetGuild(ulong.Parse(discordMessage.Guild.LocalId)).GetUserAsync(ulong.Parse(id));
=======
                //SocketGuildUser discordUser = _client.GetGuild(ulong.Parse(message.Guild.LocalId)).GetUserAsync(ulong.Parse(id));
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
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
                    case ".avi":
                    case ".mkv":
                        file = new ChatVideo(url, (int)attachment.Width, (int)attachment.Height);
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
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

<<<<<<< HEAD
        public bool IsEmoji(string content)
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

=======
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
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
<<<<<<< HEAD

=======
>>>>>>> 0b4914b28d0eab8a003fd1a5178091bce1317389
    }
}
