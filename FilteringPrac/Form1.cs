using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace FilteringPrac
{
    public partial class Form1 : Form
    {
        private delegate void DisplaySearchDelegate(ListView item);

        public static Socket ClientSocket;

        public static string Job_Type { get; set; }
        public static int Filtering_Total_Count;
        public static string filepath { get; set; }
        public static string fileName { get; set; }
        public static string message { get; set; }
        public static byte[] Buffer { get; set; }
        public static byte[] data { get; set; }

        public static List<FilteringData> list = new List<FilteringData>();

        public Form1()
        {
            InitializeComponent();
            // Set to details view.
            listView1.View = View.Details;
            // Add a column with width 20 and left alignment.
            listView1.Columns.Add("Job_Status", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Job_Type", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Client", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Server", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Schedule", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Files", 100, HorizontalAlignment.Left);
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClientSocket.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 텍스트박스에서 읽어온 고객이 검색한 Job_Type (검색기준)변수 
            Job_Type = textBox1.Text;
            String responseData = String.Empty;
            filepath = "C:\\Users\\BIT\\Desktop\\DownloadFromServer\\";
            string message;

            byte[] Buffer = new byte[1024];

            // 접속환영문구 수신
            responseData = String.Empty;
            int rev = ClientSocket.Receive(Buffer);
            if (rev <0)
            {
                throw new SocketException();
            }
            responseData = System.Text.Encoding.ASCII.GetString(Buffer, 0, rev);


            // 서버에 필터링 기준 정보를 메세지로 보냄. 
            message = string.Empty;
            message = Job_Type;
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            ClientSocket.Send(data);

            Buffer = new byte[1024];
            rev = ClientSocket.Receive(Buffer, 0, 23, 0);
            if (rev < 0)
            {
                throw new SocketException();
            }
            int fileNameLen = BitConverter.ToInt32(Buffer, 0);
            fileName = Encoding.ASCII.GetString(Buffer, 4, fileNameLen);
            int fileSize = BitConverter.ToInt32(Buffer, 4 + fileNameLen + 1);

            Buffer = new byte[4096];
            BinaryWriter bWrite = new BinaryWriter(File.Open(filepath + fileName,
            FileMode.Create, FileAccess.Write));

            rev = 0;
            for (int i = 0; i < fileSize; i += rev)
            {
                Buffer = new byte[4096];
                rev = ClientSocket.Receive(Buffer,0);
                if (rev < 0)
                {
                    throw new SocketException();
                }
                bWrite.Write(Buffer,0,rev);
            }
            bWrite.Close();

            // csv 파일 읽기
            ReadCSVFile();

            // 리스트뷰에 내용 추가 
            for (int i = 0; i < list.Count; i++)
            {
                string[] strs = new string[] { list[i].Filtering_Job_Status,
                list[i].Filtering_Job_Type, list[i].Filtering_Client, list[i].Filtering_Server,
                list[i].Filtering_Schedule, list[i].Filtering_Files };
                var item = new ListViewItem(strs);
                listView1.Items.Add(item);
            }
        }


        static void ReadCSVFile()
        {
            var lines = File.ReadAllLines(Form1.filepath + Form1.fileName);
            foreach (var line in lines)
            {
                var values = line.Split(',');
                var filteringdata = new FilteringData();
                filteringdata.Filtering_Job_Status = values[0];
                filteringdata.Filtering_Job_Type = values[1];
                filteringdata.Filtering_Client = values[2];
                filteringdata.Filtering_Server = values[3];
                filteringdata.Filtering_Schedule = values[4];
                filteringdata.Filtering_Files = values[5];
                list.Add(filteringdata);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Form1 form = new Form1();
                IPAddress ipAddress = IPAddress.Parse("192.168.0.12");
                int port = 7755;
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                ClientSocket.Connect(iPEndPoint);
                // 버퍼 
                Buffer = new byte[1024];

                // 클라이언트측에서 서버에게 "접속완료" 문구보냄.
                message = "Filtering_Data";
                data = System.Text.Encoding.ASCII.GetBytes(message);
                ClientSocket.Send(data);
            }
            catch
            {
                MessageBox.Show("Socket Connection Error");
            }
        }
    }
    public class FilteringData
    {
        // 필터링 데이터 6개
        public string Filtering_Job_Status { get; set; } 
        public string Filtering_Job_Type { get; set; }
        public string Filtering_Client { get; set; }
        public string Filtering_Server { get; set; }
        public string Filtering_Schedule { get; set; }
        public string Filtering_Files { get; set; }
    }
}

