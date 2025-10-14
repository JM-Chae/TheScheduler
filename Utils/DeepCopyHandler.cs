using System.Text.Json;

namespace TheScheduler.Utils
{
    public static class DeepCopyHandler
    {
        public static T Clone<T>(T source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}
