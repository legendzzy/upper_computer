using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace upper_computer
{
    public partial class Form3 : Form
    {
        private static int INTERVAL = 3;
        private DataTable dataTable = new DataTable(); //从form1传来的原始数据表
        private Gas[] gasSet = new Gas[6];
        private int gasNumber = 0;
        private DateTime initStartTime, initEndTime;
        private DataTable alarmTable = new DataTable(); //要显示在datagridview上的警报分析表
        private List<DataTable> tableList = new List<DataTable>(); //每一段报警时段的原始数据存为一个table加在该List中
        private int gasIndex;
        public Form3(DataTable dataTable, Gas[] gasSet, DateTime initStartTime, DateTime initEndTime)
        {
            InitializeComponent();
            this.gasSet = gasSet;
            this.dataTable = dataTable;
            gasNumber = dataTable.Columns.Count - 2;
            this.initStartTime = initStartTime;
            this.initEndTime = initEndTime;
        }

        //确定按钮
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //gasIndex = comboBox1.SelectedIndex;
                //label10.Text = gasSet[gasIndex].low_level_alarm.ToString();
                //label11.Text = gasSet[gasIndex].high_level_alarm.ToString();
                //label12.Text = gasSet[gasIndex].unit;
                //label13.Text = gasSet[gasIndex].range.ToString();
                //label8.Text = gasSet[gasIndex].name;
                DateTime startDt = dateTimePicker1.Value;
                DateTime endDt = dateTimePicker2.Value;
                float alarm;
                string alarmText;
                if (radioButton1.Checked)
                {
                    alarm = gasSet[gasIndex].low_level_alarm;
                    alarmText = radioButton1.Text;
                }
                else if (radioButton2.Checked)
                {
                    alarm = gasSet[gasIndex].high_level_alarm;
                    alarmText = radioButton2.Text;
                }
                else
                {
                    alarm = (float)Convert.ToDecimal(textBox1.Text);
                    alarmText = textBox1.Text + gasSet[gasIndex].unit;
                }

                tableList.Clear();
                DataRow[] dr = dataTable.Select("date>='" + startDt + "' and date<='" + endDt + "' and " + gasSet[gasIndex].name + ">=" + alarm);

                if (dr.Length == 0)
                {
                    label8.Text = gasSet[gasIndex].name + "在本时段内未找到符合条件的报警信息";
                    dataGridView1.DataSource = null;
                    button2.Enabled = false;
                    return;
                }

                button2.Enabled = true;
                DataTable newDt = dataTable.Clone(); //newDt表为筛选后的信息表
                for (int i = 0; i < dr.Length; i++)
                {
                    newDt.ImportRow(dr[i]);
                }
                newDt.DefaultView.Sort = "id ASC";
                newDt = newDt.DefaultView.ToTable();


                DataTable currTable = dataTable.Clone();
                currTable.ImportRow(newDt.Rows[0]); //先加入第一行数据

                if (dr.Length != 1)//多于一条数据
                {
                    for (int i = 1; i < newDt.Rows.Count; i++)
                    {
                        if ((int)newDt.Rows[i]["id"] - (int)newDt.Rows[i - 1]["id"] < INTERVAL)
                        {
                            currTable.ImportRow(newDt.Rows[i]);
                        }
                        else //将当前table加入list中，并且清空该table重新记录下一个要加到list中的table
                        {
                            DataTable newTable = new DataTable();
                            newTable = currTable.Copy();
                            tableList.Add(newTable);
                            currTable.Rows.Clear();
                            currTable.ImportRow(newDt.Rows[i]);
                        }
                    }
                    DataTable newTable_end = new DataTable();
                    newTable_end = currTable.Copy();
                    tableList.Add(newTable_end); //将最后一个table加入list
                }
                else //只有一条数据
                {
                    tableList.Add(currTable.Copy());
                }


                setAlarmTable(tableList, alarmTable, gasSet[gasIndex]);
                alarmTable.DefaultView.Sort = "id ASC";
                alarmTable = alarmTable.DefaultView.ToTable();
                dataGridView1.DataSource = alarmTable;
                dataGridView1.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd hh:mm:ss";
                dataGridView1.Columns[2].DefaultCellStyle.Format = "yyyy-MM-dd hh:mm:ss";
                dataGridView1.Columns[3].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns[4].DefaultCellStyle.Format = "0.0";
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                dataGridView1.Columns[0].Width = 45;
                label8.Text = gasSet[gasIndex].name + " 共查询到" + tableList.Count + "条超过" + alarmText + "的时段记录,可选择一行显示统计图";
            }
            catch(Exception ex)
            {
                MessageBox.Show("输入异常");
            }
            

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //加载气体选择选项
            for (int i = 0; i < gasNumber; i++)
            {
                comboBox1.Items.Add(gasSet[i].name);
            }

            //加载默认起始时间
            dateTimePicker1.Value = initStartTime;
            dateTimePicker2.Value = initEndTime;

            //加载数据表
            initAlarmTable();

            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.DefaultCellStyle.Font = new Font("宋体", 9);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 10);

        }

        private void setAlarmTable(List<DataTable> list, DataTable targetTable, Gas gas)
        {
            targetTable.Rows.Clear();
            for(int i = 0; i < list.Count; i++) //每一时间段数据，处理后，作为一行数据放到要显示的目标表中
            {
                DataTable dt = list[i];
                
                DateTime starttime = (DateTime)dt.Rows[0]["date"];
                DateTime endtime = (DateTime)dt.Rows[dt.Rows.Count - 1]["date"];
                double max = Convert.ToDouble(dt.Rows[0][gas.name]);
                double average = 0;
                for(int j = 0; j < dt.Rows.Count; j++)
                {
                    average += Convert.ToDouble(dt.Rows[j][gas.name]);
                    if(max < Convert.ToDouble(dt.Rows[j][gas.name]))
                        max = Convert.ToDouble(dt.Rows[j][gas.name]);
                }
                average /= dt.Rows.Count;

                DataRow dr = targetTable.NewRow();
                dr["id"] = i + 1;
                dr["starttime"] = starttime;
                dr["endtime"] = endtime;
                dr["max"] = max;
                dr["average"] = average;
                targetTable.Rows.Add(dr);
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
                button1.Enabled = true;
            gasIndex = comboBox1.SelectedIndex;
            label10.Text = gasSet[gasIndex].low_level_alarm.ToString();
            label11.Text = gasSet[gasIndex].high_level_alarm.ToString();
            label12.Text = gasSet[gasIndex].unit;
            label13.Text = gasSet[gasIndex].range.ToString();
            label8.Text = gasSet[gasIndex].name;
        }

        //统计图显示
        private void button2_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index; //选中行
            int id = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells[0].Value);
            DataTable currTable = tableList[id - 1]; //要使用的数据表
            Form2 f2 = new Form2(currTable, gasSet, gasIndex);
            f2.StartPosition = FormStartPosition.CenterScreen;
            f2.Show();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                textBox1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
            }
        }

        private void initAlarmTable()
        {
            alarmTable.Columns.Clear();
            alarmTable.Columns.Add(new DataColumn("id", Type.GetType("System.Int32"), ""));
            alarmTable.Columns.Add(new DataColumn("starttime", Type.GetType("System.DateTime"), ""));
            alarmTable.Columns.Add(new DataColumn("endtime", Type.GetType("System.DateTime"), ""));
            alarmTable.Columns.Add(new DataColumn("max", Type.GetType("System.Decimal"), ""));
            alarmTable.Columns.Add(new DataColumn("average", Type.GetType("System.Decimal"), ""));
        }
    }
}
