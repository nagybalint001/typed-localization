using System.ComponentModel;

namespace TypedLocalization.Abstractions
{
    public interface ILocalizer
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string this[string index] { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        string GetResourceString(string key, params (string Token, string Value)[] tokens);
    }
}
