using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace upper_computer
{
    public struct Gas
    {
        public string name;
        public float range;
        public string unit;
        public float low_level_alarm;
        public float high_level_alarm;
    }
    public partial class Form1 : Form
    {
        private long receive_count = 0; //接收字节计数, 作用相当于全局变量
        private long send_count = 0;    //发送字节计数
        private DateTime current_time = new DateTime();
        private string[] date_time = new string[2]; //文件中读取的日期+时间
        private float[] gas = new float[6] {0, 0, 0, 0, 0, 0}; //储存一组气体数值
        DataTable dt = new DataTable("tableText"); //该Form中的数据表
        private int rownumber = 0; //读取的数据行数
        private DateTime startDate = new DateTime(); //DateTime类型的起始时间，用于后续的form的数据处理
        private DateTime endDate = new DateTime(); //DateTime类型的结束时间
        private string[] startDateTime = new string[2]; //用于筛选的开始时间，数组形式存储日期+时间
        private string[] endDateTime= new string[2]; //用于筛选的结束时间，数组形式存储日期+时间
        private DataTable dateDt = new DataTable(); //日期datatable，日期+时间仅占一列，方便数据传到图表
        private DataTable currentDt = new DataTable(); //当前数据，传到统计图处理的Form中
        private int gas_number = 0; //气体数量
        private Gas[] gasSet = new Gas[6]; //储存气体信息的结构体的数组
        private string filename = ""; //打开文件名

        private DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
        private StringBuilder sb = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //单个添加
            for (int i = 300; i <= 38400; i = i*2)
            {
                comboBox2.Items.Add(i.ToString());  //添加波特率列表
            }
            //批量添加波特率列表
            string[] baud = { "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            comboBox2.Items.AddRange(baud);

            //获取电脑当前可用串口并添加到选项列表中
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            //设置默认值
            //comboBox1.Text = "COM1";
            comboBox2.Text = "9600";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";

            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 9);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if(serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    button1.Text = "打开串口";
                    button1.BackColor = Color.ForestGreen;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    textBox1.Text = "";
                    textBox2.Text = "";
                    label6.Text = "串口已关闭";
                    label6.ForeColor = Color.Red;
                    button2.Enabled = false;
                }
                else
                {
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("NONE"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if(comboBox4.Text.Equals("ODD"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if(comboBox4.Text.Equals("EVEN"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;

                    serialPort1.Open();
                    button1.Text = "关闭串口";
                    button1.BackColor = Color.Firebrick;
                    label6.Text = "串口已打开";
                    label6.ForeColor = Color.Green;
                    button2.Enabled = true;
                    checkBox3.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                serialPort1 = new System.IO.Ports.SerialPort();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                checkBox2.Enabled = false;
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int num = serialPort1.BytesToRead;
            byte[] received_buf = new byte[num];

            receive_count += num;
            serialPort1.Read(received_buf, 0, num);
            sb.Clear();

            if(radioButton2.Checked)
            {
                foreach (byte b in received_buf)
                {
                    sb.Append(b.ToString("X2")+ "");
                }
            }
            else
            {
                sb.Append(Encoding.ASCII.GetString(received_buf));
            }
            
            try
            {
                this.Invoke((EventHandler)delegate
                {
                    if(checkBox1.Checked)
                    {
                        current_time = System.DateTime.Now;
                        textBox1.AppendText(current_time.ToString("HH:mm:ss") + " " + sb.ToString());
                    }
                    else
                    {
                        textBox1.AppendText(sb.ToString());
                    }
                    
                    label7.Text = "Rx:" + receive_count.ToString() + "Bytes";
                }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            receive_count = 0;
            send_count = 0;
            label7.Text = "Rx:" + receive_count.ToString() + "Bytes";
            label8.Text = "Tx:" + send_count.ToString() + "Bytes";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] temp = new byte[1];
            try
            {
                if(serialPort1.IsOpen)
                {
                    int num = 0;

                    if (radioButton4.Checked)
                    {
                        //以HEX模式发送
                        string buf = textBox2.Text;
                        string pattern = @"\s";
                        string replacement = "";
                        Regex rgx = new Regex(pattern);
                        string send_data = rgx.Replace(buf, replacement);

                        num = (send_data.Length - send_data.Length % 2) / 2;
                        for(int i = 0; i < num; i++)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(i * 2, 2), 16);
                            serialPort1.Write(temp, 0, 1);
                        }

                        if (send_data.Length % 2 != 0)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(textBox2.Text.Length - 1, 1), 16);
                            serialPort1.Write(temp, 0, 1);
                            num++;
                        }

                        if (checkBox2.Checked)
                        {
                            serialPort1.WriteLine("");
                        }
                    }
                    else
                    {
                        //以ASCⅡ模式发送
                        if (checkBox2.Checked)
                        {
                            serialPort1.WriteLine(textBox2.Text);
                            num = textBox2.Text.Length + 2;
                        }
                        else
                        {
                            serialPort1.Write(textBox2.Text);
                            num = textBox2.Text.Length;
                        }
                    }
                    send_count += num;
                    label8.Text = "Tx:" + send_count.ToString() + "Bytes";
                }
            }
            catch (Exception ex)
            {
                serialPort1.Close();
                serialPort1 = new System.IO.Ports.SerialPort();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                //自动发送功能选中
                numericUpDown1.Enabled = false;
                timer1.Interval = (int)numericUpDown1.Value;
                timer1.Start();
                label6.Text = "串口已打开 自动发送中...";
            }
            else
            {
                //自动发送功能未选中
                numericUpDown1.Enabled = true;
                timer1.Stop();
                label6.Text = "串口已打开";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //定时时间到
            button2_Click(button2, new EventArgs());
        }

        //导入文件       
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime datetime = new DateTime();
                rownumber = 0;
                openFileDialog1.Filter = "*.txt|*.txt|所有文件|*.*";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    dt.Rows.Clear();
                    dateDt.Rows.Clear();
                    dataGridView1.DataSource = null;
                    comboBox6.Items.Clear();
                    comboBox6.Items.Add("all");
                    comboBox6.Text = "all";
                    filename = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                    StreamReader sr = new StreamReader(filename);
                    string string1 = "";

                    //读取文件头，对气体的属性进行初始化，存储在gasSet中
                    for (int i = 0; i < 6; i++)
                    {
                        string1 = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(string1))
                            break;
                        else
                        {
                            string trim = Regex.Replace(string1.Trim(), "\\s{2,}", " ");
                            string[] result = trim.Split();
                            gasSet[i] = new Gas
                            {
                                name = result[0],
                                range = Convert.ToSingle(result[1]),
                                unit = result[2],
                                low_level_alarm = Convert.ToSingle(result[3]),
                                high_level_alarm = Convert.ToSingle(result[4])
                            };
                        }
                    }

                    while ((string1 = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(string1))
                            continue;
                        rownumber++;
                        string trim = Regex.Replace(string1.Trim(), "\\s{2,}", " ");
                        string[] result = trim.Split();

                        //当第一次读到数据时，获取气体数量，进行数据表初始化
                        if (rownumber == 1)
                        {
                            gas_number = result.Length - 2;
                            initDataSet(gas_number);
                            initDateDt(gas_number);
                            currentDt = dateDt.Clone();
                            for (int k = 0; k < gas_number; k++)
                            {
                                comboBox6.Items.Add(gasSet[k].name);
                            }
                        }

                        //得到一行数据处理date_time[] + gas[]
                        for (int j = 0; j < result.Length; j++)
                        {
                            if (j >= 2) //气体浓度
                            {
                                float fresult = Convert.ToSingle(result[j]);

                                //超出量程按照量程显示
                                if (fresult == 3999)
                                    fresult = gasSet[j - 2].range;
                                gas[j - 2] = fresult;
                            }
                            else if (j == 0) //日期
                            {
                                date_time[j] = "20" + result[j].Insert(2, "-");
                                date_time[j] = date_time[j].Insert(7, "-");
                            }
                            else //时间
                            {
                                date_time[j] = result[j].Insert(2, ":");
                                date_time[j] = date_time[j].Insert(5, ":");
                            }
                            string s = date_time[0] + " " + date_time[1];
                            datetime = sToDate(s);
                        }

                        //在此加入一行数据
                        addDataSet(date_time, gas, rownumber);
                        addDateDt(datetime, gas, rownumber);

                        if (rownumber == 1) //获取起始时间
                        {
                            string s = date_time[0] + " " + date_time[1];
                            startDate = sToDate(s);
                            dateTimePicker2.Value = startDate;
                        }
                    }
                    sr.Close();
                    currentDt = dateDt;

                    //获取终止时间
                    string str = date_time[0] + " " + date_time[1];
                    endDate = sToDate(str);
                    dateTimePicker1.Value = endDate;

                    dataGridView1.DataSource = dataSet1.Tables[0];
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    dataGridView1.Rows[0].Selected = false;

                    button6.Enabled = true;
                    button7.Enabled = true;
                    button8.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("导入出错");
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
            }    
        }

        //初始化DataSet，加入dt表并初始化结构
        private void initDataSet(int number)
        {
            dataSet1.Tables.Clear();
            dataSet1.Tables.Add(dt);
            dt.Columns.Clear();
            dt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32"), ""));
            dt.Columns.Add(new DataColumn("Date", Type.GetType("System.DateTime"), ""));
            dt.Columns.Add(new DataColumn("Time", Type.GetType("System.String"), ""));
            for (int i = 0; i < number; i++)
            {
                dt.Columns.Add(new DataColumn(gasSet[i].name, Type.GetType("System.Decimal"), ""));
            }
        }

        //初始化DateDt表结构
        private void initDateDt(int number)
        {
            dateDt.Columns.Clear();
            dateDt.Columns.Add(new DataColumn("ID", Type.GetType("System.Int32"), ""));
            dateDt.Columns.Add(new DataColumn("Date", Type.GetType("System.DateTime"), ""));
            for (int i = 0; i < number; i++)
            {
                dateDt.Columns.Add(new DataColumn(gasSet[i].name, Type.GetType("System.Decimal"), ""));
            }
        }

        //向DateSet里添加一行数据，该数据表用于this Form
        private void addDataSet(string[] s, float[] f, int row)
        {
            DataRow dr = dt.NewRow();
            dr["ID"] = row;
            dr["Date"] = s[0];
            dr["Time"] = s[1];
            for(int i = 0; i < gas_number; i++)
            {
                dr[gasSet[i].name] = f[i];
            }
            dt.Rows.Add(dr);
            dataSet1.AcceptChanges();
        }

        //向DateDt里添加一行数据, 该数据表用于后续其他Form
        private void addDateDt(DateTime dt, float[] f, int row)
        {
            DataRow dr = dateDt.NewRow();
            dr["ID"] = row;
            dr["Date"] = dt;
            for (int i = 0; i < gas_number; i++)
            {
                dr[gasSet[i].name] = f[i];
            }
            dateDt.Rows.Add(dr);
        }

        //筛选按钮
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                button5.Enabled = true; //启用复位

                //选择的筛选起始时间和结束时间
                string end = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string start = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string[] endarr = end.Split(' ');
                string[] startarr = start.Split(' ');
                startDateTime[0] = startarr[0];
                startDateTime[1] = startarr[1];
                endDateTime[0] = endarr[0];
                endDateTime[1] = endarr[1];

                //当前显示页的数据表筛选
                if (sToDate(end) < sToDate(start))
                    selectGas(dt.Clone());
                else if (startDateTime[0].Equals(endDateTime[0])) //如果起止日期相同
                {
                    DataRow[] drArr = dt.Select("Time>='" + startDateTime[1] + "' and Time<='" + endDateTime[1] + "' and Date='" + startDateTime[0] + "'"); //日期相同，主要只用考虑起止时间，并且日期要匹配！

                    //把一个表的数据复制到另一个表
                    DataTable dtNew = dt.Clone();
                    for (int i = 0; i < drArr.Length; i++)
                    {
                        dtNew.ImportRow(drArr[i]);
                    }

                    dtNew.DefaultView.Sort = "ID ASC";
                    dtNew = dtNew.DefaultView.ToTable();
                    selectGas(dtNew);
                }
                else //如果起止日期不同
                {
                    DataRow[] drArr0 = dt.Select("Date>'" + startDateTime[0] + "' and Date<'" + endDateTime[0] + "'"); // 夹在左右日期之间的日期（不包含）
                    DataRow[] drArr1 = dt.Select("Date='" + startDateTime[0] + "' and Time>='" + startDateTime[1] + "'"); //左边界日期，当天时间需大于起始时间
                    DataRow[] drArr2 = dt.Select("Date='" + endDateTime[0] + "' and Time<='" + endDateTime[1] + "'"); //右边界日期，当天时间需小于结束时间   

                    //把一个表的数据复制到另一个表
                    DataTable dtNew = dt.Clone();
                    for (int i = 0; i < drArr0.Length; i++)
                    {
                        dtNew.ImportRow(drArr0[i]);
                    }
                    for (int i = 0; i < drArr1.Length; i++)
                    {
                        dtNew.ImportRow(drArr1[i]);
                    }
                    for (int i = 0; i < drArr2.Length; i++)
                    {
                        dtNew.ImportRow(drArr2[i]);
                    }

                    dtNew.DefaultView.Sort = "ID ASC";
                    dtNew = dtNew.DefaultView.ToTable();
                    selectGas(dtNew);
                }

                //要传输到图表的数据表的筛选
                DataRow[] dr = dateDt.Select("Date>='" + sToDate(start) + "' and Date<='" + sToDate(end) + "'");

                //把一个表的数据复制到另一个表
                DataTable dtNew1 = dateDt.Clone();
                for (int i = 0; i < dr.Length; i++)
                {
                    dtNew1.ImportRow(dr[i]);
                }
                dtNew1.DefaultView.Sort = "ID ASC";
                dtNew1 = dtNew1.DefaultView.ToTable();
                selectGasDt(dtNew1);
            }
            catch(Exception ex)
            {
                MessageBox.Show("输入异常");
            }    
        }

        //传入日期筛选后的datatable，进行气体筛选，将结果复制到新表中绑定到dataGridView
        private void selectGas(DataTable datatable)
        {
            float up = (float)numericUpDown2.Value;
            float down = (float)numericUpDown3.Value;
            string gas = comboBox6.Text.ToString();

            if (gas.Equals("all")) //全选，此时直接将数据绑定即可
            {
                dataGridView1.DataSource = datatable;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            else //选择某气体，需要先筛选再绑定数据
            {
                DataRow[] drArr = datatable.Select(gas + ">=" + down + "and " + gas + "<=" + up);

                //把一个表的数据复制到另一个表
                DataTable dtNew = datatable.Clone();
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtNew.ImportRow(drArr[i]);
                }
                dtNew.DefaultView.Sort = "ID ASC";
                dtNew = dtNew.DefaultView.ToTable();
                dataGridView1.DataSource = dtNew;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }  
        }

        //传入日期筛选后的datatable，进行气体筛选，将结果复制到currentDt表中
        private void selectGasDt(DataTable datatable)
        {
            float up = (float)numericUpDown2.Value;
            float down = (float)numericUpDown3.Value;
            string gas = comboBox6.Text.ToString();

            if (!gas.Equals("all"))
            {
                DataRow[] drArr = datatable.Select(gas + ">=" + down + "and " + gas + "<=" + up);
                
                //把一个表的数据复制到另一个表
                DataTable dtNew = datatable.Clone();
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtNew.ImportRow(drArr[i]);
                }
                dtNew.DefaultView.Sort = "ID ASC";
                dtNew = dtNew.DefaultView.ToTable();
                currentDt = dtNew;
            }
            else
            {
                currentDt = datatable;
            }
        }

        //当选择气体为all时，取消气体上限下限；当选中某气体时，用高低报当作默认上下限
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox6.Text.Equals("all"))
            {
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
            }
            else
            {
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;

                switch (comboBox6.SelectedIndex)
                {
                    case 1:
                        numericUpDown2.Value = (decimal)gasSet[0].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[0].low_level_alarm;
                        break;
                    case 2:
                        numericUpDown2.Value = (decimal)gasSet[1].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[1].low_level_alarm;
                        break;
                    case 3:
                        numericUpDown2.Value = (decimal)gasSet[2].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[2].low_level_alarm;
                        break;
                    case 4:
                        numericUpDown2.Value = (decimal)gasSet[3].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[3].low_level_alarm;
                        break;
                    case 5:
                        numericUpDown2.Value = (decimal)gasSet[4].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[4].low_level_alarm;
                        break;
                    case 6:
                        numericUpDown2.Value = (decimal)gasSet[5].high_level_alarm;
                        numericUpDown3.Value = (decimal)gasSet[5].low_level_alarm;
                        break;
                    default:
                        break;
                }
            }
        }

        //复位按钮
        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dt;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            currentDt = dateDt;
            dateTimePicker1.Value = endDate;
            dateTimePicker2.Value = startDate;
        }

        //图表显示按钮
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox6.SelectedIndex == 0) //选择了all
                {
                    Form2 f2 = new Form2(currentDt, gasSet);
                    f2.StartPosition = FormStartPosition.CenterScreen;
                    f2.Show();
                }
                else //选择了某项气体
                {
                    Form2 f2 = new Form2(currentDt, gasSet, comboBox6.SelectedIndex - 1);
                    f2.StartPosition = FormStartPosition.CenterScreen;
                    f2.Show();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }    
        }

        //字符串转DateTime类
        private DateTime sToDate(string s)
        {
            dtFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";
            DateTime dt = Convert.ToDateTime(s, dtFormat);
            return dt;
        }

        //报警分析按钮
        private void button8_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3(dateDt, gasSet, startDate, endDate);
            f3.StartPosition = FormStartPosition.CenterScreen;
            f3.Show();
        }
    }
}
