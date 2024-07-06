using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkNet.Model;
using СrossAppBot.Commands;
using СrossAppBot.Entities;
using СrossAppBot.Entities.Files;
using СrossAppBot.Events;
using CommandContext = СrossAppBot.Commands.CommandContext;
using CommandInfo = СrossAppBot.Commands.CommandInfo;
using Emoji = Discord.Emoji;

namespace СrossAppBot
{
    public class DiscordBotClient : AbstractBotClient, IEmojiable, IAddReaction, ISlashable
    {

        private DiscordSocketClient _client;
        private DiscordSocketConfig _config;

        public DiscordBotClient(string Token) : base("Discord", Token)
        {
            base.Token = Token;
        }

        public override async Task SendMessageAsync(string channelId, string text = null, string messageReferenceId = null, List<string> files = null)
        {
            string fullText = text;
            if (string.IsNullOrEmpty(fullText) && files == null) 
            {
                throw new NullReferenceException("Cannot send message without text and files.");
            }

            List<string> messages = new List<string>();
            
            if (fullText?.Length > 2000)
            {
                while (fullText.Length > 2000)
                {
                    int splitIndex = fullText.LastIndexOf(' ', 1999);

                    if (splitIndex == -1)
                        splitIndex = 1999;

                    string messagePart = fullText.Substring(0, splitIndex + 1);
                    messages.Add(messagePart);
                    fullText = fullText.Remove(0, splitIndex + 1).Trim();
                }

                if (!string.IsNullOrEmpty(fullText))
                    messages.Add(fullText);
            }
            else
            {
                messages.Add(fullText);
            }

            try
            {
                ulong parsedChannelId = ulong.Parse(channelId);
                IMessageChannel channel = await _client.GetChannelAsync(parsedChannelId) as IMessageChannel;
                if (channel == null)
                {
                    channel = await _client.GetDMChannelAsync(parsedChannelId) as IMessageChannel;
                }

                MessageReference messageReference = null;
                if (messageReferenceId != null)
                {
                    messageReference = new MessageReference(ulong.Parse(messageReferenceId));
                }

                if (messages.Count > 1)
                {
                    for (int i = 0; i <= messages.Count; i++)
                    {
                        await channel.SendMessageAsync(text: messages[i], messageReference: messageReference);
                        messageReference = null;
                    }
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

                    await channel.SendFilesAsync(attachments: attachments, text: messages.Any() ? messages[^1] : null, messageReference: messageReference);

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
            _config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.All, UseInteractionSnowflakeDate = false };
            _client = new DiscordSocketClient(_config);
            //_client.Log += LogAsync;           
            _client.Ready += OnBotConnected;
            _client.Disconnected += OnBotDisconnected;
            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += MessageUpdatedAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;
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
            base.Id = _client.CurrentUser.Id.ToString();
            await EventManager.CallEvent(new BotConnectedEvent(this));
        }

        public async Task OnBotDisconnected(Exception exception)
        {
            await EventManager.CallEvent(new BotConnectedEvent(this));
        }

        private async Task MessageReceivedAsync(SocketMessage originalMessage)
        {
            SocketGuild discordGuild = null;
            ChatGroup chatGuild = null;
            if (originalMessage.Channel is SocketGuildChannel)
            {
                discordGuild = (originalMessage.Channel as SocketGuildChannel).Guild;
                chatGuild = ConvertDiscordGuildToChatGuild(discordGuild);
            }

            ChatMessage message = await ConvertDiscordMessageToChatMessage(originalMessage);
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
                    ChatMessage message = await ConvertDiscordMessageToChatMessage(after);
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
            if (mention.StartsWith("<@") && mention.EndsWith(">"))
            {
                isMention = GetUserByMention(message, mention) != null;
            }
            return isMention;
        }

        public override Task<ChatUser> GetUserAsync(string userId, ChatGroup guild)
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
        public override Task<ChatGroup> GetGuildAsync(string guildId)
        {
            ChatGroup guild = null;
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
            return new ChatUser(discordUserId.ToString(), discordUser.Username, this, discordUser);
        }

        private ChatUser ConvertDiscordUserToChatUser(SocketUser discordUser)
        {
            ulong discordUserId = discordUser.Id;
            return new ChatUser(discordUserId.ToString(), discordUser.Username, this, discordUser);
        }

        private ChatGroup ConvertDiscordGuildToChatGuild(SocketGuild discordGuild)
        {
            return new ChatGroup(discordGuild.Id.ToString(), this, discordGuild.Name, discordGuild)
            {
                Id = discordGuild.Id.ToString(),
                Name = discordGuild.Name,
                Client = this
            };
        }

        private ChatChannel ConvertDiscordChannelToChatChannel(ISocketMessageChannel discordChannel)
        {
            ChatGroup guild = null;
            bool isPrivate = discordChannel is IDMChannel;
            if (!isPrivate)
            {
                guild = ConvertDiscordGuildToChatGuild((discordChannel as SocketGuildChannel).Guild);
            }
            return new ChatChannel(discordChannel.Name, discordChannel.Name, isPrivate, discordChannel as SocketGuildChannel, guild)
            {
                Id = discordChannel.Id.ToString(),
                Name = discordChannel.Name
            };
        }

        private async Task<ChatMessage> ConvertDiscordMessageToChatMessage(SocketMessage discordMessage)
        {
            SocketGuild discordGuild = null;
            ChatGroup chatGuild = null;
            ChatUser author = null;
            ISocketMessageChannel discordChannel = discordMessage.Channel;
            if (discordChannel is SocketGuildChannel publicDiscordChannel)
            {
                discordGuild = publicDiscordChannel.Guild;
                chatGuild = ConvertDiscordGuildToChatGuild(discordGuild);
                author = ConvertDiscordUserToChatUser(discordGuild.GetUser(discordMessage.Author.Id));
            }
            else
            {
                author = ConvertDiscordUserToChatUser(discordMessage.Author);
            }

            MessageReference discordMessageReference = discordMessage.Reference;
            ChatMessage messageReference = null;
            if (discordMessageReference != null)
            {
                messageReference = await ConvertDiscordMessageReferenceToChatMessage(discordMessageReference, discordMessage.Channel);
            }

            /* var repliedMessage = await discordMessage.Channel.GetMessageAsync((ulong)discordMessage.Reference.MessageId);
             var repliedAuthor = repliedMessage.Author;*/


            return new ChatMessage(discordMessage.Id.ToString(), author,
                chatGuild, this, ConvertDiscordChannelToChatChannel(discordChannel), discordMessage, text: discordMessage.Content, messageReference: messageReference,
                files: GetFilesFromAttachments(discordMessage.Attachments));
        }

        private async Task<ChatMessage> ConvertDiscordMessageReferenceToChatMessage(MessageReference discordMessageReference, ISocketMessageChannel discordChannel)
        {
            ChatUser author = null;
            ChatGroup guild = null;

            var discordMessage = await discordChannel.GetMessageAsync(discordMessageReference.MessageId.Value);
            ulong authorId = discordMessage.Author.Id;

            if (discordMessageReference.GuildId.IsSpecified)
            {
                SocketGuild discordGuild = _client.GetGuild(discordMessageReference.GuildId.Value);
                SocketGuildUser discordUser = discordGuild.GetUser(authorId);

                author = ConvertDiscordUserToChatUser(discordUser);
                guild = ConvertDiscordGuildToChatGuild(discordGuild);
            }
            else
            {
                SocketUser discordUser = (SocketUser)await _client.GetUserAsync(authorId);
                author = ConvertDiscordUserToChatUser(discordUser);
            }

            ChatMessage messageReference = new ChatMessage(id: discordMessageReference.MessageId.ToString(), author: author,
                                            guild: guild, client: this, channel: ConvertDiscordChannelToChatChannel(discordChannel),
                                            originalObject: discordMessage, files: GetFilesFromAttachments(discordMessage.Attachments.ToList()));

            return messageReference;
        }

        private List<ChatMessageFile> GetFilesFromAttachments(IEnumerable<IAttachment> attachments)
        {
            List<ChatMessageFile> files = new List<ChatMessageFile>();
            foreach (Discord.Attachment attachment in attachments)
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
            return files;
        }


        private SocketGuildUser GetDiscordUser(ChatUser user, ChatGroup guild)
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
        private SocketGuild GetDiscordGuild(ChatGroup guild)
        {
            return _client.GetGuild(ulong.Parse(guild.Id));
        }

        private SocketGuild GetDiscordGuild(ulong? guildId)
        {
            return _client.GetGuild(guildId.Value);
        }

        private static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        public async Task RegisterSlashCommand(AbstractCommand command)
        {
            SlashCommandBuilder slashCommand = new SlashCommandBuilder()
                                                    .WithName(command.Name)
                                                    .WithDescription(command.Description);
            foreach (CommandArgument argument in command.GetArguments())
            {
                ApplicationCommandOptionType slashCommandParameterType;
                switch (argument.Type)
                {
                    case var t when t == typeof(int):
                        slashCommandParameterType = ApplicationCommandOptionType.Integer;
                        break;
                    case var t when t == typeof(int?):
                        slashCommandParameterType = ApplicationCommandOptionType.Integer;
                        break;
                    case var t when t == typeof(bool):
                        slashCommandParameterType = ApplicationCommandOptionType.Boolean;
                        break;
                    case var t when t == typeof(bool?):
                        slashCommandParameterType = ApplicationCommandOptionType.Boolean;
                        break;
                    case var t when t == typeof(ChatUser):
                        slashCommandParameterType = ApplicationCommandOptionType.User;
                        break;
                    case var t when t == typeof(ChatChannel):
                        slashCommandParameterType = ApplicationCommandOptionType.Channel;
                        break;
                    case var t when t == typeof(double?):
                        slashCommandParameterType = ApplicationCommandOptionType.Number;
                        break;
                    case var t when t == typeof(float?):
                        slashCommandParameterType = ApplicationCommandOptionType.Number;
                        break;
                    case var t when t == typeof(double):
                        slashCommandParameterType = ApplicationCommandOptionType.Number;
                        break;
                    case var t when t == typeof(float):
                        slashCommandParameterType = ApplicationCommandOptionType.Number;
                        break;
                    case var t when t == typeof(ChatMessageFile):
                        slashCommandParameterType = ApplicationCommandOptionType.Attachment;
                        break;
                    default:
                        slashCommandParameterType = ApplicationCommandOptionType.String;
                        break;
                }
                CommandArgumentAttribute attributes = argument.Attributes;

                slashCommand.AddOption(attributes.Name, slashCommandParameterType, attributes.Description, attributes.Optional);
            }
            try
            {
                await _client.CreateGlobalApplicationCommandAsync(slashCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public async Task ExecuteSlashCommand(AbstractCommand command)
        {
            await command.Execute();
        }

        private async Task SlashCommandHandler(SocketSlashCommand slashCommand)
        {
            object[] arguments = Array.Empty<object>();
            if (slashCommand.Data.Options.Count > 0)
            {
                arguments = slashCommand.Data.Options.ToArray();
            }

            var discordUser = slashCommand.User;
            var discordGuildId = slashCommand.GuildId;
            if (discordGuildId != null)
            {
                var discordGuild = _client.GetGuild(discordGuildId.Value);
                discordUser = discordGuild.GetUser(slashCommand.User.Id);
            }

            CommandContext context = new CommandContext(

                async (text, reply, files) =>
                {
                    try
                    {
                        await slashCommand.RespondAsync(text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n" + e.ToString());
                    }
                },

                sender: ConvertDiscordUserToChatUser((SocketGuildUser)discordUser),
                guild: ConvertDiscordGuildToChatGuild(GetDiscordGuild(slashCommand.GuildId)),
                channel: ConvertDiscordChannelToChatChannel(slashCommand.Channel),
                client: this,
                message: null
            );

            AbstractCommand command = CommandManager.CreateExecutableCommandInstance(slashCommand.CommandName, arguments, context);

            await ExecuteSlashCommand(command);
        }

        public override Task<List<UserRight>> GetUserRights(ChatUser user, ChatGroup guild)
        {
            SocketGuild discordGuild = GetDiscordGuild(guild);
            SocketGuildUser discordUser = GetDiscordUser(user, guild);

            if (discordUser == null) return null;

            List<UserRight> rights = new List<UserRight>();
            if (discordUser.GuildPermissions.Administrator)
            {
                rights.Add(UserRight.Administrator);
            }
            if (discordUser.Id == (discordGuild.OwnerId))
            {
                rights.Add(UserRight.Owner);
            }

            return Task.FromResult(rights);
        }
    }
}
