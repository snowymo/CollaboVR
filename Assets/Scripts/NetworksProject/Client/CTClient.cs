//#define SINGLE_THREAD_RECEIVE

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;
using System.Linq;
using System;
using NetUtils;



public class CTClient : MonoBehaviour {
    public Transform testObject;
    void Awake()
    {
        if (this.testObject != null) {
            return;
        }

        this.testObject = new GameObject().transform;
    }

    public LineRenderer lineRenderer;
    private LineRenderer[] ourLines;

    public ushort id;
    public string SERVER_HEADER = "FRL";
    public int SERVER_PORT = 21235;
    public string SERVER_IP = "127.0.0.1";

    public string MULTICAST_GROUP = "239.0.2.4";
    IPAddress SERVER_IP_ADDRESS;
    IPAddress SERVER_MULTICAST_GROUP;
    public int CLIENT_PORT = 9591;
    Thread sendThread;
    Thread recvThread;

    byte[] sendBuff;
    byte[] rcvBuff;
    public bool isActive;

    int MAX_BUF_SIZE = 65536; // 2^16

    // https://yal.cc/cs-dotnet-asynchronous-udp-example/



    char msg;

    public const bool USE_RECEIVE_FROM = true;

    public class PacketFromServer {
        public string msg;
    }

    public class ClientPacketHeader {
        public ushort id;
        public float time;
    }
    public class PacketFromClient {
        public string msg;
    }


    NetBuffer nb;

    static byte CLIENT_TYPE = 0;

    public int Serialize(byte[] buff, int multiples = 1)
    {
        // reset the buffer data information for writing new data
        NetBuffer.Init(nb, buff, (multiples * 16) + this.SERVER_HEADER.Length);

        // begin writing at the end of the header
        NetBuffer.Advance(nb, this.SERVER_HEADER.Length);

        // client type
        NetBuffer.WriteByte(nb, CLIENT_TYPE);

        // id
        NetBuffer.WriteUint16(nb, this.id);

        // TEMP test (write one char)
        GenerateMessage(nb, buff);

        NetBuffer.WriteVector3(nb, this.position);

        // size of the data
        return nb.size;
    }

    public class NetState {
        public UdpClient clientSocket;
        public IPEndPoint clientEndpoint;

        public IPEndPoint serverEndpoint;

        public ConcurrentQueue<PacketFromServer> receiveQ;

        public ConcurrentQueue<BatchData> receiveQTest;
    }
    public NetState state;

    void Start()
    {
        Application.runInBackground = true;
        //this.ourLines = new LineRenderer[65536 / 4];
        this.ourLines = new LineRenderer[65536 / 4];

        Transform lineHolder = new GameObject("Line Holder").transform;
        //for (int i = 0; i < this.ourLines.Length; i += 1) {
        //    this.ourLines[i] = GameObject.Instantiate(this.lineRenderer);
        //    this.ourLines[i].name = i.ToString();
        //    this.ourLines[i].startWidth = 0.005f;
        //    this.ourLines[i].endWidth = 0.005f;
        //    this.ourLines[i].numCapVertices = 5;
        //    this.ourLines[i].gameObject.SetActive(false);
        //    this.ourLines[i].enabled = false;
        //    this.ourLines[i].transform.SetParent(lineHolder);
        //    ;
        //}


        if (this.SERVER_IP.Equals("0")) {
            this.SERVER_IP = "127.0.0.1";
        }
        this.SERVER_IP_ADDRESS = IPAddress.Parse(this.SERVER_IP);



        this.sendBuff = new byte[MAX_BUF_SIZE];
        this.rcvBuff = new byte[MAX_BUF_SIZE];
        this.msg = 'a';

        // permanently store the header here
        Encoding.Default.GetBytes(
           this.SERVER_HEADER,
            0, this.SERVER_HEADER.Length,
            this.sendBuff, 0
        );

        for (int i = 0; i < this.SERVER_HEADER.Length; i += 1) {
            Debug.Log((char)this.sendBuff[i]);
        }

        this.state = new NetState();

        // bind client
        this.state.clientSocket = new UdpClient();
        this.state.clientEndpoint = new IPEndPoint(
            IPAddress.Any, CLIENT_PORT
        );


        if (!this.MULTICAST_GROUP.Equals("0")) {
            this.state.clientSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.state.clientSocket.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);

            this.SERVER_MULTICAST_GROUP = IPAddress.Parse(this.MULTICAST_GROUP);
            this.state.clientSocket.JoinMulticastGroup(this.SERVER_MULTICAST_GROUP);
            this.state.clientSocket.Client.MulticastLoopback = false;
        }

