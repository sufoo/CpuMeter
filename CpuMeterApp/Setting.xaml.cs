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
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;

namespace CpuMeterApp
{
    public enum InfoTypeId
    {
        INFO_CPU_TOTAL = 0,
        INFO_CPU_REMAIN,
        INFO_MEM_TOTAL,
        INFO_INVALID,
        INFO_CPUEACH_START = 100
    }

    public class Parameters
    {
        public int[] MeterContentId = Enumerable.Repeat<int>(0, MainWindow.MAX_METER_NUM).ToArray();

        public int NumberOfRows { get; set; }
        public int UpdateMoveInterval { get; set; }
        public int UpdateInfoInterval { get; set; }
        public bool IsSmooth { get; set; }
        public Color BackColor { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool IsAlwaysOnTop { get; set; }

        public int GetUpdateInterval()
        {
            if (IsSmooth)
            {
                return UpdateMoveInterval;
            }
            else
            {
                return UpdateInfoInterval;
            }
        }
        public int GetMeterUpdatePerInfo()
        {
            if (IsSmooth)
            {
                return UpdateInfoInterval / UpdateMoveInterval;
            }
            else
            {
                return 1;
            }
        }
        public InfoTypeId GetInfoTypeAt(int i)
        {
            return (InfoTypeId)MeterContentId[i];
        }
        public void SetInitValue()
        {
            MeasureCpuEach mcpue = new MeasureCpuEach(0);
            string[] names = MeasureCpuEach.GetInstanceNames();
            int n = (names.Length < MainWindow.MAX_METER_NUM) ? names.Length : MainWindow.MAX_METER_NUM;
            NumberOfRows = (n % 2 == 0) ? n / 2 : (n / 2 + 1);

            for (int i = 0; i < n; i++ )
                MeterContentId[i] = i + (int)InfoTypeId.INFO_CPUEACH_START;
            for (int i = n; i < MainWindow.MAX_METER_NUM; i++ )
                MeterContentId[i] = 2;

            UpdateInfoInterval = 1000;
            UpdateMoveInterval = 150;
            IsSmooth = true;
            //BackColor = new Color() { R = 0xCC, G = 0x28, B = 0x28, A = 0xDD };
            BackColor = new Color() { R = 0xD7, G = 0x0A, B = 0x5A, A = 0xDC };
            WindowWidth = 250;
            WindowHeight = 520;
            IsAlwaysOnTop = true;
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Setting : Window
    {
        static public Parameters param = new Parameters();
        private ComboBox[] combos = new ComboBox[MainWindow.MAX_METER_NUM];
        ComboBoxItem[] new_list = null;

        public Setting()
        {
            InitializeComponent();
            combos[0] = comboBox0;
            combos[1] = comboBox1;
            combos[2] = comboBox2;
            combos[3] = comboBox3;
            combos[4] = comboBox4;
            combos[5] = comboBox5;
            combos[6] = comboBox6;
            combos[7] = comboBox7;
            combos[8] = comboBox8;
            combos[9] = comboBox9;
            combos[10] = comboBox10;
            combos[11] = comboBox11;
            combos[12] = comboBox12;
            combos[13] = comboBox13;
            combos[14] = comboBox14;
            combos[15] = comboBox15;
            combos[16] = comboBox16;
            combos[17] = comboBox17;
            combos[18] = comboBox18;
            combos[19] = comboBox19;
            combos[20] = comboBox20;
            combos[21] = comboBox21;
            combos[22] = comboBox22;
            combos[23] = comboBox23;
            combos[24] = comboBox24;
            combos[25] = comboBox25;

            CreateTypeComboItems();

            // set the previous settings
            for (int i = 0; i < MainWindow.MAX_METER_NUM; i++)
            {
                int id = (int)param.GetInfoTypeAt(i);
                combos[i].SelectedValue = id;
            }
            comboBoxRows.SelectedValue = param.NumberOfRows;
            comboBoxInterval.SelectedValue = param.UpdateInfoInterval;
            checkBoxSmooth.IsChecked = param.IsSmooth;
            ColorSample.Fill = new SolidColorBrush(param.BackColor);
        }

        private void CreateTypeComboItems()
        {
            ComboBoxItem[] old_list = Resources["DispInfoTypesItems"] as ComboBoxItem[];
            MeasureCpuEach mcpue = new MeasureCpuEach(0);
            string[] names = MeasureCpuEach.GetInstanceNames();
            new_list = new ComboBoxItem[names.Length + old_list.Length];
            for (int i = 0; i < old_list.Length; i++)
            {
                new_list[i] = old_list[i];
            }
            for (int i = 0; i < names.Length; i++)
            {
                new_list[old_list.Length + i] = new ComboBoxItem()
                {
                    Content = "CPU " + names[i],
                    Tag = (i + (int)InfoTypeId.INFO_CPUEACH_START).ToString()
                };
            }
            foreach (var x in combos)
            {
                x.ItemsSource = new_list;
            }
        }

        private void OnSelectedRowNum(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = e.Source as ListBoxItem;

            if (lbi != null)
            {
                string text = lbi.Tag as string;
                int n = int.Parse(text);
                for (int i = 0; i < n; i++)
                {
                    if (combos[2 * i] != null)
                    {
                        combos[2 * i].IsEnabled = true;
                        combos[2 * i + 1].IsEnabled = true;
                    }
                }
                for (int i = n; i < (MainWindow.MAX_METER_NUM / 2); i++)
                {
                    if (combos[2 * i] != null)
                    {
                        combos[2 * i].IsEnabled = false;
                        combos[2 * i + 1].IsEnabled = false;
                    }
                }
            }
            foreach (var x in combos)
            {
                x.ItemsSource = new_list;
            }
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MainWindow.MAX_METER_NUM; i++ )
                param.MeterContentId[i] = int.Parse(combos[i].SelectedValue.ToString());

            param.NumberOfRows = int.Parse(comboBoxRows.SelectedValue.ToString());
            param.UpdateInfoInterval = int.Parse(comboBoxInterval.SelectedValue.ToString());
            param.IsSmooth = (bool)checkBoxSmooth.IsChecked;

            e.Handled = true;

            DialogResult = true;
        }

        private void ButtonColorChangeClicked(object sender, RoutedEventArgs e)
        {
            ColorPickerControl.ColorPickerDialog cPicker = new ColorPickerControl.ColorPickerDialog();
            cPicker.StartingColor = param.BackColor;
            cPicker.Owner = this;

            bool? dialogResult = cPicker.ShowDialog();
            if (dialogResult != null && (bool)dialogResult == true)
            {
                param.BackColor = cPicker.SelectedColor;
                ColorSample.Fill = new SolidColorBrush(param.BackColor);
            }

            e.Handled = true;
        }

        //設定をロードする
        public static void Load()
        {
            //ユーザ毎のアプリケーションデータディレクトリに保存する
            String appPath = String.Format(
                "{0}\\{1}", 
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CpuMeterApp\\settings.xml");

            if (File.Exists(appPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Parameters));

                using (FileStream stream = new FileStream(appPath, FileMode.Open))
                {
                    Parameters temp = serializer.Deserialize(stream) as Parameters;
                    if (temp == null)
                        param.SetInitValue();
                    else
                        param = (Parameters)temp;
                }
            }
            else
            {
                String folderPath = String.Format(
                    "{0}\\{1}",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CpuMeterApp");
                System.IO.Directory.CreateDirectory(folderPath);
                param.SetInitValue();
            }
        }

        //設定を保存する
        public static void Save()
        {
            String appPath = String.Format(
                "{0}\\{1}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CpuMeterApp\\settings.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(Parameters));

            using (FileStream stream = new FileStream(appPath, FileMode.Create))
            {
                serializer.Serialize(stream, param);
            }
        }
    }
}
