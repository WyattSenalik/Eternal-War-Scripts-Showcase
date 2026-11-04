// Original Authors - Wyatt Senalik
using System.Globalization;

namespace Networking.Core
{
    public enum NetworkDeliveryMethod
    {
        Reliable,
        Unreliable
    }

    public readonly record struct NetworkConnectionId(ulong Value)
    {
        public static readonly NetworkConnectionId Invalid = new(0);

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