        this.state.clientSocket.Client.Bind(this.state.clientEndpoint);



        // to server
        this.state.serverEndpoint = new IPEndPoint(
            this.SERVER_IP_ADDRESS, this.SERVER_PORT
        );


        Debug.Log("Server port: " + this.state.clientEndpoint.Port);

        // message queue
        this.state.receiveQ = new ConcurrentQueue<PacketFromServer>();
        this.state.receiveQTest = new ConcurrentQueue<BatchData>();
        this.recvQ = this.state.receiveQTest;

        this.sendThread = new Thread(DataSend);
        // this.sendThread.IsBackground = true;
#if !SINGLE_THREAD_RECEIVE
        this.recvThread = new Thread(DataRecv);
#endif
        // this.recvThread.IsBackground = true;

        this.nb = new NetBuffer();

        this.isActive = true;

        this.sendThread.Start(state);
#if !SINGLE_THREAD_RECEIVE
        this.recvThread.Start(state);
#endif
    }

    StringBuilder receiveStr = new StringBuilder();



    private const char SHUTDOWN_FLAG = '~';

    public class BatchData {
        public float[][] data;
        public uint time;
        public uint slicesCount;
        public byte[] TEST;
        public List<Vector3[]> curves;

        public HashSet<uint> arrived = new HashSet<uint>();
        internal int TEST_LEN;
    }
    public IDictionary<uint, BatchData> timeToDataSliceMap = new Dictionary<uint, BatchData>();

    uint latestTime = 0;
    public bool receivedSomething = false;

    public static int ParsetoInt16(byte[] value, int index)
    {
        return BitConverter.ToInt16(value, index) < 0 ? BitConverter.ToInt16(value, index) + 0x10000 : BitConverter.ToInt16(value, index);
    }
    public static string ParsetoString(byte[] value, int index, int len)
    {
        string ret = "";
        for (int i = 0; i < len / 2; i++) {
            int curbyte = ParsetoInt16(value, index + i * 2);
            int firsthalf = curbyte >> 8;
            int secondhalf = curbyte - ((curbyte >> 8) << 8);
            ret += (char)('A' + (firsthalf - 65));
            ret += (char)('A' + (secondhalf - 65));

            Debug.Log("Doing something");
        }
        return ret;
    }





    float time = 0.0f;
    Vector3 position = Vector3.zero;
    public int bytesReceived = 0;

    int Receive()
    {
        NetState ns = this.state;
        EndPoint endPoint = (EndPoint)ns.serverEndpoint;

        Debug.Log("Trying to receive");
        int recvCount = ns.clientSocket.Client.ReceiveFrom(
            this.rcvBuff, 0, this.rcvBuff.Length, SocketFlags.None,
            ref endPoint
        );
        Debug.Log("Received something");
        string msg = Encoding.ASCII.GetString(this.rcvBuff, 0, recvCount);
        Debug.Log(msg);

        bytesReceived = recvCount;
        return recvCount;
    }

    void SetLines(List<Vector3[]> curves)
    {
        int curvesCount = curves.Count;
        {
            int c = 0;
            for (; c < curvesCount; c += 1) {
                ourLines[c].positionCount = curves[c].Length;
                ourLines[c].SetPositions(curves[c]);
                ourLines[c].gameObject.SetActive(true);
                ourLines[c].enabled = true;
            }
            for (; c < ourLines.Length && ourLines[c].enabled == true; c += 1) {
                ourLines[c].gameObject.SetActive(false);
                ourLines[c].enabled = false;
            }
        }
    }


    void SendHandler(IAsyncResult args)
    {
        NetState ns = (NetState)args.AsyncState;
        ns.clientSocket.Client.EndSendTo(args);

        Debug.Log("Sent");
    }

    StringBuilder sbDebug = new StringBuilder();
    void DataSend(object args)
    {
        NetState ns = this.state;
        while (this.isActive) {
            //Debug.Log("Sending " + (char)this.buff[0]);           

            byte[] message = Encoding.ASCII.GetBytes("[" + this.SERVER_HEADER + "Hello from client]");
            NetBuffer.Init(nb, sendBuff, message.Length);
            Buffer.BlockCopy(message, 0, nb.data, 0, message.Length);
            nb.index += sizeof(byte) * message.Length;
            Debug.Log("Sending");

            var result = ns.clientSocket.Client.BeginSendTo(
                this.sendBuff, 0, message.Length,
                SocketFlags.None,
                ns.serverEndpoint,
                new AsyncCallback(SendHandler), ns
            );


            Thread.Sleep(10);
        }
    }

    void GenerateMessage(NetBuffer nb, byte[] buff)
    {
        buff[nb.index] = (byte)this.msg;

        this.msg = (char)(((byte)this.msg) + 1);
        if (this.msg > 'z') {
            this.msg = 'a';
        }

        // TEMP test (advance by the one char)
        NetBuffer.Advance(nb, 1);
    }

