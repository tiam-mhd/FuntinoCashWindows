using System;

namespace FuntinoControlCenter.Services
{
    internal static class EdgeAmountConverter
    {
        public static long ToRial(long amountToman)
        {
            return checked(amountToman * 10L);
        }
    }
}
