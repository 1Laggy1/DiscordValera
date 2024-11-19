using Discord;
using Discord.WebSocket;
using DiscordValera;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

public class Program
{
    private DiscordSocketClient _client;
    private List<IGuildUser> toNotify = new List<IGuildUser>();
    private IGuildUser valera;

    // Подія для перевірки статусу Valera
    public event Func<Task> OnCheckValeraStatus;
    BotConfig botConfig = new BotConfig();

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        DiscordSocketConfig config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged |
                             GatewayIntents.MessageContent |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildPresences,
            AlwaysDownloadUsers = true
        };
        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        _client.Ready += ReadyAsync;

        // Підписуємось на подію
        OnCheckValeraStatus += CheckValeraStatusAsync;
        LoadConfig("config.json");
        SteamChecker steam = new SteamChecker(botConfig.SteamUrl);

        steam.ValeraGay += CheckValeraStatusAsync;

        await _client.LoginAsync(TokenType.Bot, Info.Token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine("Bot is ready.");
        var guild = _client.Guilds.FirstOrDefault();

        if (guild == null)
        {
            Console.WriteLine("Bot is not part of any guilds.");
            return;
        }

        // Знаходимо Valera за ID
        if (ulong.TryParse(Info.Valera, out ulong valeraId))
        {
            valera = guild.GetUser(valeraId);
            if (valera == null)
            {
                Console.WriteLine("Valera not found in the guild.");
                return;
            }
        }
        else
        {
            Console.WriteLine("Invalid Valera ID.");
            return;
        }

        // Парсимо ToNotify та додаємо користувачів у список
        foreach (var idString in Info.ToNotify.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            if (ulong.TryParse(idString.Trim(), out ulong userId))
            {
                var user = guild.GetUser(userId);
                if (user != null)
                {
                    toNotify.Add(user);
                }
                else
                {
                    Console.WriteLine($"User with ID {userId} not found in the guild.");
                }
            }
            else
            {
                Console.WriteLine($"Invalid user ID: {idString}");
            }
        }

    }

    private async Task CheckValeraStatusAsync()
    {
        if (valera == null)
        {
            Console.WriteLine("Valera is not initialized or not found in the guild.");
            return;
        }

        if (valera.Status == UserStatus.Offline)
        {
            Console.WriteLine("Valera is offline. Sending notifications...");

            var random = new Random();
            string gif = Info.Gifs[random.Next(Info.Gifs.Count)]; // Обираємо випадковий GIF

            foreach (var user in toNotify)
            {
                try
                {
                    var dmChannel = await user.CreateDMChannelAsync();
                    await dmChannel.SendMessageAsync($"{gif}");
                    Console.WriteLine($"Notification sent to {user.Username}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send DM to {user.Username}: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine("Valera is online or has another status.");
        }
    }


    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
    private void LoadConfig(string path)
    {
        path = AppContext.BaseDirectory + @"" + path;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Could not find {path}.");
        }

        var json = File.ReadAllText(path);
        botConfig = JsonConvert.DeserializeObject<BotConfig>(json);
        Info.Token = botConfig.Token;
        Info.Valera = botConfig.Valera;
        Info.ToNotify = botConfig.ToNotify;
        Info.SteamUrl = botConfig.SteamUrl;
        Info.Gifs = botConfig.Gifs;
    }
}




