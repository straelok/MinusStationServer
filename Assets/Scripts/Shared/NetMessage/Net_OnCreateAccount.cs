[System.Serializable]
public class Net_OnCreateAcoount : NetMsg
{
    public Net_OnCreateAcoount()
    {
        OperationCode = NetOperationCode.OnCreateAccount;
    }

    public byte Succes { set; get; }
    public string Information { set; get; }
}
