using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace upper_computer
{
    public partial class Form2 : Form
    {
        private static int MAX_NUMBER = 100;
        private DataTable datatable = new DataTable();
        private double[] gasMax = new double[6] { 0, 0, 0, 0, 0, 0};
        //private int gas_number = 0;
       
        public Form2(DataTable datatable)
        {
            InitializeComponent();
            this.datatable = datatable;
        }

        public void runMain()
        {
            addSeries();
            setArea();
            setAttri();
            RefreshData();
            //setXYCursor();
        }

        public void addSeries()
        {
            chart1.DataSource = datatable;
            chart1.Series.Clear();
            for(int i = 0; i < datatable.Columns.Count - 2; i++)
            {
                chart1.Series.Add(i.ToString());
            }
        }

        public void RefreshData()
        {
            //通过datatable绑定数据
        
            for(int i = 0; i < datatable.Columns.Count - 2; i++)
            {
                chart1.Series[i].XValueMember = datatable.Columns[1].ColumnName;
                chart1.Series[i].YValueMembers = datatable.Columns[i + 2].ColumnName;
                chart1.DataBind();
            }


            //求出最大点
            double m = 0;

            for (int i = 0; i < datatable.Columns.Count - 2; i++)
            {
                m = 0;
                for(int j = 0; j < datatable.Rows.Count; j++)
                {
                    if (Convert.ToDouble(datatable.Rows[j][i + 2]) >= m)
                    {
                        m = Convert.ToDouble(datatable.Rows[j][i + 2]);
                    }
                }
                gasMax[i] = m;

                foreach (DataPoint dp in chart1.Series[i].Points)
                {
                    if (dp.YValues[0] >= m && dp.YValues[0] != 0)
                    {
                        //dp.MarkerColor = Color.Red;
                        dp.MarkerStyle = MarkerStyle.Star5;
                        dp.MarkerSize = 6;
                    }
                }
            }
            //maxline();
        }

        public void setAttri()
        {
            for (int i = 0; i < datatable.Columns.Count - 2; i++)
            {
                chart1.Series[i].ChartType = SeriesChartType.Line;
                chart1.Series[i].XValueType = ChartValueType.DateTime;
          
                chart1.Series[i].LegendToolTip = "Gas" + (i + 1).ToString();//鼠标放到系列上出现的文字
                chart1.Series[i].LegendText = "Gas" + (i+1).ToString();//系列名字 
                
                chart1.Series[i].MarkerStyle = MarkerStyle.Circle;
                chart1.Series[i].MarkerSize = 3;
                //chart1.Series[i].MarkerColor = Color.Red;
                //chart1.Series[i].ToolTip = "#VALX,#VALY";
            }
        }

        public void setArea()
        {
            //设置坐标轴标题
            chart1.ChartAreas[0].AxisX.Title = "日期";
            chart1.ChartAreas[0].AxisY.Title = "浓度";

            //设置坐标轴标题的字体
            chart1.ChartAreas[0].AxisX.TitleFont = new Font("微软雅黑", 12F);
            chart1.ChartAreas[0].AxisY.TitleFont = new Font("微软雅黑", 12F);

            //设置坐标轴栅格
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            //设置坐标轴显示格式
            chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true; 
            chart1.ChartAreas[0].AxisY.IsLabelAutoFit = true;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm:ss";
            chart1.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;
            chart1.ChartAreas[0].AxisX.ScrollBar.Size = 14;
            chart1.ChartAreas[0].AxisX.ScaleView.MinSizeType = DateTimeIntervalType.Minutes; 
            chart1.ChartAreas[0].AxisX.ScaleView.SizeType = DateTimeIntervalType.Minutes;
            chart1.ChartAreas[0].AxisX.ScaleView.Size = 10;
            chart1.ChartAreas[0].AxisX.ScaleView.MinSize = 2;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = 10;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 5;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
            chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Seconds;

            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            runMain();
            //string[] gasRange = { "Gas1", "Gas2", "Gas3", "Gas4", "Gas5", "Gas6" };
            for(int i = 0; i < datatable.Columns.Count - 2; i++)
            {
                checkedListBox1.Items.Add("Gas" + (i + 1).ToString());
            }

            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true); //默认全选中
            }
            textBox1.Text = string.Format("曲线：\r\n时间：\r\n数值："); 
        }
      
        //鼠标滚轮事件，用来缩放
        protected void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)//鼠标向上
            {
                if (chart1.ChartAreas[0].AxisX.ScaleView.Size < MAX_NUMBER)//判断显示的最大数值
                    chart1.ChartAreas[0].AxisX.ScaleView.Size += 5;//+=5---滚动一次显示5个

            }
            else//鼠标向下滚动
            {
                if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 5)
                    chart1.ChartAreas[0].AxisX.ScaleView.Size -= 5;// - = 5---滚动一次减小显示5个
                else if(chart1.ChartAreas[0].AxisX.ScaleView.Size <= 5)
                    chart1.ChartAreas[0].AxisX.ScaleView.Size = 2;
            }
        }

        //鼠标点击数据点事件
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            HitTestResult hit = chart1.HitTest(e.X, e.Y);

            if (hit.Series != null && hit.PointIndex != -1)
            {
                DateTime dt = DateTime.FromOADate(hit.Series.Points[hit.PointIndex].XValue);
                double y = hit.Series.Points[hit.PointIndex].YValues[0];
                string lgtext = hit.Series.LegendText;

                textBox1.Text = string.Format("曲线：{0}\r\n时间：{1}\r\n数值：{2:F1}", lgtext, dt, y);
            }
        }
        
        //气体选择事件
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i) == true)
                    chart1.Series[i].Enabled = true;
                else
                    chart1.Series[i].Enabled = false;
            }
            chart1.ChartAreas[0].RecalculateAxesScale(); //刷新坐标轴
            checkedListBox1.ClearSelected();
        }

        //鼠标所在坐标显示
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                double cursorX = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double cursorY = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                DateTime dateTime = DateTime.FromOADate(cursorX);
                textBox2.Text = string.Format("时间：{0}\r\n数值：{1:F1}", dateTime, cursorY);
            }
            catch
            {
            }
        }

        //气体全选
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
                chart1.Series[i].Enabled = true;
            }
        }

        //气体全不选
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
                chart1.Series[i].Enabled = false;
            }
        }
    }
}
