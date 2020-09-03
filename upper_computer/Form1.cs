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

namespace upper_computer
{
    public partial class Form1 : Form
    {
        private long receive_count = 0; //接收字节计数, 作用相当于全局变量
        private long send_count = 0;    //发送字节计数
        private StringBuilder sb = new StringBuilder();
        private DateTime current_time = new DateTime();
        private string filename = "";
        private string[] date_time = new string[2];
        private float[] gas = new float[6] {0, 0, 0, 0, 0, 0};
        DataTable dt = new DataTable("tableText");
        private int rownumber = 0;
        private DateTime startDate = new DateTime();
        private DateTime endDate = new DateTime();
        private DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
        private string[] startDateTime = new string[2];
        private string[] endDateTime= new string[2];
        private DataTable dateDt = new DataTable(); //日期datatable，日期+时间仅占一列，方便数据传到图表
        private DataTable currentDt = new DataTable(); //当前数据，传到图标处理

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int i;
            //单个添加
            for (i = 300; i <= 38400; i = i*2)
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
            comboBox2.Text = "115200";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";

            string[] gaslist = { "all", "gas1", "gas2", "gas3", "gas4", "gas5", "gas6" };
            comboBox6.Items.AddRange(gaslist);
            comboBox6.Text = "all";

            initDataSet();
            initDateDt();//初始化DateDt表，表结构确定
            currentDt = dateDt.Clone(); //Clone()复制表的结构
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
            DateTime datetime = new DateTime();
            rownumber = 0;
            openFileDialog1.Filter = "*.txt|*.txt|所有文件|*.*";
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dt.Rows.Clear();
                dateDt.Rows.Clear();
                
                dataGridView1.DataSource = null;
                filename = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                StreamReader sr = new StreamReader(filename);
                string string1 = "";
                //textBox1.Clear();
                while ((string1 = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(string1))
                        continue;

                    rownumber++;
                    string trim = Regex.Replace(string1.Trim(), "\\s{2,}", " ");
                    string[] result = trim.Split();

                    //得到一行数据处理date_time[] + gas[]
                    for (int j = 0; j < result.Length; j++)
                    {
                        if (j >= 2) //气体浓度
                        {
                            float fresult = Convert.ToSingle(result[j]);
                            if (fresult == 3999)
                                fresult = 100;
                            gas[j - 2] = fresult;
                            //textBox1.AppendText(fresult.ToString() + " ");
                        }
                        else if(j == 0) //日期
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

                    //addDataRow(date_time, gas);

                    addDataSet(date_time, gas, rownumber);
                    addDateDt(datetime, gas, rownumber);

                    if(rownumber == 1) //获取起始时间
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
                //DateTimeFormatInfo dtFormat1 = new DateTimeFormatInfo();

                endDate = sToDate(str);
                dateTimePicker1.Value = endDate;

                dataGridView1.DataSource = dataSet1.Tables[0];
                dataGridView1.Rows[0].Selected = false;

                /*首列标号
                 * for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1.Rows[i].HeaderCell.Value = i.ToString();
                }
                */

                button6.Enabled = true;
                button7.Enabled = true;
            }
        }

        //初始化DataSet，加入dt表并初始化结构
        private void initDataSet()
        {
            dataSet1.Clear();
            dataSet1.Tables.Add(dt);
            DataColumn dc1 = new DataColumn("id", Type.GetType("System.Int32"), "");//创建第一列
            DataColumn dc2 = new DataColumn("date", Type.GetType("System.String"), "");//创建第二列
            DataColumn dc3 = new DataColumn("time", Type.GetType("System.String"), "");//创建第三列
            DataColumn dc4 = new DataColumn("gas1", Type.GetType("System.Decimal"), "");//创建第四列
            DataColumn dc5 = new DataColumn("gas2", Type.GetType("System.Decimal"), "");//创建第五列
            DataColumn dc6 = new DataColumn("gas3", Type.GetType("System.Decimal"), "");//创建第六列
            DataColumn dc7 = new DataColumn("gas4", Type.GetType("System.Decimal"), "");//创建第七列
            DataColumn dc8 = new DataColumn("gas5", Type.GetType("System.Decimal"), "");//创建第八列
            DataColumn dc9 = new DataColumn("gas6", Type.GetType("System.Decimal"), "");//创建第九列
            dt.Columns.Add(dc1);//向DataTable中添加一列
            dt.Columns.Add(dc2);//向DataTable中添加一列
            dt.Columns.Add(dc3);//向DataTable中添加一列
            dt.Columns.Add(dc4);//向DataTable中添加一列
            dt.Columns.Add(dc5);//向DataTable中添加一列
            dt.Columns.Add(dc6);//向DataTable中添加一列
            dt.Columns.Add(dc7);//向DataTable中添加一列
            dt.Columns.Add(dc8);//向DataTable中添加一列
            dt.Columns.Add(dc9);//向DataTable中添加一列
        }

        //初始化DateDt表结构
        private void initDateDt()
        {
            dateDt.Clear();
            DataColumn dc1 = new DataColumn("id", Type.GetType("System.Int32"), "");//创建第一列
            DataColumn dc2 = new DataColumn("date", Type.GetType("System.DateTime"), "");//创建第二列
            DataColumn dc3 = new DataColumn("gas1", Type.GetType("System.Decimal"), "");//创建第三列
            DataColumn dc4 = new DataColumn("gas2", Type.GetType("System.Decimal"), "");//创建第四列
            DataColumn dc5 = new DataColumn("gas3", Type.GetType("System.Decimal"), "");//创建第五列
            DataColumn dc6 = new DataColumn("gas4", Type.GetType("System.Decimal"), "");//创建第六列
            DataColumn dc7 = new DataColumn("gas5", Type.GetType("System.Decimal"), "");//创建第七列
            DataColumn dc8 = new DataColumn("gas6", Type.GetType("System.Decimal"), "");//创建第八列
            dateDt.Columns.Add(dc1);//向DataTable中添加一列
            dateDt.Columns.Add(dc2);//向DataTable中添加一列
            dateDt.Columns.Add(dc3);//向DataTable中添加一列
            dateDt.Columns.Add(dc4);//向DataTable中添加一列
            dateDt.Columns.Add(dc5);//向DataTable中添加一列
            dateDt.Columns.Add(dc6);//向DataTable中添加一列
            dateDt.Columns.Add(dc7);//向DataTable中添加一列
            dateDt.Columns.Add(dc8);//向DataTable中添加一列
        }

