using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace upper_computer
{
    public partial class Form2 : Form
    {
        private static int MAX_NUMBER = 50; //鼠标滚轮能缩放的最多点数
        private DataTable datatable = new DataTable();
        //private double[] gasMax = new double[6] { 0, 0, 0, 0, 0, 0};
        private DataPoint clickDp = null; //记录最后一次在图中点击的点
        private DataPoint clickDpDuplicate = new DataPoint();
        private Gas[] gasSet = new Gas[6]; //气体数组
        private int[] gasChosen = new int[6] {0, 0, 0, 0, 0, 0}; //选择的气体
        private int gasNumber = 0; //气体数量
        private int gasIndex = -1; //气体数组下标

        public Form2(DataTable datatable, Gas[] gasSet)
        {
            InitializeComponent();
            this.datatable = datatable;
            this.gasSet = gasSet;
            gasNumber = datatable.Columns.Count - 2;
        }

        //该构造函数用于选择了一个气体
        public Form2(DataTable datatable, Gas[] gasSet, int gasIndex)
        {
            InitializeComponent();
            this.datatable = datatable;
            this.gasSet = gasSet;
            gasNumber = datatable.Columns.Count - 2;
            this.gasIndex = gasIndex;
        }

        public void runMain()
        {
            addSeries();
            setArea();
            setAttri();
            RefreshData();
            initCheckedListBox();
            setPanel();
        }

        //添加Series
        public void addSeries()
        {
            chart1.DataSource = datatable;
            chart1.Series.Clear();
            for(int i = 0; i < gasNumber; i++)
            {
                chart1.Series.Add(gasSet[i].name);
            }
        }

        //图表数据
        public void RefreshData()
        {
            //通过datatable绑定数据
        
            for(int i = 0; i < gasNumber; i++)
            {
                chart1.Series[i].XValueMember = datatable.Columns[1].ColumnName;
                chart1.Series[i].YValueMembers = datatable.Columns[i + 2].ColumnName;
                chart1.DataBind();
            }


            //求出最大点并特殊显示
            double m = 0;

            for (int i = 0; i < gasNumber; i++)
            {
                m = 0;
                for(int j = 0; j < datatable.Rows.Count; j++)
                {
                    if (Convert.ToDouble(datatable.Rows[j][i + 2]) >= m)
                    {
                        m = Convert.ToDouble(datatable.Rows[j][i + 2]);
                    }
                }
                //gasMax[i] = m;

                foreach (DataPoint dp in chart1.Series[i].Points)
                {
                    if (dp.YValues[0] >= m && dp.YValues[0] != 0)
                    {
                        //dp.MarkerColor = Color.Red;
                        dp.MarkerStyle = MarkerStyle.Star5;
                        dp.MarkerSize = 8;
                    }
                }
            }
            //maxline();
        }

        //设置Series中的属性
        public void setAttri()
        {
            for (int i = 0; i < gasNumber; i++)
            {
                chart1.Series[i].ChartType = SeriesChartType.Line;
                chart1.Series[i].XValueType = ChartValueType.DateTime;
          
                chart1.Series[i].LegendToolTip = gasSet[i].name;//鼠标放到系列上出现的文字
                chart1.Series[i].LegendText = gasSet[i].name;//系列名字 
                
                chart1.Series[i].MarkerStyle = MarkerStyle.Circle;
                chart1.Series[i].MarkerSize = 5;
            }
        }

        //设置ChartArea的属性
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
            chart1.ChartAreas[0].AxisX.ScaleView.MinSizeType = DateTimeIntervalType.Seconds; 
            chart1.ChartAreas[0].AxisX.ScaleView.SizeType = DateTimeIntervalType.Minutes;
            chart1.ChartAreas[0].AxisX.ScaleView.Size = 5;
            chart1.ChartAreas[0].AxisX.ScaleView.MinSize = 2;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = 10;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 5;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;

            //设置游标格式
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
            chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Seconds;

            chart1.ChartAreas[0].CursorY.IsUserEnabled = false;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = false;

        }

        //窗口加载函数
        private void Form2_Load(object sender, EventArgs e)
        {
            runMain();    
        }
      
        //鼠标滚轮事件，用来缩放
        protected void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) //鼠标向上，缩放
            {
                if (chart1.ChartAreas[0].AxisX.ScaleView.Size < MAX_NUMBER) //判断显示的最大数值
                    chart1.ChartAreas[0].AxisX.ScaleView.Size += 2; //滚动一次显示2个

            }
            else //鼠标向下滚动，放大
            {
                if (chart1.ChartAreas[0].AxisX.ScaleView.Size > 2)
                    chart1.ChartAreas[0].AxisX.ScaleView.Size -= 2; //滚动一次减小显示2个
                else if(chart1.ChartAreas[0].AxisX.ScaleView.Size <= 2)
                    chart1.ChartAreas[0].AxisX.ScaleView.Size = 1;
            }
        }

        //鼠标点击数据点事件函数
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            HitTestResult hit = chart1.HitTest(e.X, e.Y);

            if (hit.Series != null && hit.PointIndex != -1) //点击的点需要是Series上并且有点Index才可判定为有效（否则点击Legend会出现bug）
            {
                DateTime dt = DateTime.FromOADate(hit.Series.Points[hit.PointIndex].XValue); //时间
                double y = hit.Series.Points[hit.PointIndex].YValues[0]; //y值
                string lgtext = hit.Series.LegendText; //曲线名称
                int seriesindex = 0;
                for(int i = 0; i < 6; i++)
                {
                    if (hit.Series.Equals(chart1.Series[i]))
                    {
                        seriesindex = i;
                        break;
                    }
                }
                textBox1.Text = string.Format("曲线：{0}  时间：{1}  数值：{2:F1}{3}", lgtext, dt, y, gasSet[seriesindex].unit);
                DataPoint dp = hit.Series.Points[hit.PointIndex];
                if(clickDp == dp) //如果和上一次点击的点坐标相同
                {
                    if(dp.MarkerStyle == MarkerStyle.Circle && dp.MarkerColor == Color.Red && dp.MarkerSize == 8) //如果是已被选中的点，回到原本样式
                    {
                        setPointMarker(clickDpDuplicate, dp);
                        textBox1.Text = string.Format("曲线：\r\n时间：\r\n数值：");
                    }
                    else //再次点击的点是该点的原本样式，需要重新被标记
                    {
                        //this.clickDp.XValue = dp.XValue;
                        //this.clickDp.YValues[0] = dp.YValues[0];
                        dp.MarkerSize = 8;
                        dp.MarkerColor = Color.Red;
                        dp.MarkerStyle = MarkerStyle.Circle;
                    }
                }
                else //点击了其他的点or第一次点击
                {
                    if(clickDp == null)//第一次点击
                    {
                        setPointMarker(dp, clickDpDuplicate);//复制该点的原本属性，后续再进行更改
                        //this.clickDp.XValue = dp.XValue;
                        //this.clickDp.YValues[0] = dp.YValues[0];
                        this.clickDp = dp;
                        dp.MarkerSize = 8;
                        dp.MarkerColor = Color.Red;
                        dp.MarkerStyle = MarkerStyle.Circle;
                    }
                    else //点击了其他点
                    {
                        setPointMarker(clickDpDuplicate, clickDp);//将上一个点的属性还原
                        setPointMarker(dp, clickDpDuplicate);//复制该点的原本属性，后续再进行更改
                        dp.MarkerSize = 8;
                        dp.MarkerColor = Color.Red;
                        dp.MarkerStyle = MarkerStyle.Circle;
                        clickDp = dp;
                    }
                }
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
            checkedListBox1.ClearSelected(); //显示时取消横行选中，只显示打勾
        }

        //鼠标移动事件，鼠标所在坐标显示
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                //chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                double cursorX = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double cursorY = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                DateTime dateTime = DateTime.FromOADate(cursorX); //鼠标所在x轴的时间
                textBox2.Text = dateTime.ToString()+ "  ";

                //判断当前哪些气体被选中
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i) == true)
                        gasChosen[i] = 1;
                    else
                        gasChosen[i] = 0;
                }

                //寻找该时间点的所有气体对应数值
                for (int i = 0; i < gasNumber; i++)
                {
                    foreach (DataPoint dp in chart1.Series[i].Points)
                    {
                        DateTime d = DateTime.FromOADate(dp.XValue);
                        if (dateTime.Second == d.Second && dateTime.Minute == d.Minute && dateTime.Hour == d.Hour && dateTime.Date == d.Date) //找到了横坐标时间相同的点
                        {
                            switch (i)
                            {
                                case 0:
                                    label12.Text = dp.YValues[0].ToString();
                                    if(dp.YValues[0] < gasSet[0].low_level_alarm)
                                        label12.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[0].low_level_alarm && dp.YValues[0] <= gasSet[0].high_level_alarm)
                                        label12.ForeColor = Color.Orange;
                                    else
                                        label12.ForeColor = Color.Red;
                                    break;
                                case 1:
                                    label13.Text = dp.YValues[0].ToString();
                                    if (dp.YValues[0] < gasSet[1].low_level_alarm)
                                        label13.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[1].low_level_alarm && dp.YValues[0] <= gasSet[1].high_level_alarm)
                                        label13.ForeColor = Color.Orange;
                                    else
                                        label13.ForeColor = Color.Red;
                                    break;
                                case 2:
                                    label14.Text = dp.YValues[0].ToString();
                                    if (dp.YValues[0] < gasSet[2].low_level_alarm)
                                        label14.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[2].low_level_alarm && dp.YValues[0] <= gasSet[2].high_level_alarm)
                                        label14.ForeColor = Color.Orange;
                                    else
                                        label14.ForeColor = Color.Red;
                                    break;
                                case 3:
                                    label15.Text = dp.YValues[0].ToString();
                                    if (dp.YValues[0] < gasSet[3].low_level_alarm)
                                        label15.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[3].low_level_alarm && dp.YValues[0] <= gasSet[3].high_level_alarm)
                                        label15.ForeColor = Color.Orange;
                                    else
                                        label15.ForeColor = Color.Red;
                                    break;
                                case 4:
                                    label16.Text = dp.YValues[0].ToString();
                                    if (dp.YValues[0] < gasSet[4].low_level_alarm)
                                        label16.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[4].low_level_alarm && dp.YValues[0] <= gasSet[4].high_level_alarm)
                                        label16.ForeColor = Color.Orange;
                                    else
                                        label16.ForeColor = Color.Red;
                                    break;
                                case 5:
                                    label17.Text = dp.YValues[0].ToString();
                                    if (dp.YValues[0] < gasSet[5].low_level_alarm)
                                        label17.ForeColor = Color.Green;
                                    else if (dp.YValues[0] >= gasSet[5].low_level_alarm && dp.YValues[0] <= gasSet[5].high_level_alarm)
                                        label17.ForeColor = Color.Orange;
                                    else
                                        label17.ForeColor = Color.Red;
                                    break;
                                default:
                                    break;
                            }
                            if(gasChosen[i] == 1)
                                textBox2.AppendText(gasSet[i].name + "：" + string.Format("{0:F1}", dp.YValues[0]) + gasSet[i].unit + "  ");
                        }
                    }
                } 
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
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
            chart1.ChartAreas[0].RecalculateAxesScale();
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

        //曲线上鼠标悬浮提示
        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                DateTime dt = DateTime.FromOADate(dp.XValue);
                double y = dp.YValues[0];
                string lgtext = e.HitTestResult.Series.LegendText;
                e.Text = string.Format("曲线：{0}\r\n时间：{1}\r\n数值：{2:F1}", lgtext, dt, y);
            }
        }

        //将一个DataPoint的一些属性值复制到另一个
        private void setPointMarker(DataPoint source, DataPoint dp)
        {
            dp.MarkerStyle = source.MarkerStyle;
            dp.MarkerSize = source.MarkerSize;
            dp.MarkerColor = source.MarkerColor;
        }

        //气体选择框初始化
        private void initCheckedListBox()
        {
            for (int i = 0; i < gasNumber; i++)
            {
                checkedListBox1.Items.Add(gasSet[i].name);
            }
            if(gasIndex < 0) //all
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, true); //默认全选中
                }
            }
            else //选中了某气体
            {
                checkedListBox1.SetItemChecked(gasIndex, true);
                for(int i = 0; i < gasNumber; i++)
                {
                    if(checkedListBox1.GetItemChecked(i) == false)
                    {
                        chart1.Series[i].Enabled = false;
                    }
                }
            }
            textBox1.Text = string.Format("曲线：\r\n时间：\r\n数值：");
        }

        //设置显示面板，需要适应气体数量
        private void setPanel()
        {
            switch (gasNumber)
            {
                case 6:
                    label5.Text = gasSet[0].name;
                    label6.Text = gasSet[1].name;
                    label7.Text = gasSet[2].name;
                    label8.Text = gasSet[3].name;
                    label9.Text = gasSet[4].name;
                    label10.Text = gasSet[5].name;
                    label19.Text = gasSet[0].unit;
                    label20.Text = gasSet[1].unit;
                    label21.Text = gasSet[2].unit;
                    label22.Text = gasSet[3].unit;
                    label23.Text = gasSet[4].unit;
                    label24.Text = gasSet[5].unit;
                    break;
                case 5:
                    label5.Text = gasSet[0].name;
                    label6.Text = gasSet[1].name;
                    label7.Text = gasSet[2].name;
                    label8.Text = gasSet[3].name;
                    label9.Text = gasSet[4].name;
                    label19.Text = gasSet[0].unit;
                    label20.Text = gasSet[1].unit;
                    label21.Text = gasSet[2].unit;
                    label22.Text = gasSet[3].unit;
                    label23.Text = gasSet[4].unit;
                    label10.Visible = false;
                    label17.Visible = false;
                    label24.Visible = false;
                    panel3.Size = new Size(197, 285);
                    break;
                case 4:
                    label5.Text = gasSet[0].name;
                    label6.Text = gasSet[1].name;
                    label7.Text = gasSet[2].name;
                    label8.Text = gasSet[3].name;
                    label19.Text = gasSet[0].unit;
                    label20.Text = gasSet[1].unit;
                    label21.Text = gasSet[2].unit;
                    label22.Text = gasSet[3].unit;
                    label9.Visible = false;
                    label16.Visible = false;
                    label23.Visible = false;
                    label10.Visible = false;
                    label17.Visible = false;
                    label24.Visible = false;
                    panel3.Size = new Size(197, 240);
                    break;
                case 3:
                    label5.Text = gasSet[0].name;
                    label6.Text = gasSet[1].name;
                    label7.Text = gasSet[2].name;
                    label19.Text = gasSet[0].unit;
                    label20.Text = gasSet[1].unit;
                    label21.Text = gasSet[2].unit;
                    label9.Visible = false;
                    label16.Visible = false;
                    label23.Visible = false;
                    label10.Visible = false;
                    label17.Visible = false;
                    label24.Visible = false;
                    label8.Visible = false;
                    label15.Visible = false;
                    label22.Visible = false;
                    panel3.Size = new Size(197, 195);
                    break;
                case 2:
                    label5.Text = gasSet[0].name;
                    label6.Text = gasSet[1].name;
                    label19.Text = gasSet[0].unit;
                    label20.Text = gasSet[1].unit;
                    label9.Visible = false;
                    label16.Visible = false;
                    label23.Visible = false;
                    label10.Visible = false;
                    label17.Visible = false;
                    label24.Visible = false;
                    label8.Visible = false;
                    label15.Visible = false;
                    label22.Visible = false;
                    label7.Visible = false;
                    label14.Visible = false;
                    label21.Visible = false;
                    panel3.Size = new Size(197, 150);
                    break;
                case 1:
                    label5.Text = gasSet[0].name;
                    label19.Text = gasSet[0].unit;
                    label9.Visible = false;
                    label16.Visible = false;
                    label23.Visible = false;
                    label10.Visible = false;
                    label17.Visible = false;
                    label24.Visible = false;
                    label8.Visible = false;
                    label15.Visible = false;
                    label22.Visible = false;
                    label7.Visible = false;
                    label14.Visible = false;
                    label21.Visible = false;
                    label6.Visible = false;
                    label13.Visible = false;
                    label20.Visible = false;
                    panel3.Size = new Size(197, 105);
                    break;
                default:
                    break;
            }
        }
    }
}