public static class NetOperationCode
{
    public const int None = 0;

    //components

    //objects

    //server only

    public const int CreateAccount = 1;
    public const int LoginRequest = 2;

    //Client only

    public const int OnCreateAccount = 3;
    public const int OnLoginRequest = 4;
}
//5 is free


[System.Serializable]
public abstract class NetMsg
{
    public byte OperationCode { get; set; }

    public NetMsg()
    {
        OperationCode = NetOperationCode.None;
    }
}