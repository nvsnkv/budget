using System.Text;

namespace NVs.Budget.Controllers.Web.Utils;

internal static class StringStreamUtils
{
    public  static StreamReader AsStreamReader(this string s) => new(new MemoryStream(Encoding.UTF8.GetBytes(s)));
}
