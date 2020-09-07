using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;

/// <summary>
/// 访问的客户端类
/// </summary>
class ClientOP
{
    DateTime dt;//最后访问的时间
    Socket socket = null;//最后访问的socket

    string ip = "";//客户端IP地址
    string port = "";//客户端端口号
    string msg = "";//客户端发送的信息
    int pactype = -1;//数据类型
    bool fin = false;//数据包是否结束
    bool isflash = false;//是否是采用flash的websocket

    /// <summary>
    /// 声明ClietnOP类
    /// </summary>
    public ClientOP()
    {
        dt = DateTime.Now;
    }

    #region 属性访问

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string IP
    {
        get { return ip; }
        set { ip = value; }
    }

    /// <summary>
    /// 客户端端口号
    /// </summary>
    public string Port
    {
        get { return port; }
        set { port = value; }
    }

    /// <summary>
    /// 获取接收消息最后的时间
    /// </summary>
    public DateTime Time
    {
        get { return dt; }
    }

    /// <summary>
    /// 获取用户的socket
    /// </summary>
    public Socket cSocket
    {
        get { return socket; }
        set { socket = value; }
    }

    /// <summary>
    /// 数据包类型
    /// </summary>
    public int Pac_Type
    {
        get { return pactype; }
        set { pactype = value; }
    }
    /// <summary>
    /// 是否最后一个数据包
    /// </summary>
    public bool Pac_Fin
    {
        get { return fin; }
        set { fin = value; }
    }
    /// <summary>
    /// 数据包消息内容
    /// </summary>
    public string Pac_Msg
    {
        get { return msg; }
        set { msg = value; }
    }

    /// <summary>
    /// 是否flashweb
    /// </summary>
    public bool Pac_Flash
    {
        get { return isflash; }
        set { isflash = value; }
    }

    #endregion
}

/// <summary>
/// websocket数据包解析
/// </summary>
class WSClass
{
    /// <summary>
    /// 客户端字典
    /// </summary>
    public static ConcurrentDictionary<string, ClientOP> dic_clients = new ConcurrentDictionary<string, ClientOP>();
    /// <summary>
    /// 客户端消息队列
    /// </summary>
    public static ConcurrentQueue<ClientOP> que_msgs = new ConcurrentQueue<ClientOP>();

    static bool isStop = false;//中心是否停止监听
    static Thread check_logout = new Thread(Thread_Check_Logout);//检查客户端是否已经离线
    static Socket ListenSocket;//接收客户端请求的socket

    /// <summary>
    /// 声明WSClass
    /// </summary>
    public WSClass()
    {
        if (!check_logout.IsAlive)
        {
            check_logout.IsBackground = true;
            check_logout.Start();
        }
    }

    #region 静态方法

    /// <summary>
    /// 获取本地IP列表
    /// </summary>
    /// <returns></returns>
    public static string[] GetLocIps()
    {
        List<string> arr = new List<string>();
        IPAddress[] arrIPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        foreach (IPAddress ip in arrIPAddresses)
        {
            if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                arr.Add(ip.ToString());
        }
        return arr.ToArray();
    }

    #endregion

    #region 异步socket监听

