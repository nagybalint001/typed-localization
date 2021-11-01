using System.ComponentModel;

using Microsoft.Extensions.Localization;

namespace TypedLocalization.Abstractions
{
    public abstract class LocalizerBase : ILocalizer
    {
        protected readonly IStringLocalizer _localizer;

        protected LocalizerBase(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string this[string key] => GetResourceString(key);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string GetResourceString(string key, params (string Token, string Value)[] tokens)
        {
            try
            {
                var value = (string)_localizer.GetString(key);

                if (value == null)
                    return key;

                foreach (var token in tokens)
                    value = value.Replace(token.Token, token.Value);

                return value;
            }
            catch
            {
                return key;
            }
        }
    }
}
