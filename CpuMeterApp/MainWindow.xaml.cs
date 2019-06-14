using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;   // for PerformanceCounter
using System.Windows.Threading; // for DispatcherTimer
using System.Runtime.InteropServices; // for DLL import

namespace CpuMeterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int MAX_METER_NUM = 26;
        private int num_rows = 5;
        private BoeingMeter[] meters = new BoeingMeter[MAX_METER_NUM];
        private MeterControl[] meter_controls = new MeterControl[MAX_METER_NUM];
        private RowDefinition[] rows = new RowDefinition[MAX_METER_NUM];
        private Label[] labels = new Label[MAX_METER_NUM / 2];
        public MainWindow()
        {
            InitializeComponent();

            meters[0] = Meter0;
            meters[1] = Meter1;
            meters[2] = Meter2;
            meters[3] = Meter3;
            meters[4] = Meter4;
            meters[5] = Meter5;
            meters[6] = Meter6;
            meters[7] = Meter7;
            meters[8] = Meter8;
            meters[9] = Meter9;
            meters[10] = Meter10;
            meters[11] = Meter11;
            meters[12] = Meter12;
            meters[13] = Meter13;
            meters[14] = Meter14;
            meters[15] = Meter15;
            meters[16] = Meter16;
            meters[17] = Meter17;
            meters[18] = Meter18;
            meters[19] = Meter19;
            meters[20] = Meter20;
            meters[21] = Meter21;
            meters[22] = Meter22;
            meters[23] = Meter23;
            meters[24] = Meter24;
            meters[25] = Meter25;

            labels[0] = label0;
            labels[1] = label1;
            labels[2] = label2;
            labels[3] = label3;
            labels[4] = label4;
            labels[5] = label5;
            labels[6] = label6;
            labels[7] = label7;
            labels[8] = label8;
            labels[9] = label9;
            labels[10] = label10;
            labels[11] = label11;
            labels[12] = label12;

            for (int i = 0; i < MainPanel.RowDefinitions.Count(); i++)
            {
                rows[i] = MainPanel.RowDefinitions.ElementAt(i);
            }

            // Load settings and initialize
            Setting.Load();
            UpdateDisplayLayout();
            Width = Setting.param.WindowWidth;
            Height = Setting.param.WindowHeight;
            MenuAlwaysOnTop.IsChecked = Setting.param.IsAlwaysOnTop;
            Topmost = Setting.param.IsAlwaysOnTop;
            ShowInTaskbar = !Setting.param.IsAlwaysOnTop;

            Loaded += Window_Loaded;
            Closing += Window_Closing;
        }
        private void MenuSettingClicked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            Setting dialog = new Setting();
            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                UpdateDisplayLayout();
            }
            timer.Start();
        }

        private void UpdateDisplayLayout()
        {
            MainPanel.Background = new SolidColorBrush(Setting.param.BackColor);
            timer_count_max = Setting.param.GetMeterUpdatePerInfo();

            for (int i = 0; i < MAX_METER_NUM; i++)
            {
                MeasureObj obj;
                InfoTypeId id = Setting.param.GetInfoTypeAt(i);
                switch (id)
                {
                    case InfoTypeId.INFO_CPU_TOTAL:
                        obj = new MeasureCpuTotal(false);
                        break;
                    case InfoTypeId.INFO_CPU_REMAIN:
                        obj = new MeasureCpuTotal(true);
                        break;
                    case InfoTypeId.INFO_MEM_TOTAL:
                        obj = new MeasureMemory();
                        break;
                    default:
                        if (id >= InfoTypeId.INFO_CPUEACH_START)
                        {
                            obj = new MeasureCpuEach((int)id - (int)InfoTypeId.INFO_CPUEACH_START);
                            break;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                }

                meter_controls[i] = new MeterControl()
                {
                    Meter = meters[i],
                    MeasureObj = obj,
                    MaxTimeIndex = timer_count_max
                };
            }

            if (timer != null)
            {
                timer.Interval = TimeSpan.FromMilliseconds(Setting.param.GetUpdateInterval()); 
            }
            SetNumRows(Setting.param.NumberOfRows);

            for (int i = 0; i < Setting.param.NumberOfRows; i++)
            {
                labels[i].Content = GetTitleText(Setting.param.GetInfoTypeAt(2 * i), Setting.param.GetInfoTypeAt(2 * i + 1));
            }
        }

        private void SetNumRows(int nRows)
        {
            MainPanel.RowDefinitions.Clear();
            for (int i = 0; i < 2 * nRows; i++)
            {
                MainPanel.RowDefinitions.Add(rows[i]);
            }
            for (int i = 2 * num_rows; i < 2 * nRows; i++)
            {
                if (meter_controls[i] != null)
                    meter_controls[i].IsEnable = true;
            }
            for (int i = num_rows; i < nRows; i++)
            {
                labels[i].Visibility = Visibility.Visible;
            }
            for (int i = 2 * nRows; i < MAX_METER_NUM; i++)
            {
                if (meter_controls[i] != null)
                    meter_controls[i].IsEnable = false;
            }
            for (int i = nRows; i < (MAX_METER_NUM / 2); i++)
            {
                labels[i].Visibility = Visibility.Hidden;
            }
            num_rows = nRows;
        }

        private string GetTitleText(InfoTypeId id_left, InfoTypeId id_right)
        {
            string cpu_left = GetCpuDataText(id_left);
            string cpu_right = GetCpuDataText(id_right);
            if (cpu_left != null && cpu_right != null)
            {
                return "CPU " + cpu_left + " / " + cpu_right;
            }
            string mem_left = GetMemDataText(id_left);
            string mem_right = GetMemDataText(id_right);
            if (mem_left != null && mem_right != null)
            {
                return "MEM " + mem_left + " / " + mem_right;
            }
            if (cpu_left != null || cpu_right != null)
            {
                if (cpu_left != null) cpu_left = "CPU " + cpu_left;
                if (cpu_right != null) cpu_right = "CPU " + cpu_right;
            }
            if (mem_left != null || mem_right != null)
            {
                if (mem_left != null) mem_left = "MEM " + mem_left;
                if (mem_right != null) mem_right = "MEM " + mem_right;
            }
            if (cpu_left == null)
            {
                return mem_left + " / " + cpu_right;
            }
            else
            {
                return cpu_left + " / " + mem_right;
            }            
        }

        private string GetCpuDataText(InfoTypeId id)
        {
            if (id == InfoTypeId.INFO_CPU_TOTAL || id == InfoTypeId.INFO_CPU_REMAIN
                || id >= InfoTypeId.INFO_CPUEACH_START)
            {
                switch (id)
                {
                    case InfoTypeId.INFO_CPU_REMAIN:
                        return "Available";
                    case InfoTypeId.INFO_CPU_TOTAL:
                        return "Total";
                    default:
                        if (id >= InfoTypeId.INFO_CPUEACH_START)
                        {
                            string[] instance_names = MeasureCpuEach.GetInstanceNames();
                            return instance_names[(int)id - (int)InfoTypeId.INFO_CPUEACH_START];
                        }
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private string GetMemDataText(InfoTypeId id)
        {
            if (id == InfoTypeId.INFO_MEM_TOTAL)
            {
                return "Total";
            }
            else
            {
                return null;
            }
        }

        private DispatcherTimer timer = null;
        private int timer_count = 0;
        private int timer_count_max = 1;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            timer.Interval = TimeSpan.FromMilliseconds(Setting.param.GetUpdateInterval());
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            MeasureCpuTotal.Dispose();

            timer = null;
            meter_controls = null;

            // Save settings
            Setting.param.WindowWidth = Width;
            Setting.param.WindowHeight = Height;
            Setting.Save();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (meter_controls == null) return;

            if (Setting.param.IsSmooth)
            {
                if (timer_count == 0)
                {
                    MeasureCpuTotal.Update();
                    MeasureCpuEach.Update();
                    MeasureMemory.Update();
                    foreach (var x in meter_controls)
                    {
                        x.UpdateData();
                    }
                }
                foreach (var x in meter_controls)
                {
                    x.UpdateSmooth(timer_count);
                }
                timer_count++;
                if (timer_count == timer_count_max) timer_count = 0;
            }
            else
            {
                MeasureCpuTotal.Update();
                MeasureCpuEach.Update();
                MeasureMemory.Update();
                foreach (var x in meter_controls)
                {
                    x.UpdateData();
                    x.UpdateDirect();
                }
            }
        }
        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MenuAlwaysOnTopClicked(object sender, RoutedEventArgs e)
        {
            MenuAlwaysOnTop.IsChecked = !MenuAlwaysOnTop.IsChecked;
            Topmost = MenuAlwaysOnTop.IsChecked;
            Setting.param.IsAlwaysOnTop = MenuAlwaysOnTop.IsChecked;
            ShowInTaskbar = !Setting.param.IsAlwaysOnTop;
        }
        private void MenuAboutClicked(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            Nullable<bool> dialogResult = dialog.ShowDialog();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            try
            {
                DragMove();
            }
            catch { }
        }
    }

    public class MeterControl
    {
        public BoeingMeter Meter { get; set; }
        public MeasureObj MeasureObj
        {
            get { return measurement; }
            set
            {
                measurement = value;
                Initialize();
            }
        }
        public int MaxTimeIndex { 
            set{ delta_x = 1.0f / value; }
        }
        public bool IsEnable
        {
            set
            {
                is_enable = value;
                if (is_enable)
                {
                    Meter.Visibility = Visibility.Visible;
                    Meter.IsEnabled = true;
                }
                else
                {
                    Meter.Visibility = Visibility.Hidden;
                    Meter.IsEnabled = false;
                    measurement = null; // release the resources
                }
            }
            get { return is_enable; }
        }
        private bool is_enable = true;
        private MeasureObj measurement = null;
        private float prev_value;
        private float next_value;
        private float delta_y;
        private float delta_x;
        private float x;

        private void Initialize()
        {
            if (measurement != null)
            {
                measurement.InitializeMeterFormat(Meter);
                Meter.MeterMax = measurement.GetMax();
                prev_value = next_value = measurement.GetValue();
            }
        }

        public void UpdateData()
        {
            if (measurement != null)
            {
                prev_value = next_value;
                next_value = measurement.GetValue();
                delta_y = next_value - prev_value;
            }
        }

        public void UpdateSmooth(int time_index)
        {
            if (measurement != null)
            {
                if (time_index == 0)
                {
                    Meter.MeterValue = prev_value;
                    x = 0;
                }
                else
                {
                    float z = 1.0f - x;
                    float y = 1.0f - z * z;
                    Meter.MeterValue = prev_value + delta_y * y;
                }
                x += delta_x;
            }
        }

        public void UpdateDirect()
        {
            if (measurement != null)
            {
                Meter.MeterValue = next_value;
            }
        }
    }

    public abstract class MeasureObj
    {
        public abstract void InitializeMeterFormat(BoeingMeter meter);
        public abstract float GetMax();
        public virtual float GetMin() { return 0; }
        public abstract float GetValue();
    }

    public class MeasureCpuTotal : MeasureObj
    {
        protected static PerformanceCounter pc = null;
        protected static float value = 0;
        protected static int user_count = 0;
        protected bool is_remain;

        public MeasureCpuTotal(bool IsRemain)
        {
            is_remain = IsRemain;
            if (user_count == 0) Allocate();
            user_count++;
        }
        ~MeasureCpuTotal()
        {
            user_count--;
            if (user_count == 0) Dispose();
        }
        public override void InitializeMeterFormat(BoeingMeter meter)
        {
            meter.ValueFormat = "{0:0}%";
        }
        public override float GetMax()
        {
            return 100;
        }
        public override float GetValue()
        {
            return is_remain ? (100 - value) : value;
        }
        public static void Update()
        {
            if (pc != null)
            {
                value = pc.NextValue();
                if (value < 0) value = 0;
                if (value > 100.0f) value = 100.0f;
            }
        }
        protected static void Allocate()
        {
            if (pc != null) return;
            pc = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        }
        public static void Dispose()
        {
            if (pc == null) return;
            pc.Dispose();
            pc = null;
        }
    }

    public class MeasureCpuEach : MeasureObj
    {
        protected static PerformanceCounter pc = null;
        protected static int user_count = 0;

        protected static string[] instance_names;
        protected static CounterSample[] counters;
        protected static float[] values;

        protected int index;
        public static string[] GetInstanceNames() { return instance_names; }

        public MeasureCpuEach(int id)
        {
            index = id;
            if (user_count == 0) Allocate();
            user_count++;
        }
        ~MeasureCpuEach()
        {
            user_count--;
            if (user_count == 0) Dispose();
        }
        public override void InitializeMeterFormat(BoeingMeter meter)
        {
            meter.ValueFormat = "{0:0}%";
        }
        public override float GetMax()
        {
            return 100;
        }
        public override float GetValue()
        {
            return values[index];
        }
        public static void Update()
        {
            if (pc != null)
            {
                int n = instance_names.Length;
                for (int i = 0; i < n; i++)
                {
                    pc.InstanceName = instance_names[i];
                    CounterSample newSample = pc.NextSample();
                    CounterSample oldSample = counters[i];
                    float difference = newSample.RawValue - oldSample.RawValue;
                    float timeInterval = newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec;
                    if (timeInterval != 0)
                    {
                        float val = 100.0f * (1.0f - (difference / timeInterval));
                        if(val < 0) val = 0;
                        if(val > 100.0f) val = 100.0f;
                        values[i] = val;
                    }
                    counters[i] = newSample;
                }
            }
        }
        protected static void Allocate()
        {
            if (pc != null) return;
            pc = new PerformanceCounter("Processor", "% Processor Time", true);
            var cat = new PerformanceCounterCategory("Processor");
            instance_names = cat.GetInstanceNames();
            int n = instance_names.Length;
            counters = new CounterSample[n];
            for (int i = 0; i < n; i++)
            {
                pc.InstanceName = instance_names[i];
                counters[i] = pc.NextSample();
            }
            values = new float[n];
        }
        public static void Dispose()
        {
            if (pc == null) return;
            pc.Dispose();
            pc = null;
        }
    }

    public class MeasureMemory : MeasureObj
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                //this.dwLength = (uint)Marshal.SizeOf(typeof(NativeMethods.MEMORYSTATUSEX));
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        // ------------------------------
        protected static PerformanceCounter pc = null;
        protected static float mem_size_total = 0;
        protected static float mem_size_free = 0;
        protected static MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
        private const int scale = 1024 * 1024 * 1024;

        public MeasureMemory()
        {
            if (GlobalMemoryStatusEx(memStatus))
            {
                mem_size_total = (float)memStatus.ullTotalPhys / scale;
            }
        }
        ~MeasureMemory()
        {
        }
        public override void InitializeMeterFormat(BoeingMeter meter)
        {
            meter.ValueFormat = "{0:F1}G";
        }
        public override float GetMax()
        {
            return mem_size_total;
        }
        public override float GetValue()
        {
            return mem_size_total - mem_size_free;
        }
        public static void Update()
        {
            if (GlobalMemoryStatusEx(memStatus))
            {
                mem_size_free = (float)memStatus.ullAvailPhys / scale;
            }
        }
    }
}
