namespace SimListener
{
    public enum ErrorCodes
    {
        OK,
        INVALID_DATA_REQUEST,
    }
    internal enum Definition
    {
        Dummy = 0
    };
    internal enum Request
    {
        Dummy = 0,
        Struct1
    };
    internal enum Event
    {
        RECUR_1SEC,
    }
    internal enum Requests
    {
        SIMULATION,
        AIRCRAFT_LOADED,
    }
}