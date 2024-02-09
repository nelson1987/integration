using Adrian.Core.Commands;

namespace Adrian.Core.Extensions
{
    public static class ComnadExtensions
    {
        public static string ToJson(this ICommand command) 
        { 
            return System.Text.Json.JsonSerializer.Serialize(command);
        }
    }
}
