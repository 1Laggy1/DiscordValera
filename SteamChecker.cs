using Discord;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DiscordValera
{
    internal class SteamChecker
    {
        public event Func<Task> ValeraGay;
        Timer timer = new Timer();
        private static bool _isFirstCall = true;
        private string _steamUrl = "";
        private float _time = 0;
        private IGuildUser _valera;
        public SteamChecker(string steamUrl, IGuildUser valera)
        {
            _steamUrl = steamUrl;
            _valera = valera;
            OnStart();
            
        }

        private async Task OnStart()
        {
            timer.Elapsed += CathThatASS;
            timer.Interval = 60000;
            timer.Enabled = true;

        }

        private async void CathThatASS(object? sender, ElapsedEventArgs e)
        {
            
            try
            {
                string url = _steamUrl;
                var web = new HtmlWeb();
                var document = web.Load(url);
                var nodes = document.DocumentNode.SelectNodes("//*[contains(@class, 'recentgame_quicklinks') and contains(@class, 'recentgame_recentplaytime')]");
                var DATE = $"{DateTime.Now.Day}.{DateTime.Now.Month}  {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
                foreach (var node in nodes)
                {
                    var s = node.InnerText.Replace(" ", "");
                    s = s.Replace("\r", "");
                    s = s.Replace("\n", "");
                    s = s.Replace("\t", "");
                    s = s.Replace("hourspast2weeks", "");
                    float dd = float.Parse(s, CultureInfo.InvariantCulture);
                    if (_isFirstCall == true)
                    {
                        _time = dd;
                        _isFirstCall = false;
                    }
                    if (_time < dd)
                    {
                        _time = dd;
                        if(_valera.Status == UserStatus.Offline)
                        {
                            ValeraGay.Invoke();
                            timer.Enabled = false;
                            await Task.Delay(3600000 * 3);
                            timer.Enabled = true;
                            _isFirstCall = true;
                        } 
                    }
                    if (_time > dd)
                    {
                        _time = dd;
                        Console.WriteLine("Time Decreaces");
                    }
                    Console.WriteLine(dd + "\t\t" + DATE);
                }
            }
            catch
            {
                Console.WriteLine($"Лох");
            }
        }
    }
}
