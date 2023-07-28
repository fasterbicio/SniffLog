using Microsoft.Win32;
using SniffLogFW.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SniffLogFW
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; IsDisconnected = !value; OnPropertyChanged(); }
        }
        public bool IsDisconnected
        {
            get { return !isConnected; }
            set { isConnected = !value; OnPropertyChanged(); }
        }
        public bool Translate
        {
            get { return translate; }
            set { translate = value; OnPropertyChanged(); }
        }
        public bool Pause
        {
            get { return pause; }
            set { pause = value; OnPropertyChanged(); }
        }
        public bool Isolate
        {
            get { return isolate; }
            set { isolate = value; OnPropertyChanged(); }
        }

        private string[] serialPorts;
        private SerialPort port;
        private bool isConnected;
        private int slaveCatch;
        private int functionCatch;
        private bool messageDetected;
        private System.Timers.Timer checkIdle;
        private int memoBytesNumber = 0;
        private bool translate = true;
        private bool pause = false;
        private bool isolate = false;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = $"SniffLog {Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}";
            RefreshPort(Settings.Default.Port);
            baudCombo.SelectedIndex = Settings.Default.Baudrate;
            dataCombo.SelectedIndex = Settings.Default.DataBit;
            stopCombo.SelectedIndex = Settings.Default.StopBit;
            parityCombo.SelectedIndex = Settings.Default.Parity;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (portCombo.SelectedIndex == -1) return;

            SaveDefault();

            port = new SerialPort();

            port.PortName = serialPorts[portCombo.SelectedIndex];
            port.BaudRate = GetBaudrate();
            port.DataBits = GetDataBits();
            port.StopBits = GetStopBits();
            port.Parity = GetParity();

            port.ReadTimeout = 100;

            try
            {
                if (!port.IsOpen)
                    port.Open();

                if (port.IsOpen)
                {
                    IsConnected = true;
                    Log("Connected", false);
                }
                else
                    Log("Disconnected", false);
            }
            catch (Exception ex)
            {
                Log(ex.Message, true);
                IsConnected = false;
            }
            checkIdle = new System.Timers.Timer(1);
            checkIdle.Elapsed += new System.Timers.ElapsedEventHandler(CheckIdle_Tick);
            checkIdle.Start();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (port.IsOpen)
                port.Close();
            port.Dispose();
            IsConnected = false;
            Log("Disconnected", false);
            checkIdle.Stop();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "File di testo | *.txt";
            sfd.DefaultExt = ".txt";
            sfd.FileName = $"{DateTime.Now:yyyyMMddHHmmss}_comm";
            bool? result = sfd.ShowDialog();

            if (result == true)
                SaveLog(sfd.FileName);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            logBox.Items.Clear();
        }

        private void SlaveCatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(slaveCatchText.Text) | string.IsNullOrWhiteSpace(slaveCatchText.Text))
                slaveCatch = -1;
            else
            {
                try
                {
                    slaveCatch = ushort.Parse(slaveCatchText.Text);
                }
                catch
                {
                    slaveCatch = -1;
                }
            }
        }

        private void FunctionCatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(functionCatchText.Text) | string.IsNullOrWhiteSpace(functionCatchText.Text))
                functionCatch = -1;
            else
            {
                try
                {
                    functionCatch = ushort.Parse(functionCatchText.Text);
                }
                catch
                {
                    functionCatch = -1;
                }
            }
        }

        private void CheckIdle_Tick(object sender, EventArgs e)
        {
            if (port == null) return;
            if (!port.IsOpen) return;
            if (port.BytesToRead == 0) return;

            if (memoBytesNumber == port.BytesToRead)
            {
                memoBytesNumber = 0;
                ReadBytes(port.BytesToRead);
            }
            else
                memoBytesNumber = port.BytesToRead;
        }



        private void ReadBytes(int number)
        {
            byte[] buffer;

            try
            {
                buffer = new byte[number];
                port.Read(buffer, 0, number);
            }
            catch { buffer = new byte[0]; }

            List<ModbusPacket> modbus_packet = ModbusDecoder.Decode(buffer);
            if (modbus_packet == null) return;

            for (int i = 0; i < modbus_packet.Count; i++)
            {
                messageDetected = modbus_packet[i].Contains(slaveCatch, functionCatch);

                if ((isolate & messageDetected) | !isolate)
                {
                    if (modbus_packet[i].GetType() == typeof(ReadRegistersRequest))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((ReadRegistersRequest)modbus_packet[i]).ToString(), messageDetected);
                            Log(((ReadRegistersRequest)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(ReadRegistersResponse))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((ReadRegistersResponse)modbus_packet[i]).ToString(), messageDetected);
                            Log(((ReadRegistersResponse)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(ReadCoilsRequest))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((ReadCoilsRequest)modbus_packet[i]).ToString(), messageDetected);
                            Log(((ReadCoilsRequest)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(ReadCoilsResponse))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((ReadCoilsResponse)modbus_packet[i]).ToString(), messageDetected);
                            Log(((ReadCoilsResponse)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteRegistersRequest))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteRegistersRequest)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteRegistersRequest)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteRegistersResponse))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteRegistersResponse)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteRegistersResponse)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteCoilsRequest))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteCoilsRequest)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteCoilsRequest)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteCoilsResponse))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteCoilsResponse)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteCoilsResponse)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteSingleCoil))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteSingleCoil)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteSingleCoil)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(WriteSingleRegister))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((WriteSingleRegister)modbus_packet[i]).ToString(), messageDetected);
                            Log(((WriteSingleRegister)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else if (modbus_packet[i].GetType() == typeof(ExceptionResponse))
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(((ExceptionResponse)modbus_packet[i]).ToString(), messageDetected);
                            Log(((ExceptionResponse)modbus_packet[i]).Message(), messageDetected);
                        }
                    }
                    else
                    {
                        if (!pause)
                        {
                            if (translate)
                                Log(modbus_packet[i].ToString(), false);
                            Log(modbus_packet[i].Message(), false);
                        }
                    }
                }
            }
        }

        private void RefreshPort(string defaultPort)
        {
            serialPorts = SerialPort.GetPortNames();

            portCombo.Items.Clear();
            for (int i = 0; i < serialPorts.Length; i++)
            {
                portCombo.Items.Add(serialPorts[i]);
                if (serialPorts[i].Equals(defaultPort))
                    portCombo.SelectedIndex = i;
            }

            //portCombo.SelectedIndex = 0;
        }

        private int GetBaudrate()
        {
            switch (baudCombo.SelectedIndex)
            {
                case 0:
                    return 1200;
                case 1:
                    return 4800;
                default:
                    return 9600;
                case 3:
                    return 19200;
                case 4:
                    return 38400;
                case 5:
                    return 115200;
            }
        }

        private int GetDataBits()
        {
            switch (dataCombo.SelectedIndex)
            {
                case 0:
                    return 7;
                default:
                    return 8;
            }
        }

        private StopBits GetStopBits()
        {
            switch (stopCombo.SelectedIndex)
            {
                case 0:
                    return StopBits.One;
                default:
                    return StopBits.Two;
            }
        }

        private Parity GetParity()
        {
            switch (parityCombo.SelectedIndex)
            {
                default:
                    return Parity.None;
                case 1:
                    return Parity.Odd;
                case 2:
                    return Parity.Even;
            }
        }

        private string Now()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }

        private void Log(string message, bool highlight)
        {
            Dispatcher.Invoke(() =>
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Content = $"{Now()} - {message}";
                if (highlight)
                {
                    lvi.Foreground = Application.Current.TryFindResource("SecondaryHueMidForegroundBrush") as Brush;
                    lvi.Background = Application.Current.TryFindResource("SecondaryHueMidBrush") as Brush;
                }
                logBox.Items.Insert(0, lvi);
            });
        }

        private void SaveLog(string path)
        {
            StreamWriter sw;

            try
            {
                sw = new StreamWriter(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            for (int i = 0; i < logBox.Items.Count; i++)
            {
                ListViewItem lvi = (ListViewItem)logBox.Items[i];
                sw.WriteLine((string)lvi.Content);
            }

            sw.Close();
        }

        private void SaveDefault()
        {
            Settings.Default.Port = portCombo.SelectedValue as string;
            Settings.Default.Baudrate = baudCombo.SelectedIndex;
            Settings.Default.DataBit = dataCombo.SelectedIndex;
            Settings.Default.StopBit = stopCombo.SelectedIndex;
            Settings.Default.Parity = parityCombo.SelectedIndex;

            Settings.Default.Save();
        }
    }
}