        //向DateSet里添加一行数据
        private void addDataSet(string[] s, float[] f, int row)
        {
            DataRow dr = dt.NewRow();
            dr["id"] = row;
            dr["date"] = s[0];
            dr["time"] = s[1];
            dr["gas1"] = f[0];
            dr["gas2"] = f[1];
            dr["gas3"] = f[2];
            dr["gas4"] = f[3];
            dr["gas5"] = f[4];
            dr["gas6"] = f[5];
            dt.Rows.Add(dr);
            dataSet1.AcceptChanges();
        }

        //向DateDt里添加一行数据
        private void addDateDt(DateTime dt, float[] f, int row)
        {

            DataRow dr = dateDt.NewRow();
            dr["id"] = row;
            dr["date"] = dt;
            dr["gas1"] = f[0];
            dr["gas2"] = f[1];
            dr["gas3"] = f[2];
            dr["gas4"] = f[3];
            dr["gas5"] = f[4];
            dr["gas6"] = f[5];
            dateDt.Rows.Add(dr);
        }

        //筛选按钮
        private void button6_Click(object sender, EventArgs e)
        {
            button5.Enabled = true; //启用复位

            string end = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string start = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string[] endarr = end.Split(' ');
            string[] startarr = start.Split(' ');
            startDateTime[0] = startarr[0];
            startDateTime[1] = startarr[1];
            endDateTime[0] = endarr[0];
            endDateTime[1] = endarr[1];

            if(startDateTime[0].Equals(endDateTime[0])) //如果起始日期相同
            {
                DataRow[] drArr = dt.Select("time>='" + startDateTime[1] + "' and time<='" + endDateTime[1] + "'"); // 日期相同，只用考虑起始时间

                //把一个表的数据复制到另一个表
                DataTable dtNew = dt.Clone();
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtNew.ImportRow(drArr[i]);
                }

                dtNew.DefaultView.Sort = "id ASC";
                dtNew = dtNew.DefaultView.ToTable();
                selectGas(dtNew);
                //dataGridView1.DataSource = dtNew;
            }
            else //如果起始日期不同
            {
                DataRow[] drArr0 = dt.Select("date>='" + startDateTime[0] + "' and date<='" + endDateTime[0] + "'"); // 日期不同，不用考虑时间对比
                DataRow[] drArr1 = dt.Select("date='" + startDateTime[0] + "' and time>'" + startDateTime[1] + "'"); //左边界日期，当天时间需大于起始时间
                DataRow[] drArr2 = dt.Select("date='" + endDateTime[0] + "' and time<'" + endDateTime[1] + "'"); //右边界日期，当天时间需小于结束时间   

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

                dtNew.DefaultView.Sort = "id ASC";
                dtNew = dtNew.DefaultView.ToTable();
                selectGas(dtNew);
                //dataGridView1.DataSource = dtNew;
            }

            DataRow[] dr = dateDt.Select("date>='" + sToDate(start) + "' and date<='" + sToDate(end) + "'");

            //把一个表的数据复制到另一个表
            DataTable dtNew1 = dateDt.Clone();
            for (int i = 0; i < dr.Length; i++)
            {
                dtNew1.ImportRow(dr[i]);
            }
            dtNew1.DefaultView.Sort = "id ASC";
            dtNew1 = dtNew1.DefaultView.ToTable();
            selectGasDt(dtNew1);

            //dataGridView1.Rows[0].Selected = false;
        }

        //传入日期筛选后的datatable，进行气体筛选，将结果复制到新表中绑定到dataGridView
        private void selectGas(DataTable datatable)
        {
            float up = (float)numericUpDown2.Value;
            float down = (float)numericUpDown3.Value;
            string gas = comboBox6.Text.ToString();

            if (gas.Equals("all"))
            {
                dataGridView1.DataSource = datatable;
            }
            else
            {
                DataRow[] drArr = datatable.Select(gas + ">=" + down + "and " + gas + "<=" + up);

                //把一个表的数据复制到另一个表
                DataTable dtNew = datatable.Clone();
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtNew.ImportRow(drArr[i]);
                }
                dtNew.DefaultView.Sort = "id ASC";
                dtNew = dtNew.DefaultView.ToTable();
                dataGridView1.DataSource = dtNew;
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
                dtNew.DefaultView.Sort = "id ASC";
                dtNew = dtNew.DefaultView.ToTable();
                currentDt = dtNew;
            }
            else
            {
                currentDt = datatable;
            }
        }

        //当选择气体为all时，取消气体上限下限
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
            }
        }

        //复位按钮
        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dt;
            currentDt = dateDt;
            dateTimePicker1.Value = endDate;
            dateTimePicker2.Value = startDate;
        }

        //图表显示按钮
        private void button7_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(currentDt);
            f2.Show();
        }

        private DateTime sToDate(string s)
        {
            dtFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";
            DateTime dt = Convert.ToDateTime(s, dtFormat);
            return dt;
        }
    }
}
