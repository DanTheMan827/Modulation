using DtxCS.DataTypes;
using System.Text;

namespace AmpHelper.Extensions
{
    internal static class DataArray_ToDta
    {
        /// <summary>
        /// Converts a <see cref="DataArray"/> to a string representation.
        /// </summary>
        /// <param name="array">The input <see cref="DataArray"/>.</param>
        /// <returns></returns>
        public static string ToDta(this DataArray array)
        {
            var sb = new StringBuilder();

            foreach (var child in array.Children)
            {
                sb.AppendLine(child.ToString(0));
            }

            return sb.ToString();
        }
    }
}
