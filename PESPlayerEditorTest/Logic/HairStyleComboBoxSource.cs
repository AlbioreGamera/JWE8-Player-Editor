using System.Collections.Generic;
using System.Linq;

namespace JWE8Editor.Logic
{
    public static class HairStyleComboBoxSource
    {
        public static List<KeyValuePair<int, string>> Items =>
            HairStyleCatalog.Names.OrderBy(kv => kv.Key).ToList();
    }
}