using System.Windows.Markup;
using TheScheduler.Services;

namespace TheScheduler.Utils
{
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return LocalizationService.Instance.GetString(Key);
        }
    }
}
