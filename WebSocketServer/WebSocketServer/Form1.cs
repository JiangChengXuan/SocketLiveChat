using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Net.Mail;

namespace WebSocketServer
{
    public partial class Form1 : Form
    {
        private ConcurrentDictionary<string, List<string>> dic_rec = new ConcurrentDictionary<string, List<string>>();//接收的消息
        private ConcurrentDictionary<string, List<string>> dic_send = new ConcurrentDictionary<string, List<string>>();//主动发送的消息
        private WSClass ws = new WSClass();//自定websocket类
        private SynchronizationContext syncContext = null;//UI线程的同步上下文
        private ListViewItem sel_item = null;//选择的客户端
        private int nowDate = 0;//当前日期

        public Form1()
        {
            InitializeComponent();
        }

        #region 方法函数

        /// <summary>
        /// 获取列表指定的客户端
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <returns>返回项</returns>
        private ListViewItem isinList(string ip)
        {
            foreach (ListViewItem lvi in list_clients.Items)
            {
                if (lvi.SubItems[1].Text == ip)
                {
                    return lvi;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加或新增消息字典
        /// </summary>
        /// <param name="ip">客户端ip</param>
        /// <param name="msg">消息</param>
        /// <param name="dic">接收或发送的字典</param>
        private void UpdateOrAddMsg(string ip, string msg, ConcurrentDictionary<string, List<string>> dic)
        {
            if (dic.ContainsKey(ip))
                dic[ip].Add(msg);
            else
            {
                List<string> recs = new List<string>();
                recs.Add(msg);
                dic.TryAdd(ip, recs);
            }
        }

        /// <summary>
        /// 在UI上解析并显示接收的信息
        /// </summary>
        /// <param name="msg"></param>
        private void rec_msg(object msg)
        {
            ClientOP cp = (ClientOP)msg;
            int type = cp.Pac_Type;
            string str_msg = cp.Pac_Msg;
            string ip = cp.IP;
            string port = cp.Port;
            DateTime dt = cp.Time;

            Action Newlogin = () =>//新连接的客户端
            {
                ListViewItem lvi = new ListViewItem("");
                lvi.SubItems.Add(ip);
                lvi.SubItems.Add(dt.ToString("yyyy/MM/dd HH:mm:ss"));
                lvi.SubItems.Add("登录");
                lvi.UseItemStyleForSubItems = false;
                lvi.SubItems[0].BackColor = Color.LightBlue;//登录时颜色标注
                lvi.SubItems[3].ForeColor = Color.Blue;
                list_clients.Items.Add(lvi);
                ThreadPool.QueueUserWorkItem(new WaitCallback(Thread_GetAddress), ip);
            };

            if (!WSClass.dic_clients.ContainsKey(ip) && type != 8)
                Newlogin();
            else
            {
                ListViewItem lvi = isinList(ip);
                if (lvi != null)
                {
                    lvi.SubItems[2].Text = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    if (type != 8)
                    {
                        if (type == 1)//文本消息包
                            lvi.SubItems[0].BackColor = Color.LightGreen;
                        else if (type == 101)//连接成功包
                            lvi.SubItems[0].BackColor = Color.LightBlue;
                        lvi.SubItems[3].Text = "在线";
                        lvi.SubItems[3].ForeColor = Color.Green;
                    }
                    else
                    {
                        lvi.SubItems[3].Text = "离线";
                        lvi.SubItems[3].ForeColor = Color.Red;
                    }
                }
                else
                {
                    if (type != 8)
                        Newlogin();
                }
            }
            if (str_msg != string.Empty)
            {
                str_msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + str_msg + "\n";
                UpdateOrAddMsg(ip, str_msg, dic_rec);//添加信息到dic_rec字典中
            }
            if (sel_item != null && sel_item.SubItems[1].Text == ip)//当选中的客户端与接收到的客户端一致时，显示信息
            {
                if (str_msg != string.Empty)
                {
                    rbox_rec.AppendText(str_msg);
                    rbox_rec.SelectionStart = rbox_rec.Text.Length;
                    rbox_rec.ScrollToCaret(); //自动滚动
                }
            }
        }

        /// <summary>
        /// 根据IP获取物理地址
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns></returns>
        public string ipToAddr(string ip)
        {
            string addr = "";
            try
            {
                WebRequest request = WebRequest.Create("http://wap.ip138.com/ip_search138.asp?ip=" + ip);
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string read = reader.ReadToEnd();
                addr = Regex.Matches(read, "<b>.*?</b>")[1].Value.Replace("<b>", "").Replace("</b>", "");
            }
            catch { addr = "获取地理位置繁忙"; }
            return addr;
        }

        #endregion

        #region 控件事件

        //程序首次运行
        private void Form1_Load(object sender, EventArgs e)
        {
            syncContext = SynchronizationContext.Current;
            combox_ip.Items.AddRange(WSClass.GetLocIps());
            combox_ip.Text = "127.0.0.1";
        }

        //开启监听服务
        private void btn_listen_Click(object sender, EventArgs e)
        {
            if (ws.StartListen(combox_ip.Text, tbox_port.Text))//开始监听...
            {
                btn_listen.Enabled = false;
                btn_stop.Enabled = true;
                Thread t = new Thread(new ThreadStart(Queue_UIshow));//将接收的消息显示在ui上
                t.IsBackground = true;
                t.Start();
            }
            else
                MessageBox.Show("端口被占用或未能监听！");
        }

        //停止监听服务
        private void btn_stop_Click(object sender, EventArgs e)
        {
            ws.StopListen();
            btn_listen.Enabled = true;
            btn_stop.Enabled = false;
        }

        //服务端手动发送消息到客户端
        private void btn_send_Click(object sender, EventArgs e)
        {
            string sendmsg = rbox_sendto.Text.Trim();//发送的数据
            string showmsg = "";//显示在窗体上的数据
            if (sel_item == null) { MessageBox.Show("请先选择右边的客户端列表！"); return; }
            if (sendmsg == "") { MessageBox.Show("不能为空字符！"); return; }

            string ip = sel_item.SubItems[1].Text;
            if (WSClass.dic_clients.ContainsKey(ip))
            {
                bool islive = false;
                if (WSClass.dic_clients[ip].cSocket.Connected &&
                    ws.SendToClient("<b style='color:#ff9900;'>作者(在线)</b>：" + sendmsg, WSClass.dic_clients[ip].cSocket))
                {
                    showmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + sendmsg + "\n";
                    islive = true;
                }
                if (!islive)
                    MessageBox.Show("此客户端已经离线...");
                else
                {
                    UpdateOrAddMsg(ip, showmsg, dic_send);//保存历史发送消息到字典中
                    rbox_send.AppendText(showmsg);
                    rbox_send.SelectionStart = rbox_send.Text.Length;
                    rbox_send.ScrollToCaret(); //自动滚动到最低部
                }
            }
            else
                MessageBox.Show("此客户端已经离线...");
        }

        //点击列表项
        private void list_clients_Click(object sender, EventArgs e)
        {
            if (list_clients.SelectedItems.Count != 0)//点击列表时，显示该客户端的所有历史消息
            {
                ListViewItem lvi = list_clients.SelectedItems[0];
                string client_ip = lvi.SubItems[1].Text;
                lvi.BackColor = Color.White;
                if (!lvi.Equals(sel_item))
                {
                    rbox_rec.Clear();
                    rbox_send.Clear();
                    List<string> recs, sends = null;
                    dic_rec.TryGetValue(client_ip, out recs);
                    dic_send.TryGetValue(client_ip, out sends);
                    if (recs != null)
                    {
                        foreach (string msg in recs)
                        {
                            rbox_rec.AppendText(msg);
                        }
                    }
                    rbox_rec.SelectionStart = rbox_rec.Text.Length;
                    rbox_rec.ScrollToCaret(); //自动滚动
                    if (sends != null)
                    {
                        foreach (string msg in sends)
                        {
                            rbox_send.AppendText(msg);
                        }
                    }
                    rbox_send.SelectionStart = rbox_send.Text.Length;
                    rbox_send.ScrollToCaret(); //自动滚动
                }
                sel_item = lvi;
            }
        }

        //清除所有客户端的历史消息
        private void btn_clearall_Click(object sender, EventArgs e)
        {
            dic_rec.Clear();
            dic_send.Clear();
            rbox_rec.Clear();
            rbox_send.Clear();
        }

        #endregion

        #region 队列

        /// <summary>
        /// 队列_界面显示信息
        /// </summary>
        private void Queue_UIshow()
        {
            while (true)
            {
                ClientOP cp = null;
                WSClass.que_msgs.TryDequeue(out cp);
                if (cp != null)
                {
                    syncContext.Post(rec_msg, cp);//解析并显示接收信息
                }
                Thread.Sleep(100);
            }
        }

        #endregion

        #region 线程

        /// <summary>
        /// 线程_获取地理位置
        /// </summary>
        /// <param name="obj"></param>
        private void Thread_GetAddress(object obj)
        {
            string ip = obj.ToString();
            string addr = ipToAddr(obj.ToString());
            syncContext.Post(new SendOrPostCallback(delegate
            {
                ListViewItem lvi = isinList(ip);
                if (lvi != null)
                    lvi.SubItems[0].Text = addr;
                if (DateTime.Now.Day != nowDate)
                    statelb_count.Text = "1";
                else
                    statelb_count.Text = (int.Parse(statelb_count.Text) + 1).ToString();//统计当日的新访客
                nowDate = DateTime.Now.Day;
            }), null);

            //SendMsg sm = new SendMsg();//发送邮件通知
            //sm.InitMail("huangyaohuang@qq.com", "[" + addr.Trim() + "(" + ip + ")]的用户登录", "请及时登录服务器查看");
            //sm.SendMail();
        }

        #endregion
    }

    /// <summary>
    /// 发送邮件通知
    /// </summary>
    class SendMsg
    {
        string smtp = "smtp.aliyun.com";
        string frommail = "alarmmessage@aliyun.com";//自己的邮箱名
        string frompsw = "xxxxx";//自己的邮箱密码
        int port = 25;
        bool sslenable = false;
        bool checkpsw = false;

        MailMessage mMailMessage;
        SmtpClient mSmtpClient;

        public SendMsg()
        {

        }

        /// <summary>
        /// 定义邮件发送
        /// </summary>
        /// <param name="toMail">接收的邮箱,多个用";"分割</param>
        /// <param name="title">邮件标题</param>
        /// <param name="content">邮件内容</param>
        public void InitMail(string toMail, string title, string content)
        {
            mMailMessage = new MailMessage();
            string[] mails = toMail.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < mails.Length; i++)
            {
                mMailMessage.To.Add(mails[i]);
            }
            mMailMessage.From = new MailAddress(frommail);
            mMailMessage.Subject = title;
            mMailMessage.Body = content;
            mMailMessage.IsBodyHtml = true;
            mMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mMailMessage.Priority = MailPriority.Normal;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public void SendMail()
        {
            try
            {
                if (mMailMessage != null)
                {
                    mSmtpClient = new SmtpClient();
                    mSmtpClient.Host = smtp;
                    mSmtpClient.Port = port;
                    mSmtpClient.UseDefaultCredentials = false;
                    mSmtpClient.EnableSsl = sslenable;
                    if (checkpsw)
                    {
                        System.Net.NetworkCredential nc = new System.Net.NetworkCredential(frommail, frompsw);
                        mSmtpClient.Credentials = nc.GetCredential(mSmtpClient.Host, mSmtpClient.Port, "NTLM");
                    }
                    else
                    {
                        mSmtpClient.Credentials = new System.Net.NetworkCredential(frommail, frompsw);
                    }
                    mSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    mSmtpClient.Send(mMailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
