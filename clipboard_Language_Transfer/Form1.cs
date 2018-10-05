using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tulpep.NotificationWindow;
namespace clipboard_Language_Transfer
{
    public partial class Form1 : Form
    {
        static string NAVERCLIENTID = "YOUR_CLIENTID";
        static string NAVERSECRET = "YOUR_SECRET";

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private IntPtr _ClipboardViewerNext;

        private const int WM_DRAWCLIPBOARD = 0x0308;
        // private const int WM_CUT = 0x0301; 


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Opacity = 0;
            StartClipboardViewer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopClipboardViewer();
        } 

        private void StartClipboardViewer()
        {
            _ClipboardViewerNext = SetClipboardViewer(this.Handle);
        }

        private void StopClipboardViewer()
        {
            ChangeClipboardChain(this.Handle, _ClipboardViewerNext);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_DRAWCLIPBOARD)
            {
                SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                if (Clipboard.ContainsText())
                {

                    String TransferTargetText = Clipboard.GetText();

                    string LanguageType = JObject.Parse(GetPaPagoLanguageType(TransferTargetText))["langCode"].ToString();

                    if (LanguageType != "ko")
                    {
                        String SMTTransferText = GetPapagoSMTTransferText(TransferTargetText, LanguageType);
                        JObject SMTTransferResult = JObject.Parse(SMTTransferText);
                        String SMTResultText = SMTTransferResult["message"]["result"]["translatedText"].ToString();

                        String NMTTransferText = GetPapagoNMTTransferText(TransferTargetText, LanguageType);
                        JObject NMTTransferResult = JObject.Parse(NMTTransferText);
                        String NMTResultText = NMTTransferResult["message"]["result"]["translatedText"].ToString();

                        PopupNotifier popup = new PopupNotifier();
                        popup.Image = Properties.Resources.transfer_icon;
                        popup.TitleText = "This Language is " + LanguageType;
                        popup.ContentText = "[SMT]Transfer:  " + SMTResultText + "\n\n" + "[NMT]Transfer:  " + NMTResultText;
                        popup.Popup();
                    }
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }


        static String GetPapagoSMTTransferText(string TargetText, string LanguageType)
        {
            string url = "https://openapi.naver.com/v1/language/translate";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", NAVERCLIENTID);
            request.Headers.Add("X-Naver-Client-Secret", NAVERSECRET);
            request.Method = "POST";
            string query = TargetText;
            byte[] byteDataParams = Encoding.UTF8.GetBytes("source=" + LanguageType + "&target=ko&text=" + query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            stream.Close();
            response.Close();
            reader.Close();

            return text;
        }

        static String GetPapagoNMTTransferText(string TargetText, string LanguageType)
        {
            string url = "https://openapi.naver.com/v1/papago/n2mt";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", NAVERCLIENTID);
            request.Headers.Add("X-Naver-Client-Secret", NAVERSECRET);
            request.Method = "POST";
            string query = TargetText;
            byte[] byteDataParams = Encoding.UTF8.GetBytes("source=" + LanguageType + "&target=ko&text=" + query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            stream.Close();
            response.Close();
            reader.Close();

            return text;
        }

        static String GetPaPagoLanguageType(string TargetText){
            string url = "https://openapi.naver.com/v1/papago/detectLangs";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", NAVERCLIENTID);
            request.Headers.Add("X-Naver-Client-Secret", NAVERSECRET);
            request.Method = "POST";
            string query = TargetText;
            byte[] byteDataParams = Encoding.UTF8.GetBytes("query=" + query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            stream.Close();
            response.Close();
            reader.Close();

            return text;
        }



    }
}
