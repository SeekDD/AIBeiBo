using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace RemarkUI
{
    public partial class remark : CCSkinMain
    {
        string UserID = "";
        string PSW = "";
        string CamIP = "";
        public Queue<Mat> Q = new Queue<Mat>();
        Thread Grab = null;
        Thread ShowImg = null;
        Thread SockListening = null;
        bool ClrThread = false;     //是否需要清除释放线程
        bool GetReceive = false;
        bool UnCompeleted = false;
        public List<Button> BTN = new List<Button>();//获取的需要在界面上显示的标记位置
        public List<System.Drawing.Point> BTN999 = new List<System.Drawing.Point>();//获取的999值存储的标记位置
        public List<System.Drawing.Point> Remove1 = new List<System.Drawing.Point>();
        public List<System.Drawing.Point> Remove2 = new List<System.Drawing.Point>();
        public List<string> UDPBTN = new List<string>();//UDP传来的标记位置
        public List<string> UDPRemove1 = new List<string>();
        public List<string> UDPRemove2 = new List<string>();
        List<string> UDPdata = new List<string>();
        public Socket tcpClient;
        public IPAddress MyIPAdress;
        private Button B;
        private System.Drawing.Point location;
        string Threshold = "0";
        string Jitter = "0";
        string MinAlarm = "0";
        string hwRadio = "0";
        string Maxcontours = "0";
        string aeraRadio = "0";
        public string test = "?";
        public int index = 0;
        public int Pnum = 0, ign1 = 0, ign2 = 0;
        public remark()
        {
            InitializeComponent();
            skinLabel1.Size = new System.Drawing.Size(120, 28);
            skinLabel2.Size = new System.Drawing.Size(100, 28);
            skinLabel3.Size = new System.Drawing.Size(80, 28);
            IPBOX.Size = new System.Drawing.Size(120, 28);
            IDBOX.Size = new System.Drawing.Size(120, 28);
            PSWBOX.Size = new System.Drawing.Size(120, 28);
            LoginBtn.Location = new System.Drawing.Point(Title.Width - LoginBtn.Width - 100, LoginBtn.Location.Y);
            LoginBtn.Size = new System.Drawing.Size(80, 30);
            showCount.Text = "未启用标记...";
            threshold.Text = "0";
            jitter.Text = "0";
            min_alarm_h_radio.Text = "0";
            hw_radio.Text = "0";
            max_contours_result.Text = "0";
            aera_radio.Text = "0";
        }
        private void Send(string IP, String message)
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipa in ips)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    MyIPAdress = ipa;
                    break;
                }
            }
            
            tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint sendiPEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3128);
            AllLocation.Text = MyIPAdress.ToString();
            tcpClient.Connect(sendiPEndPoint);
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(message+ " #END#\0");

                if (SockListening != null)
                {
                    if (SockListening.IsAlive)
                        SockListening.Abort();
                    SockListening.DisableComObjectEagerCleanup();
                }
                SockListening = new Thread(Receive);
                SockListening.IsBackground = true;
                GetReceive = true;
                SockListening.Start();
                tcpClient.Send(bytes);
            }
            catch
            {
                MessageBox.Show("发送失败", "提示", MessageBoxButtons.OK);
                tcpClient.Disconnect(true);
            }

        }
        //---------------------------------建立UDP并发送请求

        private void Receive()
        {
            byte[] rcvBuf = new byte[1024];
            EndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (GetReceive)
            {
                try
                {
                    string str = "";

                    int lenth = tcpClient.ReceiveFrom(rcvBuf, ref iPEndPoint);
                    test = "Get";
                    if (rcvBuf != null)
                    {
                        str = Encoding.UTF8.GetString(rcvBuf, 0, lenth);
                    }
                    string[] trans = str.Split(' ');
                    for (int i = 0; i < trans.Length; i++)
                        UDPdata.Add(trans[i]);
                    UDPBTN.Clear();
                    string[] point12 = UDPdata[16].Split('-');
                    for (int i = 0; i < point12.Length; i++)
                    {
                        UDPBTN.Add(point12[i]);
                    }
                    string[] point34 = UDPdata[18].Split('-');
                    for (int i = 0; i < point34.Length; i++)
                    {
                        UDPBTN.Add(point34[i]);
                    }
                    string[] point56 = UDPdata[20].Split('-');
                    for (int i = 0; i < point56.Length; i++)
                    {
                        UDPBTN.Add(point56[i]);
                    }

                    UDPRemove1.Clear();
                    string[] one12 = UDPdata[26].Split('-');
                    for (int i = 0; i < one12.Length; i++)
                    {
                        UDPRemove1.Add(one12[i]);
                    }
                    string[] one34 = UDPdata[28].Split('-');
                    for (int i = 0; i < one34.Length; i++)
                    {
                        UDPRemove1.Add(one34[i]);
                    }
                    string[] one56 = UDPdata[30].Split('-');
                    for (int i = 0; i < one56.Length; i++)
                    {
                        UDPRemove1.Add(one56[i]);
                    }

                    UDPRemove2.Clear();
                    string[] two12 = UDPdata[36].Split('-');
                    for (int i = 0; i < two12.Length; i++)
                    {
                        UDPRemove2.Add(two12[i]);
                    }
                    string[] two34 = UDPdata[38].Split('-');
                    for (int i = 0; i < two34.Length; i++)
                    {
                        UDPRemove2.Add(two34[i]);
                    }
                    string[] two56 = UDPdata[40].Split('-');
                    for (int i = 0; i < two56.Length; i++)
                    {
                        UDPRemove2.Add(two56[i]);
                    }

                    if (UDPdata[14] != "0")
                    {
                        BTN999.Clear();
                        for (int i = 0; i < Convert.ToInt32(UDPdata[14]) * 2; i++)
                        {
                            int beginX = Convert.ToInt32(UDPBTN[i]);
                            i++;
                            int beginY = Convert.ToInt32(UDPBTN[i]);
                            BTN999.Add(new System.Drawing.Point(beginX, beginY));
                        }
                    }
                    if (UDPdata[22] != "0" && UDPdata[24] != "0")
                    {
                        Remove1.Clear();
                        for (int i = 0; i < Convert.ToInt32(UDPdata[24]) * 2; i++)
                        {
                            int beginX = Convert.ToInt32(UDPRemove1[i]);
                            i++;
                            int beginY = Convert.ToInt32(UDPRemove1[i]);
                            Remove1.Add(new System.Drawing.Point(beginX, beginY));
                        }
                    }
                    if (UDPdata[32] != "0" && UDPdata[34] != "0")
                    {
                        Remove2.Clear();
                        for (int i = 0; i < Convert.ToInt32(UDPdata[14]) * 2; i++)
                        {
                            int beginX = Convert.ToInt32(UDPRemove2[i]);
                            i++;
                            int beginY = Convert.ToInt32(UDPRemove2[i]);
                            Remove2.Add(new System.Drawing.Point(beginX, beginY));
                        }
                    }
                    tcpClient.Disconnect(true);
                    GetReceive = false;
                }
                catch
                {
                    test = "No Listening";
                }
                
            }
        }
        //---------------------------------UDP获取配置

        public void GrabPic()
        {
            VideoCapture capture = null;
            Mat img;
            string url = "rtsp://" + UserID + ":" + PSW + "@" + CamIP + "/LiveMedia/ch1/Media2";
            int i;
            try
            {
                capture = new VideoCapture(url);
                while (capture.Grab())
                {
                    if (Q.Count < 3)
                    {
                        try
                        {
                            img = capture.RetrieveMat();
                            if (BTN.Count >= 2)
                            {
                                float startPX = 0, startPY = 0, endPX = 0, endPY = 0;
                                for (i = 0; i < BTN.Count - 1; i++)
                                {
                                    //(btnLocX - skinPictureBox1.Location.X) / skinPictureBox1.Width * 999;
                                    startPX = (float)(BTN[i].Location.X + 3 - skinPictureBox1.Location.X) / skinPictureBox1.Width * (img.Width-1);
                                    startPY = (float)(BTN[i].Location.Y + 3 - skinPictureBox1.Location.Y) / skinPictureBox1.Height * (img.Height-1);
                                    endPX = (float)(BTN[i + 1].Location.X + 3 - skinPictureBox1.Location.X) / skinPictureBox1.Width * (img.Width-1);
                                    endPY = (float)(BTN[i + 1].Location.Y + 3 - skinPictureBox1.Location.Y) / skinPictureBox1.Height * (img.Height-1);
                                    OpenCvSharp.Point startP0 = new OpenCvSharp.Point(Convert.ToInt32(startPX), Convert.ToInt32(startPY));
                                    OpenCvSharp.Point endP0 = new OpenCvSharp.Point(Convert.ToInt32(endPX), Convert.ToInt32(endPY));
                                    Cv2.Line(img, startP0, endP0, Scalar.Red, 1);
                                    if (((i + 1 == Pnum - 1) || (i + 1 == ign1 - 1) || (i + 1 == ign2 - 1)) || ((i + 1 == BTN.Count - 1)&&ign1==0&&ign2==0&&Pnum==0))
                                    {
                                        startPX = (float)(BTN[0].Location.X + 3 - skinPictureBox1.Location.X) / skinPictureBox1.Width * (img.Width-1);
                                        startPY = (float)(BTN[0].Location.Y + 3 - skinPictureBox1.Location.Y) / skinPictureBox1.Height * (img.Height-1);
                                        OpenCvSharp.Point startP = new OpenCvSharp.Point(Convert.ToInt32(startPX), Convert.ToInt32(startPY));
                                        OpenCvSharp.Point endP = new OpenCvSharp.Point(Convert.ToInt32(endPX), Convert.ToInt32(endPY));
                                        Cv2.Line(img, endP, startP, Scalar.Red, 1);
                                    }
                                }
                            }
                            Q.Enqueue(img);
                        }
                        catch
                        {
                            capture = new VideoCapture(url);
                            capture.Grab();
                        }
                    }
                    Thread.Sleep(10);
                }
                capture = null;
            }
            catch
            {
                if (capture == null)
                    MessageBox.Show("抓取线程错误", "提示", MessageBoxButtons.OK);
            }
        }
        //---------------------------------抓取并绘制图像

        public void ShowPic()
        {
            Mat img;
            try
            {
                while (true)
                {
                    if (Q.Count > 0)
                    {
                        img = Q.Dequeue();
                        skinPictureBox1.Image = img.ToBitmap();
                    }
                    Thread.Sleep(10);
                }
            }
            catch
            {
                Thread.Sleep(10);
            }
        }
        //---------------------------------显示图像

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            if (IDBOX.Text == "" || PSWBOX.Text == "" || IPBOX.Text == "")
            {
                MessageBox.Show("请输入正确的登录信息!", "提示", MessageBoxButtons.OK);
            }
            else
            {
                UserID = IDBOX.Text;
                PSW = PSWBOX.Text;
                CamIP = IPBOX.Text;
                ReloadALL(0);
                if (ClrThread)
                {
                    Grab.Abort();
                    ShowImg.Abort();
                    SockListening.Abort();
                    Grab.DisableComObjectEagerCleanup();
                    ShowImg.DisableComObjectEagerCleanup();
                    SockListening.DisableComObjectEagerCleanup();
                    UDPdata.Clear();
                }
                try
                {
                    Grab = new Thread(new ThreadStart(GrabPic));
                    ShowImg = new Thread(new ThreadStart(ShowPic));
                    Grab.IsBackground = true;
                    ShowImg.IsBackground = true;
                    Grab.Start();
                    ShowImg.Start();
                    Concert.Text = "开始标记";
                    Concert.BaseColor = Color.FromArgb(0, 50, 0);
                    RemoveArea1.Text = "开始标记";
                    RemoveArea1.BaseColor = Color.FromArgb(0, 50, 0);
                    RemoveArea2.Text = "开始标记";
                    RemoveArea2.BaseColor = Color.FromArgb(0, 50, 0);
                    ClrThread = true;
                    
                }
                catch
                {
                    MessageBox.Show("线程启动失败!", "提示", MessageBoxButtons.OK);
                }
                Send(CamIP, "#beibohdrop_GET#");
            }
        }
        //----------------------------------登录

        private void skinPictureBox1_Click(object sender, EventArgs e)
        {
            if ((BTN.Count < Pnum || BTN.Count < ign1 || BTN.Count < ign2) && index != 0)
            {
                int btnLocX = MousePosition.X - this.Location.X;
                int btnLocY = MousePosition.Y - this.Location.Y;
                Button btn = new Button
                {
                    Location = new System.Drawing.Point(btnLocX-3, btnLocY-3),
                    Size = new System.Drawing.Size(8, 8)
                };
                btn.MouseDown += Button_MouseDown;
                btn.MouseMove += Button_MouseMove;
                BTN.Add(btn);
                this.Controls.Add(btn);
                btn.BringToFront();
                double UDPX = ((double)btn.Location.X - skinPictureBox1.Location.X + 3) / skinPictureBox1.Width * 999.9;
                double UDPY = ((double)btn.Location.Y - skinPictureBox1.Location.Y + 3) / skinPictureBox1.Height * 999.9;
                switch (index)
                {
                    case 1:
                        BTN999.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                        break;
                    case 2:
                        Remove1.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                        break;
                    case 3:
                        Remove2.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                        break;
                }
                AllLocation.Text += "(" + Convert.ToInt32(UDPX).ToString() + "," + Convert.ToInt32(UDPY).ToString() + ");";
                showCount.Text = "已标记点数：" + BTN.Count.ToString();
            }
        }
        //---------------------------------- 画点

        public void ReloadALL(int index0)
        {
            Pnum = 0;
            ign1 = 0;
            ign2 = 0;
            for (int i = 0; i < BTN.Count; i++)
            {
                this.Controls.Remove(BTN[i]);
            }
            BTN.Clear();
            switch (index0)
            {
                case 0:
                    BTN999.Clear();
                    Remove1.Clear();
                    Remove2.Clear();
                    break;
                case 1:
                    Concert.Text = "开始标记";
                    Concert.BaseColor = Color.FromArgb(0, 50, 0);
                    BTN999.Clear();
                    break;
                case 2:
                    RemoveArea1.Text = "开始标记";
                    RemoveArea1.BaseColor = Color.FromArgb(0, 50, 0);
                    Remove1.Clear();
                    break;
                case 3:
                    RemoveArea2.Text = "开始标记";
                    RemoveArea2.BaseColor = Color.FromArgb(0, 50, 0);
                    Remove2.Clear();
                    break;
            }
            PnumBox.SelectedIndex = -1;
            ignorePnum1.SelectedIndex = -1;
            ignorePnum2.SelectedIndex = -1;
            showCount.Text = "未启用标记...";
        }
        //-------------------------------重刷所有数据

        private void SendBtn_Click(object sender, EventArgs e)
        {
            if ((BTN.Count != Pnum && Pnum != 0) || (Remove1.Count != ign1 && ign1 != 0) || (Remove2.Count != ign2 && ign2 != 0))
            {
                MessageBox.Show("标记未完成！请先完成标记或取消标记后保存!", "提示", MessageBoxButtons.OK);
            }
            else if (BTN999.Count == 0 && Remove1.Count == 0 && Remove2.Count == 0)
            {
                MessageBox.Show("无数据!", "提示", MessageBoxButtons.OK);
            }
            else if (Convert.ToInt32(threshold.Text) > 100 || Convert.ToInt32(threshold.Text) < 0 ||
                Convert.ToInt32(jitter.Text) > 6 || Convert.ToInt32(jitter.Text) < 1 ||
                Convert.ToInt32(min_alarm_h_radio.Text) > 100 || Convert.ToInt32(min_alarm_h_radio.Text) < 0 ||
                Convert.ToInt32(hw_radio.Text) > 10 || Convert.ToInt32(hw_radio.Text) < 0 ||
                Convert.ToInt32(max_contours_result.Text) > 100 || Convert.ToInt32(max_contours_result.Text) < 0 ||
                Convert.ToInt32(aera_radio.Text) > 100 || Convert.ToInt32(aera_radio.Text) < 0)
            {
                MessageBox.Show("数据不符合要求！", "提示", MessageBoxButtons.OK);
            }
            else
            {
                int testUDP = 0;
                AllLocation.Text = "";
                try
                {
                    UDPdata[2] = threshold.Text;
                    UDPdata[4] = jitter.Text;
                    UDPdata[6] = min_alarm_h_radio.Text;
                    UDPdata[8] = hw_radio.Text;
                    UDPdata[10] = max_contours_result.Text;
                    UDPdata[12] = aera_radio.Text;
                    if (BTN999.Count >= 4)
                    {
                        UDPdata[14] = BTN999.Count.ToString();
                        UDPdata[16] = "";
                        UDPdata[18] = "";
                        UDPdata[20] = "";
                        AllLocation.Text += "警戒区域：" + "\r\n";
                        for (int i = 0; i < BTN999.Count; i++)
                        {
                            UDPBTN[2 * i] = BTN999[i].X.ToString();
                            UDPBTN[2 * i + 1] = BTN999[i].Y.ToString();
                        }
                        for (int i = 0; i < UDPBTN.Count; i++)
                        {
                            UDPdata[16 + (i / 4) * 2] += UDPBTN[i];
                            if ((i - 3) % 4 != 0)
                                UDPdata[16 + (i / 4) * 2] += "-";
                        }
                    }
                    AllLocation.Text += UDPdata[16] + "\r\n";
                    AllLocation.Text += UDPdata[18] + "\r\n";
                    AllLocation.Text += UDPdata[20] + "\r\n";
                    AllLocation.Text += "屏蔽区1：" + "\r\n";
                    if (skinCheckBox1.Checked && Remove1.Count >= 4)
                    {
                        UDPdata[22] = "1";
                        UDPdata[26] = "";
                        UDPdata[28] = "";
                        UDPdata[30] = "";
                        for (int i = 0; i < Remove1.Count; i++)
                        {
                            UDPRemove1[2 * i] = Remove1[i].X.ToString();
                            UDPRemove1[2 * i + 1] = Remove1[i].Y.ToString();
                        }
                        testUDP = UDPRemove1.Count;
                        for (int i = 0; i < UDPRemove1.Count; i++)
                        {
                            UDPdata[26 + (i / 4) * 2] += UDPRemove1[i];
                            if ((i - 3) % 4 != 0)
                                UDPdata[26 + (i / 4) * 2] += "-";
                        }
                        UDPdata[24] = Remove1.Count.ToString();
                    }
                    else
                    {
                        UDPdata[22] = "0";
                        UDPdata[24] = "6";
                    }
                    AllLocation.Text += UDPdata[26] + "\r\n";
                    AllLocation.Text += UDPdata[28] + "\r\n";
                    AllLocation.Text += UDPdata[30] + "\r\n";
                    AllLocation.Text += "屏蔽区2：" + "\r\n";

                    if (skinCheckBox2.Checked && Remove2.Count >= 4)
                    {
                        UDPdata[32] = "1";
                        UDPdata[36] = "";
                        UDPdata[38] = "";
                        UDPdata[40] = "";
                        for (int i = 0; i < Remove2.Count; i++)
                        {
                            UDPRemove2[2 * i] = Remove2[i].X.ToString();
                            UDPRemove2[2 * i + 1] = Remove2[i].Y.ToString();
                        }
                        for (int i = 0; i < UDPRemove2.Count; i++)
                        {
                            UDPdata[36 + (i / 4) * 2] += UDPRemove2[i];
                            if ((i - 3) % 4 != 0)
                                UDPdata[36 + (i / 4) * 2] += "-";

                        }
                        UDPdata[34] = Remove2.Count.ToString();
                    }
                    else
                    {
                        UDPdata[32] = "0";
                        UDPdata[34] = "6";
                    }
                        AllLocation.Text += UDPdata[36] + "\r\n";
                    AllLocation.Text += UDPdata[38] + "\r\n";
                    AllLocation.Text += UDPdata[40] + "\r\n";


                    if (ShowTest.Checked)
                        UDPdata[42] = "1";
                    else UDPdata[42] = "0";

                    string msg = "#beibohdrop_UPDATE# ";
                    for (int j = 1; j < UDPdata.Count; j++)
                        msg += UDPdata[j] + " ";
                    Send(CamIP, msg);
                    MessageBox.Show("提交成功！", "提示", MessageBoxButtons.OK);
                }
                catch
                {
                    AllLocation.Text = testUDP.ToString();
                    MessageBox.Show("提交失败！", "提示", MessageBoxButtons.OK); }
            }
        }
        //-------------------------------确认发送数据

        private void Input_Limit(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9')))
            {
                e.Handled = true;
            }
        }
        //------------------------------输入框限制只能输入数字

        private void reset_Click(object sender, EventArgs e)
        {
            if (UDPdata.Count < 42)
                MessageBox.Show("未登录 或 数据接收错误", "提示", MessageBoxButtons.OK);
            else
            {
                PnumBox.SelectedIndex = -1;
                ignorePnum1.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                BTN999.Clear();
                Remove1.Clear();
                Remove2.Clear();
                if (UDPdata[14] != "0")
                {
                    for (int i = 0; i < Convert.ToInt32(UDPdata[14]) * 2; i++)
                    {
                        int beginX = Convert.ToInt32(UDPBTN[i]);
                        i++;
                        int beginY = Convert.ToInt32(UDPBTN[i]);
                        BTN999.Add(new System.Drawing.Point(beginX, beginY));
                    }
                }
                if (UDPdata[22] != "0" && UDPdata[24] != "0")
                {
                    for (int i = 0; i < Convert.ToInt32(UDPdata[24]) * 2; i++)
                    {
                        int beginX = Convert.ToInt32(UDPRemove1[i]);
                        i++;
                        int beginY = Convert.ToInt32(UDPRemove1[i]);
                        Remove1.Add(new System.Drawing.Point(beginX, beginY));
                    }
                }
                if (UDPdata[32] != "0" && UDPdata[34] != "0")
                {
                    for (int i = 0; i < Convert.ToInt32(UDPdata[14]) * 2; i++)
                    {
                        int beginX = Convert.ToInt32(UDPRemove2[i]);
                        i++;
                        int beginY = Convert.ToInt32(UDPRemove2[i]);
                        Remove2.Add(new System.Drawing.Point(beginX, beginY));
                    }
                }
                if (BTN999.Count != 0)
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50,50,0);
                }
                if (Remove1.Count != 0 && UDPdata[22] != "0")
                {
                    skinCheckBox1.CheckState = CheckState.Checked;
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                else
                {
                    RemoveArea1.Text = "开始标记";
                    RemoveArea1.BaseColor = Color.FromArgb(0, 50, 0);
                }
                if (Remove2.Count != 0 && UDPdata[32] != "0")
                {
                    skinCheckBox2.CheckState = CheckState.Checked;
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                else
                {
                    RemoveArea2.Text = "开始标记";
                    RemoveArea2.BaseColor = Color.FromArgb(0, 50, 0);
                }
                if (UDPdata.Count > 10)
                {
                    Threshold = UDPdata[2];
                    Jitter = UDPdata[4];
                    MinAlarm = UDPdata[6];
                    hwRadio = UDPdata[8];
                    Maxcontours = UDPdata[10];
                    aeraRadio = UDPdata[12];
                }
                threshold.Text = Threshold;
                jitter.Text = Jitter;
                min_alarm_h_radio.Text = MinAlarm;
                hw_radio.Text = hwRadio;
                max_contours_result.Text = Maxcontours;
                aera_radio.Text = aeraRadio;
            }
        }
        //------------------------------------------查看配置

        private void SaveNow_Click(object sender, EventArgs e)
        {
            if ((BTN999.Count != Pnum && Pnum != 0) || (Remove1.Count != ign1 && ign1 != 0) || (Remove2.Count != ign2 && ign2 != 0))
            {
                MessageBox.Show("当前标记未完成！\n请先完成标记或取消后保存!", "提示", MessageBoxButtons.OK);
            }
            else if (BTN999.Count == 0 && Remove1.Count == 0 && Remove2.Count == 0)
            {
                MessageBox.Show("无数据!", "提示", MessageBoxButtons.OK);
            }
            else
            {

                if ((BTN.Count == Pnum || BTN.Count == BTN999.Count)&& BTN999.Count >= 4)
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if ((BTN.Count == ign1 || BTN.Count == Remove1.Count)&& Remove1.Count >= 4)
                {
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if ((BTN.Count == ign2 || BTN.Count == Remove2.Count)&& Remove2.Count >= 4)
                {
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                Pnum = 0;
                ign1 = 0;
                ign2 = 0;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                BTN.Clear();
                PnumBox.SelectedIndex = -1;
                ignorePnum1.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                showCount.Text = "未启用标记...";
            }
        }
        //------------------------------------------暂存


        private void Concert_Click(object sender, EventArgs e)
        {
            if (PnumBox.Text == "" && BTN999.Count == 0)
            {
                MessageBox.Show("请选择标记点数", "提示", MessageBoxButtons.OK);
            }
            else if ((BTN999.Count != Pnum && Pnum != 0) || (Remove1.Count != ign1 && ign1 != 0) || (Remove2.Count != ign2 && ign2 != 0))
            {
                MessageBox.Show("当前标记未完成！\n请先完成标记或取消后保存!", "提示", MessageBoxButtons.OK);
            }
            else if (BTN999.Count != 0 && Concert.Text != "查看标记")     //当已启用标记时,重置数据和按键状态
            {
                AllLocation.Text = "";
                ReloadALL(1);
                index = 0;
            }
            else if (Concert.Text == "查看标记")
            {
                if (RemoveArea1.Text == "重新标记")
                {
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if (RemoveArea2.Text == "重新标记")
                {
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                PnumBox.Text = BTN999.Count.ToString();
                ignorePnum1.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                BTN.Clear();
                AllLocation.Text = ":";
                for (int i = 0; i < BTN999.Count; i++)
                {
                    float btnLocX = (float)BTN999[i].X * skinPictureBox1.Width / 999 + skinPictureBox1.Location.X - 3;
                    float btnLocY = (float)BTN999[i].Y * skinPictureBox1.Height / 999 + skinPictureBox1.Location.Y - 3;
                    Button btn = new Button
                    {
                        Location = new System.Drawing.Point((int)btnLocX, (int)btnLocY),
                        Size = new System.Drawing.Size(8, 8)
                    };
                    BTN.Add(btn);
                    this.Controls.Add(btn);
                    btn.BringToFront();
                    AllLocation.Text += "(" + Convert.ToInt32(BTN999[i].X).ToString() + "," + Convert.ToInt32(BTN999[i].Y).ToString() + "); ";
                }
                Concert.Text = "重新标记";
                Concert.BaseColor = Color.FromArgb(50, 0, 0);
            }
            else
            {
                if (RemoveArea1.Text == "重新标记")
                {
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if (RemoveArea2.Text == "重新标记")
                {
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                Pnum = 0;
                ign1 = 0;
                ign2 = 0;
                ignorePnum1.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                AllLocation.Text = "";
                BTN.Clear();
                Pnum = Convert.ToInt32(PnumBox.Text);//获取需要的点数
                showCount.Text = "标记就绪...";
                Concert.Text = "重新标记";
                Concert.BaseColor = Color.FromArgb(50, 0, 0);
                index = 1;
            }
        }
        //---------------------------------确认开始画算法区域
        private void RemoveArea1_Click(object sender, EventArgs e)
        {
            if (ignorePnum1.Text == "" && Remove1.Count == 0)
            {
                MessageBox.Show("请选择标记点数", "提示", MessageBoxButtons.OK);
            }
            else if ((BTN999.Count != Pnum && Pnum != 0) || (Remove1.Count != ign1 && ign1 != 0) || (Remove2.Count != ign2 && ign2 != 0))
            {
                MessageBox.Show("当前标记未完成！\n请先完成标记或取消后保存!", "提示", MessageBoxButtons.OK);
            }
            else if (Remove1.Count != 0 && RemoveArea1.Text != "查看标记")     //当已启用标记时,重置数据和按键状态
            {
                AllLocation.Text = "";
                ReloadALL(2);
                index = 0;
            }
            else if (RemoveArea1.Text == "查看标记")
            {
                if (Concert.Text == "重新标记")
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50, 50, 0);

                }
                if (RemoveArea2.Text == "重新标记")
                {
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                ignorePnum1.Text = Remove1.Count.ToString();
                PnumBox.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                BTN.Clear();
                AllLocation.Text = ":";
                for (int i = 0; i < Remove1.Count; i++)
                {
                    float btnLocX = (float)Remove1[i].X * skinPictureBox1.Width / 999 + skinPictureBox1.Location.X - 3;
                    float btnLocY = (float)Remove1[i].Y * skinPictureBox1.Height / 999 + skinPictureBox1.Location.Y - 3;
                    Button btn = new Button
                    {
                        Location = new System.Drawing.Point((int)btnLocX, (int)btnLocY),
                        Size = new System.Drawing.Size(8, 8)
                    };
                    BTN.Add(btn);
                    this.Controls.Add(btn);
                    btn.BringToFront();
                    AllLocation.Text += "(" + Convert.ToInt32(Remove1[i].X).ToString() + "," + Convert.ToInt32(Remove1[i].Y).ToString() + "); ";
                }
                RemoveArea1.Text = "重新标记";
                RemoveArea1.BaseColor = Color.FromArgb(50, 0, 0);
            }
            else
            {
                if (Concert.Text == "重新标记")
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if (RemoveArea2.Text == "重新标记")
                {
                    RemoveArea2.Text = "查看标记";
                    RemoveArea2.BaseColor = Color.FromArgb(50, 50, 0);
                }
                Pnum = 0;
                ign1 = 0;
                ign2 = 0;
                PnumBox.SelectedIndex = -1;
                ignorePnum2.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                AllLocation.Text = "";
                BTN.Clear();
                ign1 = Convert.ToInt32(ignorePnum1.Text);//获取需要的点数
                showCount.Text = "标记就绪...";
                RemoveArea1.Text = "重新标记";
                RemoveArea1.BaseColor = Color.FromArgb(50, 0, 0);
                index = 2;
            }
        }
        //---------------------------------确认开始画屏蔽区域1
        private void RemoveArea2_Click(object sender, EventArgs e)
        {
            if (ignorePnum2.Text == "" && Remove2.Count == 0)
            {
                MessageBox.Show("请选择标记点数", "提示", MessageBoxButtons.OK);
            }
            else if ((BTN999.Count != Pnum && Pnum != 0) || (Remove1.Count != ign1 && ign1 != 0) || (Remove2.Count != ign2 && ign2 != 0))
            {
                MessageBox.Show("当前标记未完成！\n请先完成标记或取消后保存!", "提示", MessageBoxButtons.OK);
            }
            else if (Remove2.Count != 0 && RemoveArea2.Text != "查看标记")     //当已启用标记时,重置数据和按键状态
            {
                AllLocation.Text = "";
                ReloadALL(3);
                index = 0;
            }
            else if (RemoveArea2.Text == "查看标记")
            {
                if (RemoveArea1.Text == "重新标记")
                {
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if (Concert.Text == "重新标记")
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50, 50, 0);
                }
                ignorePnum2.Text = Remove2.Count.ToString();
                PnumBox.SelectedIndex = -1;
                ignorePnum1.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                BTN.Clear();
                AllLocation.Text = ":";
                for (int i = 0; i < Remove2.Count; i++)
                {
                    float btnLocX = (float)Remove2[i].X * skinPictureBox1.Width / 999 + skinPictureBox1.Location.X - 3;
                    float btnLocY = (float)Remove2[i].Y * skinPictureBox1.Height / 999 + skinPictureBox1.Location.Y - 3;
                    Button btn = new Button
                    {
                        Location = new System.Drawing.Point((int)btnLocX, (int)btnLocY),
                        Size = new System.Drawing.Size(8, 8)
                    };
                    BTN.Add(btn);
                    this.Controls.Add(btn);
                    btn.BringToFront();
                    AllLocation.Text += "(" + Convert.ToInt32(Remove2[i].X).ToString() + "," + Convert.ToInt32(Remove2[i].Y).ToString() + "); ";
                }
                RemoveArea2.Text = "重新标记";
                RemoveArea2.BaseColor = Color.FromArgb(50, 0, 0);
            }
            else
            {
                if (RemoveArea1.Text == "重新标记")
                {
                    RemoveArea1.Text = "查看标记";
                    RemoveArea1.BaseColor = Color.FromArgb(50, 50, 0);
                }
                if (Concert.Text == "重新标记")
                {
                    Concert.Text = "查看标记";
                    Concert.BaseColor = Color.FromArgb(50, 50, 0);
                }
                Pnum = 0;
                ign1 = 0;
                ign2 = 0;
                PnumBox.SelectedIndex = -1;
                ignorePnum1.SelectedIndex = -1;
                for (int i = 0; i < BTN.Count; i++)
                {
                    this.Controls.Remove(BTN[i]);
                }
                AllLocation.Text = "";
                BTN.Clear();
                ign2 = Convert.ToInt32(ignorePnum2.Text);//获取需要的点数
                showCount.Text = "标记就绪...";
                RemoveArea2.Text = "重新标记";
                RemoveArea2.BaseColor = Color.FromArgb(50, 0, 0);
                index = 3;
            }
        }
        //---------------------------------确认开始画屏蔽区域2




        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            location = e.Location;
            B = sender as Button;
        }
        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            int posX, posY;
            if (e.Button == MouseButtons.Left)
            {
                posX = B.Location.X + (e.X - location.X);
                posY = B.Location.Y + (e.Y - location.Y);
                if (posX > skinPictureBox1.Location.X && posY > skinPictureBox1.Location.Y &&
                posX < skinPictureBox1.Location.X + skinPictureBox1.Width && posY < skinPictureBox1.Location.Y + skinPictureBox1.Height)
                {
                    B.Location = new System.Drawing.Point(posX-3, posY-3);
                }
                switch (index)
                {
                    case 1:
                        BTN999.Clear();
                        break;
                    case 2:
                        Remove1.Clear();
                        break;
                    case 3:
                        Remove2.Clear();
                        break;
                }
                AllLocation.Text = "";
                for (int i = 0; i < BTN.Count; i++)
                {
                    double UDPX = ((double)BTN[i].Location.X - skinPictureBox1.Location.X + 3) / skinPictureBox1.Width * 999.9;
                    double UDPY = ((double)BTN[i].Location.Y - skinPictureBox1.Location.Y + 3) / skinPictureBox1.Height * 999.9;
                    switch (index)
                    {
                        case 1:
                            BTN999.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                            break;
                        case 2:
                            Remove1.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                            break;
                        case 3:
                            Remove2.Add(new System.Drawing.Point((int)UDPX, (int)UDPY));
                            break;
                    }
                    AllLocation.Text += "(" + Convert.ToInt32(UDPX).ToString() + "," + Convert.ToInt32(UDPY).ToString() + ");";
                }
            }
        }
    }
}