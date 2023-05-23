﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace L4_Oscilloscope
{
    public partial class Form1 : Form
    {

        private Thread[] dataThread;


        private DataStorage[] dataStorage;



        public Form1()
        {
            InitializeComponent();
            // Создание и настройка графиков
            CreateChart(chart1, "Channel 1");
            CreateChart(chart2, "Channel 2");
            CreateChart(chart3, "Channel 3");
            CreateChart(chart4, "Channel 4");

            dataStorage = new DataStorage[4];
            for (int j = 0; j < 4; j++)
            {
                dataStorage[j] = new DataStorage();
            }


            dataStorage[0].chart = chart1;
            dataStorage[1].chart = chart2;
            dataStorage[2].chart = chart3;
            dataStorage[3].chart = chart4;

            CreateDataGrid();
            Calibrate();
            dataThread = new Thread[4];
            // Запуск потоков для получения данных
            StartDataThread();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Остановка потоков и закрытие клиентов UDP при закрытии формы
            StopDataThread();
        }

        private void CreateChart(Chart chart, string seriesName)
        {
            // Настройка графика
            chart.Series.Clear();
            chart.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "Time";
            chartArea.AxisY.Title = "Value";
            chart.ChartAreas.Add(chartArea);

            Series series = new Series();
            series.Name = seriesName;
            series.ChartType = SeriesChartType.Line;
            chart.Series.Add(series);
        }

        private void StartDataThread()
        {
            for (int j = 0; j < 4; j++)
            {
                int i = j;
                dataThread[i] = new Thread(() => ReceiveData(dataGridView[1, i].Value.ToString(), int.Parse(dataGridView[2, i].Value.ToString()), dataStorage[i]));
                if ((bool)dataGridView[0, i].Value == true)
                {
                    dataThread[i].Start();
                }
            }

        }

        private void StopDataThread()
        {
            for (int i = 0; i < 4; i++)
            {
                if (dataThread[i] != null && dataThread[i].IsAlive)
                    dataThread[i].Abort();
            }

        }
        private void ReceiveData(string serverIP, int serverPort, DataStorage dataStorage)
        {
            IPAddress serverAddress = IPAddress.Parse(serverIP);
            UdpClient listener = new UdpClient();
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(new IPEndPoint(serverAddress, serverPort));
            IPEndPoint groupEP = new IPEndPoint(serverAddress, serverPort);

            try
            {
                while (true)
                {
                    // Получение данных
                    byte[] bytes = listener.Receive(ref groupEP);

                    // Преобразование данных из массива байтов в строку
                    string data = Encoding.UTF8.GetString(bytes);

                    // Обработка полученных данных
                    double value;
                    if (double.TryParse(data, out value))
                    {
                        Invoke(new UpdateDataStorageDelegate(UpdateDataStorage), new object[] { dataStorage, value });
                    }
                }
            }
            catch
            {
                listener.Close();
            }
            finally
            {
                listener.Close();
            }

        }
        public delegate void UpdateDataStorageDelegate(DataStorage dataStorage, double value);

        private void UpdateDataStorage(DataStorage dataStorage, double value)
        {
            dataStorage.AddData(value);
        }


        public class DataStorage
        {
            private List<double> data;
            private List<DateTime> timestamps;
            public Chart chart;
            public double min = -100, max = 100;

            public DataStorage()
            {
                data = new List<double>();
                timestamps = new List<DateTime>();
            }

            public void AddData(double value)
            {
                if (value > min && value < max)
                {
                    data.Add(value);
                    timestamps.Add(DateTime.Now);
                }

            }

            public void ClearData()
            {
                data.Clear();
                timestamps.Clear();
            }

            public void SetMinMax(double _min, double _max)
            {
                min = _min;
                max = _max;
            }
            public void DrawLast100Data(int N)
            {
                chart.Series[0].Points.Clear();
                int count = Math.Min(data.Count, N);
                for (int i = data.Count - count; i < data.Count; i++)
                {
                    chart.Series[0].Points.AddXY(timestamps[i], data[i]);
                }

            }
        }

        private void button_IP_Click(object sender, EventArgs e)
        {
            if (button_IP.Text == "Подключится")
            {
                StartDataThread();
                timer_Draw.Enabled = true;
                button_IP.Text = "Отключится";

            }
            else
            {
                timer_Draw.Enabled = false;

                StopDataThread();
                button_IP.Text = "Подключится";
            }
        }

        private void timer_Draw_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (int.TryParse(dataGridView[6, i].Value.ToString(), out int value))
                    dataStorage[i].DrawLast100Data(value);
            }
        }

        private void CreateDataGrid()
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();
            string[] HeaderNames = { "", "ip", "port", "Минимум", "Максимум", "Изменение", "Показ", "Шумоподавление" };

            foreach (var name in HeaderNames)
            {
                DataGridViewColumn Column = new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    HeaderText = name
                };

                dataGridView.Columns.Add(Column);
            }
            dataGridView.Columns[0].Width = 20;
            for (int i = 1; i < dataGridView.Columns.Count - 1; i++)
            {
                dataGridView.Columns[i].Width = 70;
            }
            dataGridView.Columns[dataGridView.Columns.Count - 1].Width = 140;
            for (int i = 1; i < 5; i++)
            {
                NewLine(i);
            }
        }
        private void NewLine(int i)
        {
            var Row = new DataGridViewRow();
            var CheckCell = new DataGridViewCheckBoxCell();
            CheckCell.Value = true;
            var IpCell = new DataGridViewTextBoxCell();
            IpCell.Value = "127.0.0.1";
            var portCell = new DataGridViewTextBoxCell();
            portCell.Value = "100" + i;
            var MinCell = new DataGridViewTextBoxCell();
            MinCell.Value = 0;
            var MaxCell = new DataGridViewTextBoxCell();
            MaxCell.Value = 255;
            var DeltaCell = new DataGridViewTextBoxCell();
            DeltaCell.Value = 0;
            var VievCell = new DataGridViewTextBoxCell();
            VievCell.Value = 1000;
            var NoiseCell = new DataGridViewComboBoxCell();
            NoiseCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            string[] STREAMS = new string[] { "Нет", "Выборочное средне", "Скользящее среднее", "Медианный фильтр" };
            foreach (var s in STREAMS)
                NoiseCell.Items.Add(s);
            NoiseCell.Value = NoiseCell.Items[0];

            Row.Cells.AddRange(CheckCell, IpCell, portCell, MinCell, MaxCell, DeltaCell, VievCell, NoiseCell);
            dataGridView.Rows.Add(Row);
        }

        private void dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {

        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Calibrate();
        }
        private void Calibrate()
        {
            for (int i = 0; i < 4; i++)
            {
                dataStorage[i].SetMinMax(double.Parse(dataGridView[3, i].Value.ToString()), double.Parse(dataGridView[4, i].Value.ToString()));
            }
        }
    }
}

