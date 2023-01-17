using System.Threading.Tasks;
using Discord;

namespace DiscordLeagueBot;

public class DiscordBot
{
    private InsultGenerator _insultGenerator = new();

    public DiscordBot()
    {
        _insultGenerator.UpdateWordListFiles();
    }
    
    public async Task<string> Roast(IUser user)
    {
        return await _insultGenerator.GenerateRandomInsult(user.Mention);
    }
}