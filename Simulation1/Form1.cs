using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Simulation1
{
    public partial class Form1 : Form
    {
        private bool _isSimulated;
        private double _stepModeling;
        private double _speedX;
        private double _speedY;
        private double _time;
        private double _currentX;
        private double _currentY;
        private double _maxY;
        private double _k;
        private const double _C = 0.15;
        private const double _g = 9.81;
        private const double _rho = 1.29;
        private int _simulateCount = 0;
        private const int _simulateMaxCount = 7;
        private bool _isResizedChart;
        public Form1()
        {
            InitializeComponent();
            _isSimulated = false;
            _isResizedChart = false;
        }

        private void startSimulate_Click(object sender, EventArgs e)
        {
            if (_isSimulated) return;
            if(_simulateCount >= _simulateMaxCount)
            {
                MessageBox.Show($"Кол-во симуляций не может превышать {_simulateMaxCount} раз");
                return;
            }
            #region parseValue
            double height = (double)edHeight.Value;
            double angle = (double)edAngle.Value;
            double startSpeed = (double)edSpeed.Value;
            double size = (double)edSize.Value;
            double weight = (double)edWeight.Value;
            _stepModeling = (double)edStepModel.Value;
            #endregion

            _time = 0;
            _currentX = 0;
            _maxY = _currentY = height;
            angle = angle * Math.PI / 180;
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            _k = 0.5 * _C * _rho * size / weight;
            _speedX = startSpeed * cosA;
            _speedY = startSpeed * sinA;

            CreateSeries();
            
            chartResult.Series.Last().Points.AddXY(_currentX,_currentY);
            _isSimulated = true;
            _simulateCount++;
            timer1.Start();
        }
        private void ResizeChart()
        {
            float legendmaxsize = 140;

            float maxpercentage = (legendmaxsize / (float)chartResult.Width) * 100;

            var area = chartResult.ChartAreas.Last();

            area.Position.Auto = false;

            area.Position.Height = 100;
            area.Position.Width = 100F - maxpercentage;

            area.Position.Y = 0;
            area.Position.X = maxpercentage;

            _isResizedChart = true;
        }
        private void CreateSeries()
        {
            var series = chartResult.Series.Add($"Simulate {chartResult.Series.Count + 1}");
            chartResult.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend($"Legend {chartResult.Series.Count + 1}"));
            var legend = chartResult.Legends.Last();
            legend.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;

            if (!_isResizedChart) ResizeChart();
            
            chartResult.ChartAreas.Last().AxisX.Minimum = 0;
            chartResult.ChartAreas.Last().AxisY.Minimum = 0;

            series.Legend = legend.Name;
            series.IsVisibleInLegend = true;
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            double root = Math.Sqrt(_speedX * _speedX + _speedY * _speedY);
            _time += _stepModeling;
            _speedX -= _k * _speedX * root * _stepModeling;
            _speedY -= (_g + _k * _speedY * root) * _stepModeling;

            _currentX += _speedX * _stepModeling;
            _currentY += _speedY * _stepModeling;

            if(_currentY > _maxY) _maxY = _currentY;

            chartResult.Series.Last().Points.AddXY(_currentX, _currentY);

            if (_currentY <= 0)
                StopTimer();

        }
        private void StopTimer()
        {
            timer1.Stop();
            _isSimulated = false;

            double speed = Math.Sqrt(_speedX * _speedX + _speedY * _speedY);

            int columnCount = tableResults.ColumnCount += 1;
            tableResults.Controls.Add(CreateResultLayout(_stepModeling.ToString(), _currentX.ToString(), _maxY.ToString(), speed.ToString()), columnCount-1, 0);
        }

        private TableLayoutPanel CreateResultLayout(string stepModeling, string distance, string maxHeight, string speedInEnd)
        {
            TableLayoutPanel result = new TableLayoutPanel();
            result.ColumnCount = 1;
            result.RowCount = 4;
            result.Controls.Add(CreateResultLabel(stepModeling), 0, 0);
            result.Controls.Add(CreateResultLabel(distance), 0, 1);
            result.Controls.Add(CreateResultLabel(maxHeight), 0, 2);
            result.Controls.Add(CreateResultLabel(speedInEnd), 0, 3);
            result.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            result.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            result.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            result.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            result.Size = new Size(99, 188);
            
            return result;
        }
        private Label CreateResultLabel(string text)
        {
            Label label = new Label();
            label.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left
            | AnchorStyles.Right;
            label.AutoSize = true;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Text = text;
            label.Margin = new Padding(0);
            return label;
        }
        private void clearData_Click(object sender, EventArgs e)
        {
            if (_isSimulated)
            {
                MessageBox.Show("Во время симуляции, нельзя очищать данные");
                return;
            }
            foreach(var serie in chartResult.Series)
            {
                serie.Points.Clear();
            }
            chartResult.Series.Clear();
            chartResult.Legends.Clear();
            _simulateCount = 0;
            _isResizedChart = false;
        }
    }
}
