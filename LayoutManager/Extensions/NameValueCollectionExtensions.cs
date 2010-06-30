#region Using Directives

using System.Collections.Specialized;

#endregion

namespace Composite.Layout.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static bool ContainsKey(this NameValueCollection collection, string key)
        {
            var enumerator = collection.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var keyName = enumerator.Current as string;

                if (keyName != null && keyName.Equals(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}