    private void StartAccept()//开始监听请求
    {
        if (!isStop)
        {
            SocketAsyncEventArgs AcceptEventArg = new SocketAsyncEventArgs();
            AcceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Complete);
            if (!ListenSocket.AcceptAsync(AcceptEventArg))//false为同步完成，手动触发
            {
                ProcessAccept(AcceptEventArg);
            }
        }
    }

    private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)//客户端连接请求
    {
        if (acceptEventArgs.SocketError != SocketError.Success || isStop)
        {
            if (!isStop)
                StartAccept();
            closeConnect(acceptEventArgs.AcceptSocket);
            acceptEventArgs = null;
            return;
        }
        StartAccept();//继续等待下一次连接请求
        SocketAsyncEventArgs ReceiveEventArgs = new SocketAsyncEventArgs();
        ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Complete);
        ReceiveEventArgs.SetBuffer(new byte[65536], 0, 65536);//分配数据缓存空间
        ReceiveEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
        acceptEventArgs = null;
        StartReceive(ReceiveEventArgs);
    }

    private void StartReceive(SocketAsyncEventArgs receiveEventArgs)//开始接受数据
    {
        if (!isStop)
        {
            if (receiveEventArgs.AcceptSocket.Connected)
            {
                if (!receiveEventArgs.AcceptSocket.ReceiveAsync(receiveEventArgs))//false为同步完成，手动触发
                {
                    ProcessReceive(receiveEventArgs);
                }
            }
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs receiveEventArg)//接收数据
    {
        if (receiveEventArg.SocketError != SocketError.Success || receiveEventArg.BytesTransferred == 0)
        {
            closeConnect(receiveEventArg.AcceptSocket);
            receiveEventArg = null;
            return;
        }
        int len = receiveEventArg.BytesTransferred;//接收到的数据包长度
        byte[] data_pac = new byte[len];
        Array.Copy(receiveEventArg.Buffer, receiveEventArg.Offset, data_pac, 0, len);//将接收到的数据包放入data_pac中
        Func<bool, byte[]> appendData = (ok) =>
        {
            byte[] newdata;
            if (receiveEventArg.UserToken != null)
            {
                byte[] tmp = (byte[])receiveEventArg.UserToken;
                newdata = new byte[len + tmp.Length];
                Array.Copy(tmp, 0, newdata, 0, tmp.Length);
                Array.Copy(data_pac, 0, newdata, tmp.Length, len);
                if (ok)//true时表示所有数据接收完毕
                    receiveEventArg.UserToken = null;
            }
            else
                newdata = data_pac;
            return newdata;
        };
        if (receiveEventArg.AcceptSocket.Available != 0)
            receiveEventArg.UserToken = appendData(false);
        else
        {
            data_pac = appendData(true);
            string msg = "";
            ClientOP cp = new ClientOP();
            cp.cSocket = receiveEventArg.AcceptSocket;
            if (Analyze(data_pac, len, cp))
            {
                if (!cp.Pac_Flash)//当为flash请求策略文件时不加入消息队列
                    que_msgs.Enqueue(cp);//将接收的消息加入队列
                msg = cp.Pac_Msg;
                if (msg != "")
                    Send(cp.cSocket, "<b>服务端成功收到你的消息：</b>" + replaceSpecStr(msg));
                //SendToClient("<span style='color:green;'>机器人</span>：" + getRobotAnswer(msg), receiveEventArg.AcceptSocket);
                if (cp.Pac_Type == 101)
                    Send(cp.cSocket, "<b>服务端主动向你推送：</b>" + " <span style='font-size:20px;'>点击进入" +
                        "<a style='font-weight:900;font-size:16px;' href='http://www.blue-zero.com/chat/' target='_blank'>" +
                        "【聊天室】[围观]</a>，使用websocket开发，支持一对一私聊和一对多公聊</span>");
            }
            else
            {
                cp = null;
                closeConnect(receiveEventArg.AcceptSocket);
            }
        }
        StartReceive(receiveEventArg);//继续等待下一次接收数据
    }

    private void Asyn_Complete(object sender, SocketAsyncEventArgs e)//当socket的请求、接收操作完成时
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Accept: ProcessAccept(e); break;
            case SocketAsyncOperation.Receive: ProcessReceive(e); break;
            default: throw new ArgumentException("无效动作！");
        }
    }

    #endregion

    #region 函数方法


    /// <summary>
    /// 向用户发送数据
    /// </summary>
    /// <param name="msg">消息内容</param>
    /// <param name="socket">用户的套接字</param>
    /// <returns></returns>
    public bool SendToClient(string msg, Socket socket)
    {
        try
        {
            Send(socket, msg);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// 开始监听
    /// </summary>
    /// <param name="ip">监听的ip地址</param>
    /// <param name="port">监听的端口</param>
    public bool StartListen(string ip, string port)
    {
        try
        {
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ListenSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), int.Parse(port)));
            ListenSocket.Listen(Int32.MaxValue);
            isStop = false;
            StartAccept();
            return true;
        }
        catch (Exception ex)
        {
            string strException = string.Format("{0}发生系统异常[StartListen]。\r\n{1}\r\n\r\n\r\n", DateTime.Now, ex.Message + "(" + ex.StackTrace + ")");
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemException.log"), strException);
            return false;
        }
    }

    /// <summary>
    /// 停止监听，断开所有连接
    /// </summary>
    public void StopListen()
    {
        isStop = true;
        foreach (KeyValuePair<string, ClientOP> kv in dic_clients)
        {
            if (kv.Value.cSocket.Connected)
                closeConnect(kv.Value.cSocket);
        }
        dic_clients.Clear();
        closeConnect(ListenSocket);
    }

    /// <summary>
    /// 线程，检查客户端离线
    /// </summary>
    private static void Thread_Check_Logout()
    {
        while (true)
        {
            ClientOP cp = new ClientOP();
            foreach (KeyValuePair<string, ClientOP> kv in dic_clients)
            {
                if (!kv.Value.cSocket.Connected)//该客户端已经断开
                {
                    cp = new ClientOP();
                    cp.Pac_Type = 8;
                    cp.IP = kv.Key;
                    dic_clients.TryRemove(kv.Key, out cp);
                    que_msgs.Enqueue(cp);//通知离线消息
                }
            }
            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 获取机器人自动回复
    /// </summary>
    /// <param name="qus">问题</param>
    /// <returns></returns>
    private string getRobotAnswer(string qus)
    {
        try
        {
            WebRequest request = WebRequest.Create("http://i.itpk.cn/api.php?question=" + qus + "&api_key=1040a5738cb36c467c4c7fea1974cc34&api_secret=xxxxx");
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string r = Regex.Unescape(reader.ReadToEnd()).Replace("\r\n", "</br>");
            string title = Regex.Match(r, "title.*?\"content").Value.Replace("\"", "").Replace(",content", "").Replace("title:", "");
            string content = Regex.Match(r, "content\":.*").Value.Replace("\"", "").Replace("content:", "").Replace("}", "");
            return qus.Contains("笑话") ? "<div style='font-weight:900'>" + title + "</div>" + content : r;
        }
        catch
        {
            return "机器人回复维护中...";
        }
    }

    /// <summary>
    /// websocket数据包解析入口
    /// </summary>
    /// <param name="buffer">字节数组</param>
    /// <param name="len">长度</param>
    /// <param name="s">客户端的套接字</param>
    /// <returns>解析是否成功</returns>
    public bool Analyze(byte[] buffer, int len, ClientOP clientop)
    {
        try
        {
            Socket cSocket = clientop.cSocket;
            IPEndPoint ep = (IPEndPoint)cSocket.RemoteEndPoint;
            string ip = ep.Address.ToString();
            string port = ep.Port.ToString();
            string packetStr = Encoding.UTF8.GetString(buffer, 0, len);
            clientop.IP = ip;
            clientop.Port = port;
            if (Regex.Match(packetStr.ToLower(), "upgrade: websocket").Value == "")//当收到的数据[不是]握手包时
            {
                if (dic_clients.ContainsKey(ip) && Regex.Match(packetStr.ToLower(), "policy-file-request").Value == "")//当收到的数据[不是]flash请求策略文件时
                {
                    clientop.Pac_Msg = AnalyzeClientData(clientop, buffer, len);//解析出客户端的消息
                }
                else
                {
                    if (Regex.Match(packetStr.ToLower(), "policy-file-request").Value != "")//当收到flash请求策略文件时
                    {
                        cSocket.Send(Encoding.UTF8.GetBytes("<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\" /></cross-domain-policy>\0"));
                        clientop.Pac_Flash = true;
                        return true;
                    }
                    else
                    {
                        cSocket.Send(Encoding.UTF8.GetBytes("Sorry,Not allow,Please call me number:13860436191"));
                        return false;
                    }
                }
            }
            else
            {
                cSocket.Send(AnswerHandShake(packetStr));//应答握手包
                clientop.Pac_Type = 101;//连接成功
            }
            //添加客户端入集合
            if (dic_clients.ContainsKey(ip) && clientop.Pac_Type == 101)
            {
                //同个IP重复登录（注：您也可以自己定义key的值，这里作者把IP作为key）
                SendToClient("<b>同个IP已重复登录：</b>" + "如有什么疑问，也可以联系作者，QQ:114687576", dic_clients[ip].cSocket);//发出提醒
                closeConnect(dic_clients[ip].cSocket);//关闭旧连接
                dic_clients.AddOrUpdate(ip, clientop, (key, oldv) => clientop);//更新最新该连接
            }
            else
                dic_clients.TryAdd(ip, clientop);
            return true;
        }
        catch (Exception ex)
        {
            string strException = string.Format("{0}发生系统异常[Analyze]。\r\n{1}\r\n\r\n\r\n", DateTime.Now, ex.Message + "(" + ex.StackTrace + ")");
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemException.log"), strException);
            return false;
        }
    }

    /// <summary>
    /// 应答客户端连接包
    /// </summary>
    /// <param name="packetStr">数据包字符串</param>
    /// <returns></returns>
    private byte[] AnswerHandShake(string packetStr)
    {
        string handShakeText = packetStr;
        string key = string.Empty;
        Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
        Match m = reg.Match(handShakeText);
        if (m.Value != "")
        {
            key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
        }

        byte[] secKeyBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
        string secKey = Convert.ToBase64String(secKeyBytes);

        var responseBuilder = new StringBuilder();
        responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
        responseBuilder.Append("Upgrade: websocket" + "\r\n");
        responseBuilder.Append("Connection: Upgrade" + "\r\n");
        responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");

        return Encoding.UTF8.GetBytes(responseBuilder.ToString());
    }

    /// <summary>
    /// 解析数据包
    /// </summary>
    /// <param name="buffer">数据包</param>
    /// <param name="len">长度</param>
    /// <returns></returns>
    private string AnalyzeClientData(ClientOP clientop, byte[] buffer, int len)
    {
        bool mask = false;
        int lodlen = 0;
        if (len < 2)
            return string.Empty;
        clientop.Pac_Fin = (buffer[0] >> 7) > 0;
        if (!clientop.Pac_Fin)
            return string.Empty;
        clientop.Pac_Type = buffer[0] & 0xF;
        if (clientop.Pac_Type == 10)//心跳包(IE10及以上特有，不处理即可)
            return string.Empty;
        else if (clientop.Pac_Type == 8)//退出包
        {
            removeConnectDic(clientop);
            return string.Empty;
        }
        mask = (buffer[1] >> 7) > 0;
        lodlen = buffer[1] & 0x7F;
        byte[] loddata;
        byte[] masks = new byte[4];

        if (lodlen == 126)
        {
            Array.Copy(buffer, 4, masks, 0, 4);
            lodlen = (UInt16)(buffer[2] << 8 | buffer[3]);
            loddata = new byte[lodlen];
            Array.Copy(buffer, 8, loddata, 0, lodlen);
        }
        else if (lodlen == 127)
        {
            Array.Copy(buffer, 10, masks, 0, 4);
            byte[] uInt64Bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                uInt64Bytes[i] = buffer[9 - i];
            }
            lodlen = (int)BitConverter.ToUInt64(uInt64Bytes, 0);

            loddata = new byte[lodlen];
            try
            {
                for (int i = 0; i < lodlen; i++)
                {
                    loddata[i] = buffer[i + 14];
                }
            }
            catch { }
        }
        else
        {
            Array.Copy(buffer, 2, masks, 0, 4);
            loddata = new byte[lodlen];
            Array.Copy(buffer, 6, loddata, 0, lodlen);
        }
        for (var i = 0; i < lodlen; i++)
        {
            loddata[i] = (byte)(loddata[i] ^ masks[i % 4]);
        }
        return Encoding.UTF8.GetString(loddata);
    }

    /// <summary>
    /// 向客户端发送数据
    /// </summary>
    /// <param name="socket">客户端socket</param>
    /// <param name="message">要发送的数据</param>
    private void Send(Socket socket, string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        bool send = true;
        int SendMax = 65536;//每次分片最大64kb数据
        int count = 0;//发送的次数
        int sendedlen = 0;//已经发送的字节长度
        while (send)
        {
            byte[] contentBytes = null;//待发送的消息内容
            var sendArr = bytes.Skip(count * SendMax).Take(SendMax).ToArray();
            sendedlen += sendArr.Length;
            if (sendArr.Length > 0)
            {
                send = bytes.Length > sendedlen;//是否继续发送
                if (sendArr.Length < 126)
                {
                    contentBytes = new byte[sendArr.Length + 2];
                    contentBytes[0] = (byte)(count == 0 ? 0x81 : (!send ? 0x80 : 0));
                    contentBytes[1] = (byte)sendArr.Length;//1个字节存储真实长度
                    Array.Copy(sendArr, 0, contentBytes, 2, sendArr.Length);
                    send = false;
                }
                else if (sendArr.Length <= 65535)
                {
                    contentBytes = new byte[sendArr.Length + 4];
                    if (!send && count == 0)
                        contentBytes[0] = 0x81;//非分片发送
                    else
                        contentBytes[0] = (byte)(count == 0 ? 0x01 : (!send ? 0x80 : 0));//处于连续的分片发送
                    contentBytes[1] = 126;
                    byte[] slen = BitConverter.GetBytes((short)sendArr.Length);//2个字节存储真实长度
                    contentBytes[2] = slen[1];
                    contentBytes[3] = slen[0];
                    Array.Copy(sendArr, 0, contentBytes, 4, sendArr.Length);
                }
                else if (sendArr.LongLength < long.MaxValue)
                {
                    contentBytes = new byte[sendArr.Length + 10];
                    contentBytes[0] = (byte)(count == 0 ? 0x01 : (!send ? 0x80 : 0));//处于连续的分片发送
                    contentBytes[1] = 127;
                    byte[] llen = BitConverter.GetBytes((long)sendArr.Length);//8个字节存储真实长度
                    for (int i = 7; i >= 0; i--)
                    {
                        contentBytes[9 - i] = llen[i];
                    }
                    Array.Copy(sendArr, 0, contentBytes, 10, sendArr.Length);
                }
            }
            socket.Send(contentBytes);
            count++;
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="so">连接的socket</param>
    private void closeConnect(Socket socket)
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch { }
        try
        {
            socket.Close();
        }
        catch { }
    }

    /// <summary>
    /// 删除字典里的连接
    /// </summary>
    /// <param name="cp">客户端连接</param>
    private void removeConnectDic(ClientOP cp)
    {
        string ip = cp.IP;
        string port = cp.Port;
        ClientOP _cp = new ClientOP();
        if (dic_clients.ContainsKey(ip))
        {
            if (dic_clients.TryRemove(ip, out _cp))
                closeConnect(cp.cSocket);
        }
    }

    /// <summary>
    /// 替换特殊字符
    /// </summary>
    /// <param name="str">替换的字符串</param>
    /// <returns></returns>
    private string replaceSpecStr(string str)
    {
        str = str.Replace(">", "&gt;");
        str = str.Replace("<", "&lt;");
        str = str.Replace(" ", "&nbsp;");
        str = str.Replace("\"", "&quot;");
        str = str.Replace("\'", "&#39;");
        str = str.Replace("\\", "\\\\");
        str = str.Replace("\n", "<br />");
        str = str.Replace("\r", "\\r");
        str = str.Replace("\t", "&emsp;");
        return str;
    }

    #endregion
}
