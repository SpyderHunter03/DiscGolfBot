using DiscgolfBot.Data;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.Attributes
{
    public class RequireAdminAttribute : SlashCheckBaseAttribute
    {
        public IUserRepository _userRepository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return ctx.User.Id == 337045211362623493;

            var isAdminUser = await _userRepository.IsAdminUser(ctx.User.Id);
            return isAdminUser;
        }
    }
}
