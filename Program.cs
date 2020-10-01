using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using ImmersiveInCharacterChat.Config;
using Microsoft.Extensions.Configuration;

namespace ImmersiveIC
{
    public class Program
    {
        private static RequestOptions DeleteMessageRequestOptions { get; } = new RequestOptions { AuditLogReason = "ReplaceHook" };

        private ulong ChannelId => Config.ChannelId;
        private string WebhookUrl => Config.Webhook;

        private DiscordSocketClient Client { get; }

        private IConfigurationRoot ConfigRoot { get; }
        private BotConfig Config { get; }

        public static Task Main(string[] args) => new Program().RunAsync();

        public Program()
        {
            ConfigRoot = LoadConfiguration();
            Config = ConfigRoot.Get<BotConfig>();

            Client = new DiscordSocketClient();
            Client.Log += (msg) => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };
            Client.MessageReceived += OnMessageReceived;
        }

        private static IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", true);
            string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.ToLower();
            builder.AddJsonFile($"appsettings.{env}.json", true);

            return builder.Build();
        }

        private async Task RunAsync()
        {
            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Source != MessageSource.User) return;
            if (message.Channel.Id != ChannelId) return;

            var authorId = message.Author.Id.ToString();
            if (!Config.People.ContainsKey(authorId)) return;

            string name = Config.People[authorId].Name;
            string avatarUrl = Config.People[authorId].AvatarUrl;
            using var webhookClient = new DiscordWebhookClient(WebhookUrl);
            await webhookClient.SendMessageAsync(message.Content, username: name, avatarUrl: avatarUrl);

            await message.DeleteAsync(DeleteMessageRequestOptions);
        }
    }
}
