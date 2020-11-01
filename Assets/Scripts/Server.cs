using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    //Connect configuration
    private const int MAX_USER = 15;
    private const int PORT = 26000;
    //private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;


    private byte reliableChannel; //channel for data transfer
    private int hostId;           
    //private int webHostId;

    private bool isStater = false;
    private byte error;

    private Mongo dataBase;

    #region Monobehaviour
    [System.Obsolete]
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    [System.Obsolete]
    void Update()
    {
        if (!isStater)
            UpdateMessagePump();
    }
    #endregion


    [System.Obsolete]
    public void Init()
    {
        //DataBase init
        dataBase = new Mongo();
        dataBase.Init();

        //NetworkInit
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();
        reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
        HostTopology hostTopology = new HostTopology(connectionConfig, MAX_USER);
        hostId = NetworkTransport.AddHost(hostTopology, PORT, null);
        //webHostId = NetworkTransport.AddWebsocketHost(hostTopology, WEB_PORT, null);

        Debug.Log("Opening connection on port " + PORT);

        isStater = true;
    }

    //this method filtering any recived in channel and typing data for descript
    [System.Obsolete]
    public void UpdateMessagePump()
    {
        int receiveHostId;  //Web or standalone
        int ConnectionId;   //Which user is send. Use numeric from 0 to MAX_USER. Each new conenct heave lowest free number.
        int channelId;      //Witch lane is he sending taht message from
        byte[] receiveBuffer = new byte[BYTE_SIZE]; //data in byte type
        int dataSize;

        //receive data in byte type
        NetworkEventType type = NetworkTransport.Receive(out receiveHostId, out ConnectionId, out channelId, receiveBuffer, BYTE_SIZE, out dataSize, out error);

        //check type of event
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected!", ConnectionId));
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected!", ConnectionId));
                break;

            //event - data transfer
            case NetworkEventType.DataEvent:
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(receiveBuffer);
                NetMsg netMsg = (NetMsg)binaryFormatter.Deserialize(memoryStream);
                //OnData() - method for convert msg to data
                OnData(ConnectionId, channelId, receiveHostId, netMsg);
                break;

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;
        }
    }

    #region OnData
    [System.Obsolete]
    public void OnData(int connectionId, int channelId, int receiveHostId, NetMsg msg)
    {
        //check type of msg (all id type in class NetMsg)
        //after we know type - use function for work witch this data
        switch (msg.OperationCode)
        {
            case NetOperationCode.None:
                Debug.Log("Unexpected NETOperationCode");
                break;
            case NetOperationCode.CreateAccount:
                CreateAccount(connectionId, channelId, receiveHostId, (Net_CreateAccount) msg);
                break;
            case NetOperationCode.LoginRequest:
                LoginRequest(connectionId, channelId, receiveHostId, (Net_LoginRequest)msg);
                break;
        }
    }

    [System.Obsolete]
    private void CreateAccount(int connectionId, int channelId, int receiveHostId, Net_CreateAccount createAccount)
    {
        Net_OnCreateAcoount onCreateAccount = new Net_OnCreateAcoount();
        //insertAccount return bool for check succes
        if(dataBase.InsertAccount(createAccount.Username, createAccount.Password, createAccount.Email))
        {
            onCreateAccount.Succes = 1;
            onCreateAccount.Information = "Account was created!";
        }
        else
        {
            onCreateAccount.Succes = 0;
            onCreateAccount.Information = "Error on creating account";
        }
        SendClient(receiveHostId, connectionId, onCreateAccount);
    }

    [System.Obsolete]
    private void LoginRequest(int connectionId, int channelId, int receiveHostId, Net_LoginRequest loginRequest)
    {
        string randomToken = Utility.GenerateRandom(256);
        Model_Account modelAccount = dataBase.LoginAccount(loginRequest.UsernameOrEmail, loginRequest.Password, connectionId, randomToken);
        Net_OnLoginRequest onLoginRequest = new Net_OnLoginRequest();

        if(modelAccount != null)
        {
            onLoginRequest.Success = 1;
            onLoginRequest.Information = "You've been logged in as " + modelAccount.Username;
            onLoginRequest.Username = modelAccount.Username;
            onLoginRequest.Token = randomToken;
            onLoginRequest.ConnectionId = connectionId;
        }
        else
        {
            onLoginRequest.Success = 0;
            onLoginRequest.Information = "Fail login";
        }
        SendClient(receiveHostId, connectionId, onLoginRequest);
    }
    #endregion

    #region Send
    [System.Obsolete]
    public void SendClient(int receiveHost, int connectionId, NetMsg msg)
    {
        //Crush data into a byte[]
        byte[] buffer = new byte[BYTE_SIZE];
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);
        //send byte data
        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    #endregion

    [System.Obsolete]
    public void Shutdown()
    {
        isStater = false;
        NetworkTransport.Shutdown();
    }
}