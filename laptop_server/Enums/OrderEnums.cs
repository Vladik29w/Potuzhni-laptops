namespace LaptopServer.Enums
{
    public enum PayEnum
    {
        Unknown = 0,
        Cash = 1,
        Online = 2
    }
    public enum DeliveryEnum
    {
        Unknown = 0,
        Pickup = 1,
        NovaPost = 2
    }
    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1,
        Failure = 2,
        Expired = 3,
        Reversed = 4
    }
}
