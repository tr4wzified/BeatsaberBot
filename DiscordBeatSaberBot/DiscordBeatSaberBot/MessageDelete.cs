﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    static class MessageDelete
    {
        public async static Task DeleteMessageCheck(SocketMessage message)
        {
            //509230042753138689
            // role @everyone (505485680344956928)
            if (message.Channel.Id == 549350982081970176)
            {
           
                //await Task.Delay(4000);
                await message.DeleteAsync();               
            }
        }
    }
}
