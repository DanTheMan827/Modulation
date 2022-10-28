using DtxCS.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace DanTheMan827.Modulation.Extensions
{
    internal static partial class Extensions
    {
        public static IEnumerable<DataNode> FindByName(this DataArray dtx, string name)
        {
            return dtx.Children.Where(child =>
            {
                if (child.GetType() != typeof(DataArray))
                {
                    return false;
                }

                var casted = (DataArray)child;

                if (casted.Children.Count() > 1 && casted.Children[0].GetType() == typeof(DataSymbol) && ((DataSymbol)casted.Children[0]).Name == name)
                {
                    return true;
                }

                return false;
            });
        }
    }
}
