using System;

namespace Chinese_sale_api.Exceptions
{
    public class GiftAlreadyAsignedException : Exception
    {
        public string? WinnerName { get; }

        public GiftAlreadyAsignedException()
        {
        }

        public GiftAlreadyAsignedException(string? winnerName)
            : base(winnerName is null ? "Gift already assigned to a winner." : $"Gift already assigned to winner '{winnerName}'.")
        {
            WinnerName = winnerName;
        }

        public GiftAlreadyAsignedException(string? winnerName, Exception inner)
            : base(winnerName is null ? "Gift already assigned to a winner." : $"Gift already assigned to winner '{winnerName}'.", inner)
        {
            WinnerName = winnerName;
        }
    }
}
