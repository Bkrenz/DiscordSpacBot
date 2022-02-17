using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;

namespace DiscordSpacBot.Commands
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {

        private readonly CommandService _commandService;

        public AdminModule(CommandService commandService)
        {
            this._commandService = commandService;
        }

        [Command("help")]
        [Summary("List all commands for this bot.")]
        public async Task GetHelp()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

    }
}
