using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;

using Discord.Commands;

namespace DiscordSpacBot.Commands
{
    
    public class MemesModule : ModuleBase<SocketCommandContext>
    {
        
        [Command("moon")]
        [Summary("To the moon!")]
        public async Task ToTheMoon()
        {
            await Context.Channel.SendMessageAsync(@"https://tenor.com/view/rocket-lift-off-gif-10300477");
        }

    }
}