#if !SINGLE_THREAD_RECEIVE
    ConcurrentQueue<BatchData> recvQ;
    void Update()
    {
        this.time = Time.time;
        this.testObject.position = new Vector3(
            Mathf.Sin(time * 10),
            Mathf.Sin(time * 10),
            Mathf.Sin(time * 10)
        );
        this.position = this.testObject.position;

        BatchData batch;
        if (this.recvQ.TryDequeue(out batch)) {
            SetLines(batch.curves);
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            this.isActive = false;
            this.sendThread.Join();

            this.sendBuff[0] = (byte)SHUTDOWN_FLAG;
        }
    }

    void DataRecv(object args)
    {
        NetState ns = this.state;
        EndPoint endPoint = (EndPoint)ns.serverEndpoint;

        while (this.isActive) {
            Debug.Log("In DataRecv");
            int recvCount = Receive();

            List<Vector3[]> curves = new List<Vector3[]>();
            if (curves.Count == 0) {
                continue;
            }
            if (this.rcvBuff[0] == SHUTDOWN_FLAG) {
                return;
            }

            BatchData batch = new BatchData();
            batch.curves = curves;
            this.recvQ.Enqueue(batch);
        }
    }
    private void OnApplicationQuit()
    {
        this.isActive = false;
        this.sendThread.Join();

        this.sendBuff[0] = (byte)SHUTDOWN_FLAG;
        try {
            while (this.recvThread.IsAlive) {
                this.state.clientSocket.Client.SendTo(
                    this.sendBuff, 0, 1,
                    SocketFlags.None,
                    (EndPoint)this.state.clientEndpoint
                );

                Debug.Log("Waiting for receive thread to exit");
            }
        }
        catch {

        }

        this.recvThread.Join();

        try {
            this.state.clientSocket.Close();
        }
        catch (SocketException ex) {
            Debug.Log(ex.Message);
        }
        Debug.Log("client closed");
    }
#else
    void Draw()
    {
        int recvCount = Receive();
        receivedSomething = true;

        List<Vector3[]> curves = CTParser.Parse(this.rcvBuff, recvCount);
        if (curves.Count == 0)
        {
            return;
        }

        BatchData batch = new BatchData();
        batch.curves = curves;

        SetLines(batch.curves);
    }


    void Update()
    {
        this.time = Time.time;
        this.testObject.position = new Vector3(
            Mathf.Sin(time * 10),
            Mathf.Sin(time * 10),
            Mathf.Sin(time * 10)
        );
        this.position = this.testObject.position;

        Draw();
    }

    private void OnApplicationQuit()
    {
        this.isActive = false;
        this.sendThread.Join();

        try
        {
            this.state.clientSocket.Close();
        }
        catch (SocketException ex)
        {
            Debug.Log(ex.Message);
        }
        Debug.Log("client closed");
    }
#endif
}