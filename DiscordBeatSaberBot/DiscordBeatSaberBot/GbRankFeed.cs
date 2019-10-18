﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    public static class GbRankFeed
    {
        private static DiscordSocketClient _discord;
        private static Logger _logger;

        public static async Task<(List<string>,List<string>,List<string>, List<string>)> GetGbRankList()
        {
            var tab = 5;
            var playerImg = new List<string>();
            var playerRank = new List<string>();
            var playerName = new List<string>();
            var playerId = new List<string>();

            var client = new HttpClient();

            for (var x = 1; x <= tab; x++)
            {
                var url = "https://scoresaber.com/global/" + x + "&country=gb";
              
                    var html = "";
                    try
                    {

                        html = await client.GetStringAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(Logger.LogCode.debug, "Scoresaber ignored me >;d\n\n" + ex);
                        throw ex;
                    }
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectNodes("//figure[@class='image is-24x24']");
                    playerImg.AddRange(table.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList());

                    var ranks = doc.DocumentNode.SelectNodes("//td[@class='rank']");
                    playerRank.AddRange(ranks.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace("#", "").Replace(@"\r\n", "").Trim()).ToList());

                    var names = doc.DocumentNode.SelectNodes("//td[@class='player']");
                    playerName.AddRange(names.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace(@"\r\n", "").Trim()).ToList());
                    playerId.AddRange(names.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList());
                
            }

            return (playerImg, playerRank, playerName, playerId);
        }

        public static async Task<Dictionary<int, List<string>>> GetOldRankList()
        {
            var filePath = "../../../GbRankList.txt";

            var data = new Dictionary<int, List<string>>();
            try
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    data = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(json);
                }
            }
            catch
            {
                data.Add(1, new List<string>{"naam", "img"});
            }
            
            return data;
        }

        public static async Task<Dictionary<int, List<string>>> UpdateGbRankList()
        {
            var filePath = "../../../GbRankList.txt";
            var rankList = await GetGbRankList();
            var newData = new Dictionary<int, List<string>>();

            for (var x = 0; x < rankList.Item1.Count; x++)
            {
                 newData.Add(int.Parse(rankList.Item2[x]), new List<string>{rankList.Item3[x], rankList.Item4[x], rankList.Item1[x]});
            }

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newData);
            }

            return newData;
        }

        public static async Task<List<EmbedBuilder>> MessagesToSend()
        {
            var embedBuilders = new List<EmbedBuilder>();

            var oldRankList = await GetOldRankList();
            await UpdateGbRankList();
            var newRankList = await GetGbRankList();
            

            var oldCache = new List<string>();

            var counter = 0;
            foreach (var player in oldRankList)
            {
                //player.Key.ToString() != newRankList.Item2[counter] 
                //OldList                NewList                       OldList            NewList
                if (player.Value[1] != newRankList.Item4[counter])
                {
                    if (!oldCache.Contains(newRankList.Item4[counter]))
                    {
                        var imgUrl = newRankList.Item1[counter].Replace("\"", "");
                        if (imgUrl == "/imports/images/oculus.png")
                        {
                            imgUrl = "https://scoresaber.com/imports/images/oculus.png";
                        }
                        else
                        {
                            imgUrl = "https://scoresaber.com" + imgUrl;
                        }

                        // No Message
                        try
                        {
                            embedBuilders.Add(new EmbedBuilder
                            {
                                Title = "Congrats, " + newRankList.Item3[counter],
                                Description = newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the GB beat saber players",
                                Url = "https://scoresaber.com" + newRankList.Item4[counter],
                                ThumbnailUrl = imgUrl,

                                Color = GetColorFromRank(int.Parse(newRankList.Item2[counter])),


                            });
                        }
                        catch
                        {
                            embedBuilders.Add(new EmbedBuilder
                            {
                                Title = "Congrats, " + newRankList.Item3[counter],
                                Description = newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the GB beat saber players",
                                Color = GetColorFromRank(int.Parse(newRankList.Item2[counter])),
                            });
                            var Logger = new Logger(_discord);
                            Logger.Log(Logger.LogCode.debug, "-GB feed- \nUrl is not correct. \nWrong url is from: " + newRankList.Item3[counter] + "\nAnd the url is: " + imgUrl);

                        }

                        Console.WriteLine("Feed GB - Message:" + newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the GB beat saber players");
                    }
                    else
                    {
                        // New Message 
                        
                    }                  
                }

                oldCache.Add(player.Value[1]);

                counter++;
            }

            return embedBuilders;
        }

        public static async Task GbRankingFeed(DiscordSocketClient discord)
        {
            _discord = discord;
            _logger = new Logger(discord);

            var guilds = discord.Guilds.Where(x => x.Id == 483482746824687616);
            //var channels = guilds.First().Channels.Where(x => x.Id == 504392851229114369);
            //channels.First().
            var embeds = new List<EmbedBuilder>();
            try
            {
                embeds = await MessagesToSend();
            }
            catch
            {
                _logger.Log(Logger.LogCode.warning, "Scoresaber is being annoying GB");
                return;
            }
            var guild = discord.GetGuild(483482746824687616);
            var channel = guild.GetTextChannel(535187354122584070);
            foreach (var embed in embeds)
            {
                try
                {               
                    await channel.SendMessageAsync("", false, embed.Build());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(embed.ToString() + " EX: " + ex);
                }
            }

            //GB Server
            //483482746824687616
            //535187354122584070

            // test bot server
            //439514151040057344 guild
            //504392851229114369 channel                                            


        }

        private static Color GetColorFromRank(int rank)
        {
            if (rank < 1)
            {
                 return Color.Purple;
            }else if (rank <= 10)
            {
                return Color.Red;
            }
            else if (rank <= 25)
            {
                return Color.Green;
            }
            else if (rank <= 50)
            {
                return Color.Blue;
            }
            else if (rank <= 100)
            {
                return Color.Gold;
            }
            else if (rank <= 250)
            {
                return Color.Magenta;
            }
            else
            {
                return Color.Default;
            }
        }
    }
}
