
namespace Order_Processing_System.Services
{
    [Serializable]
    internal class ShippingProviderUnavailableException : Exception
    {
        public ShippingProviderUnavailableException()
        {
        }

        public ShippingProviderUnavailableException(string? message) : base(message)
        {
        }

        public ShippingProviderUnavailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}