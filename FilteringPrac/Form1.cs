using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;

namespace FilteringPrac
{
    public partial class Form1 : Form
    {
        public string Job_Type { get; set; }
        public static Socket ClientSocket;
        public static int Filtering_Total_Count;
        public static string filepath { get; set; }
        public static string fileName { get; set; }
        public Form1()
        {
            IPAddress ipAddress = IPAddress.Parse("192.168.0.12");
            int port = 7754;
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ClientSocket.Connect(iPEndPoint);

            // 버퍼 
            byte[] Buffer = new byte[1024];

            // 클라이언트측에서 서버에게 "접속완료" 문구보냄.
            string message = "Filtering_Data";
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            ClientSocket.Send(data);

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 텍스트박스에서 읽어온 고객이 검색한 Job_Type 변수 
            Job_Type = textBox1.Text;
            String responseData = String.Empty;
            filepath = "C:\\Users\\BIT\\Desktop\\DownloadFromServer\\";
            string message;

            byte[] Buffer = new byte[1024];

            // 접속환영문구 수신
            responseData = String.Empty;
            int rev = ClientSocket.Receive(Buffer);
            responseData = System.Text.Encoding.ASCII.GetString(Buffer, 0, rev);

            // 서버에 필터링 기준 정보를 메세지로 보냄. 
            message = string.Empty;
            message = Job_Type;
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            ClientSocket.Send(data);

            byte[] LargeBuffer = new byte[1024*100];
            //// 첫 파일 구조체 정보 및 파일 수신 
            rev = ClientSocket.Receive(LargeBuffer);
            int fileNameLen = BitConverter.ToInt32(LargeBuffer, 0);
            fileName = Encoding.ASCII.GetString(LargeBuffer, 4, fileNameLen);

            //// 첫 파일 저장 
            BinaryWriter bWrite = new BinaryWriter(File.Open(filepath + fileName, FileMode.Create, FileAccess.Write));
            bWrite.Write(LargeBuffer, 4 + fileNameLen + 1 , rev - 4 - fileNameLen - 1);
            bWrite.Close();

            // comma seperated value 
            ReadCSVFile();
        }

        static void ReadCSVFile()
        {
            var lines = File.ReadAllLines(Form1.filepath + Form1.fileName);
            var list = new List<FilteringData>();
            foreach (var line in lines)
            {
                var values = line.Split(',');
                var filteringdata = new FilteringData() { Filtering_Job_Status = values[0], Filtering_Job_Type = values[1],
                    Filtering_Client = values[2],
                    Filtering_Server = values[3],
                    Filtering_Schedule = values[4],
                    Filtering_Files = values[5]
                };
                list.Add(filteringdata);
            }
            list.ForEach( x=> Console.WriteLine($"{x.Filtering_Job_Status}\t{x.Filtering_Job_Type}\t{x.Filtering_Client}\t{x.Filtering_Server}\t{x.Filtering_Schedule}\t{x.Filtering_Files}"));
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
