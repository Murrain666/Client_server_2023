using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Имя клиента.
        /// </summary>
        string userName;

        /// <summary>
        /// Адрес сервера.
        /// </summary>
        const string host = "127.0.0.1";

        /// <summary>
        /// Порт сервера.
        /// </summary>
        const int port = 8005;

        TcpClient client;
        NetworkStream stream;

        /// <summary>
        /// Конструктор класса MainForm.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Метод подключения к серверу.
        /// </summary>
        private void Connection()
        {
            client = new TcpClient();
            try
            {
                client.Connect(host, port);  // подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage))
                {
                    IsBackground = true
                };
                receiveThread.Start(); //старт потока
                AllTextBox.Text = string.Format("Здраствуй, {0} \n", userName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Внимание!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Метод отправки сообщений.
        /// </summary>
        private void SendMessage()
        {
            string message = MyTextBox.Text;
            AllTextBox.Text += message + "\n";
            MyTextBox.Text = string.Empty;
            if (message != string.Empty)
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Метод получения сообщений.
        /// </summary>
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    if (AllTextBox.InvokeRequired)
                        AllTextBox.Invoke(new Action<string>((s) => AllTextBox.AppendText(s + "\n")), message);
                    else
                        AllTextBox.AppendText(message);
                }
                catch
                {
                    MessageBox.Show("Подключение прервано!", "Внимание!",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Метод отключения от сервера.
        /// </summary>
        private void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Application.Exit();
        }

        /// <summary>
        /// Событие при нажатии на кнопку применить
        /// </summary>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text != string.Empty)
            {
                NameTextBox.Enabled = false;
                ApplyButton.Enabled = false;
                MyTextBox.Enabled = true;
                SendButton.Enabled = true;
                this.Text = "Клиент: " + NameTextBox.Text;
                userName = NameTextBox.Text;
                Connection();
            }
            else
            {
                MessageBox.Show("Необходимо заполнить поле имя.",
                    "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Событие при нажатии на кнопку отправить.
        /// </summary>
        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
    }
}
