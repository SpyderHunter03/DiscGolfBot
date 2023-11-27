using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;

namespace DiscgolfBot.Helpers
{
    public static class DiscordHelpers
    {
        public static async Task<DiscordUser> GetUser(this ulong userId, InteractionContext ctx) =>
            await ctx.Client.GetUserAsync(userId);

        public static string GetStringFromBlob(this byte[] blob)
        {
            var hexString = string.Join("", blob.Select(b => (char)b));
            // Remove the '0x' prefix if present
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString[2..];
            }

            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] GetBlobFromString(this string str)
        {
            var hexString = $"0x{Convert.ToHexString(Encoding.UTF8.GetBytes(str))}";
            return Encoding.UTF8.GetBytes(hexString);
        }
    }
}
