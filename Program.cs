using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class Vertex
    {
        public string label;
        public bool isInTree;
        public Point position;
        public string type;

        public Vertex(string lab, Point pos, string t)
        {
            label = lab;
            isInTree = false;
            position = pos;
            type = t;
        }
    }

    public class DistOriginal
    {
        public int distance;
        public int parentVert;

        public DistOriginal(int pv, int d)
        {
            parentVert = pv;
            distance = d;
        }
    }

    public class Graph
    {
        private const int MAX_VERTS = 20;
        private const int INFINITY = 1000000;

        private Vertex[] vertexList;
        private int[,] adjMatrix;
        public DistOriginal[] sPath;

        private int nVerts;
        private int nTree;
        private int currentVert;
        private int startToCurrent;

        public List<string> steps = new List<string>();
        public List<int> blinkingNodes = new List<int>();

        public List<Tuple<int, int>> confirmedEdges = new List<Tuple<int, int>>();

        public List<Tuple<int, int>> finalPathEdges = new List<Tuple<int, int>>();

        public Graph()
        {
            vertexList = new Vertex[MAX_VERTS];
            adjMatrix = new int[MAX_VERTS, MAX_VERTS];

            for (int i = 0; i < MAX_VERTS; i++)
                for (int j = 0; j < MAX_VERTS; j++)
                    adjMatrix[i, j] = INFINITY;

            sPath = new DistOriginal[MAX_VERTS];
            nVerts = 0;
            nTree = 0;
        }

        public void AddVertex(string label, Point pos, string type)
        {
            vertexList[nVerts++] = new Vertex(label, pos, type);
        }

        public void AddEdge(int start, int end, int weight)
        {
            adjMatrix[start, end] = weight;
            adjMatrix[end, start] = weight;
        }

        public int GetIndex(string label)
        {
            for (int i = 0; i < nVerts; i++)
                if (vertexList[i] != null && vertexList[i].label == label)
                    return i;
            return -1;
        }

        public void ShortestPath(string startLabel, string endLabel)
        {
            steps.Clear();
            blinkingNodes.Clear();
            confirmedEdges.Clear();
            finalPathEdges.Clear();

            int start = GetIndex(startLabel);
            int end = GetIndex(endLabel);

            if (start == -1 || end == -1)
            {
                steps.Add("Node khong ton tai");
                return;
            }

            for (int k = 0; k < nVerts; k++)
                if (vertexList[k] != null)
                    vertexList[k].isInTree = false;

            vertexList[start].isInTree = true;
            nTree = 1;
            blinkingNodes.Add(start);

            steps.Add("Bat dau tu " + vertexList[start].label);

            for (int j = 0; j < nVerts; j++)
            {
                int tempDist = adjMatrix[start, j];
                sPath[j] = new DistOriginal(start, tempDist);
            }
            sPath[start].distance = 0;

            while (nTree < nVerts)
            {
                int indexMin = GetMin();
                if (indexMin == -1)
                    break;

                int minDist = sPath[indexMin].distance;

                currentVert = indexMin;
                startToCurrent = minDist;

                if (minDist == INFINITY)
                {
                    break;
                }

                vertexList[currentVert].isInTree = true;
                nTree++;
                blinkingNodes.Add(currentVert);

                if (currentVert != start && sPath[currentVert] != null)
                {
                    int parent = sPath[currentVert].parentVert;
                    confirmedEdges.Add(new Tuple<int, int>(parent, currentVert));
                }

                steps.Add("Them " + vertexList[currentVert].label + " vao cay, khoang cach: " + minDist);

                if (currentVert == end)
                    break;

                AdjustShortPath();
            }

            PrintPath(start, end);
            ResetTree();
        }

        private int GetMin()
        {
            int minDist = INFINITY;
            int indexMin = -1;

            for (int j = 0; j < nVerts; j++)
            {
                if (vertexList[j] == null) continue;
                if (!vertexList[j].isInTree &&
                    sPath[j] != null &&
                    sPath[j].distance < minDist)
                {
                    minDist = sPath[j].distance;
                    indexMin = j;
                }
            }

            return indexMin;
        }

        private void AdjustShortPath()
        {
            for (int j = 0; j < nVerts; j++)
            {
                if (vertexList[j] == null) continue;
                if (vertexList[j].isInTree)
                    continue;

                int currentToFringe = adjMatrix[currentVert, j];

                if (currentToFringe == INFINITY)
                    continue;

                int startToFringe = startToCurrent + currentToFringe;

                if (sPath[j] == null || startToFringe < sPath[j].distance)
                {
                    sPath[j].parentVert = currentVert;
                    sPath[j].distance = startToFringe;
                    steps.Add("Cap nhat khoang cach den " + vertexList[j].label + ": " + startToFringe);
                }
            }
        }

        private void PrintPath(int start, int end)
        {
            if (sPath[end] == null || sPath[end].distance == INFINITY)
            {
                steps.Add("Khong ton tai duong di");
                return;
            }

            steps.Add("Do tre toi thieu: " + sPath[end].distance);
            string path = "Duong di: ";

            int current = end;
            string p = vertexList[current].label;

            List<int> pathNodes = new List<int>();
            pathNodes.Add(current);

            while (current != start)
            {
                current = sPath[current].parentVert;
                p = vertexList[current].label + " -> " + p;
                pathNodes.Add(current);
            }

            pathNodes.Reverse();
            finalPathEdges.Clear();
            for (int i = 0; i < pathNodes.Count - 1; i++)
            {
                int a = pathNodes[i];
                int b = pathNodes[i + 1];
                finalPathEdges.Add(new Tuple<int, int>(a, b));
            }

            path += p;
            steps.Add(path);
        }

        private void ResetTree()
        {
            nTree = 0;
            for (int j = 0; j < nVerts; j++)
                if (vertexList[j] != null)
                    vertexList[j].isInTree = false;
        }

        public string GetShortestPathString(string startLabel, string endLabel)
        {
            ShortestPath(startLabel, endLabel);
            return string.Join("\n", steps);
        }

        public string GetExplanation(string startLabel, string endLabel)
        {
            int start = GetIndex(startLabel);
            int end = GetIndex(endLabel);
            if (start == -1 || end == -1)
            {
                return "Vấn đề: Node nguồn hoặc đích không tồn tại.\r\nGiải quyết: Không thể tính toán.\r\nTổng chi phí hiện tại: N/A\r\nTổng chi phí dự trù: N/A\r\nĐường đi: N/A";
            }
            if (sPath[end] == null || sPath[end].distance == INFINITY)
            {
                return "Vấn đề: Không có đường đi từ " + startLabel + " đến " + endLabel + ".\r\nGiải quyết: Thuật toán Dijkstra không tìm thấy đường đi.\r\nTổng chi phí hiện tại: Vô cùng\r\nTổng chi phí dự trù: Vô cùng\r\nĐường đi: Không tồn tại";
            }
            List<int> pathNodes = new List<int>();
            int current = end;
            pathNodes.Add(current);
            while (current != start)
            {
                current = sPath[current].parentVert;
                pathNodes.Add(current);
            }
            pathNodes.Reverse();
            string path = string.Join(" -> ", pathNodes.ConvertAll(i => vertexList[i].label));
            int totalCost = sPath[end].distance;
            int projectedCost = (int)Math.Ceiling(totalCost * 1.1);
            string solution = "Sử dụng thuật toán Dijkstra để chọn đường đi có chi phí nhỏ nhất giữa hai node.";
            string explanation = "Vấn đề: Tìm đường đi ngắn nhất từ " + startLabel + " đến " + endLabel + " trong mạng LAN.\r\nGiải thích: Thuật toán Dijkstra khởi tạo khoảng cách ban đầu từ nguồn tới các đỉnh và lặp chọn đỉnh có khoảng cách nhỏ nhất chưa vào cây, sau đó cập nhật các khoảng cách.\r\nGiải pháp: " + solution + "\r\nTổng chi phí hiện tại: " + totalCost + "\r\nTổng chi phí dự trù (ước tính +10%): " + projectedCost + "\r\nĐường đi: " + path;
            return explanation;
        }

        public Vertex[] GetVertices()
        {
            return vertexList;
        }

        public int GetNVerts()
        {
            return nVerts;
        }

        public int[,] GetAdjMatrix()
        {
            return adjMatrix;
        }
    }

    public class DijkstraForm : Form
    {
        private Graph graph;
        private Label lblStart;
        private TextBox txtStart;
        private Label lblEnd;
        private TextBox txtEnd;
        private Button btnCalculate;
        private Label lblResult;
        private ListBox lstSteps;
        private Panel pnlGraph;
        private TextBox txtExplanation;
        private Timer timer;
        private int revealIndex = 0;
        private bool revealing = false;

        public DijkstraForm()
        {
            InitializeComponent();
            graph = new Graph();
            InitializeGraph();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        private void InitializeGraph()
        {
            graph.AddVertex("PC_A", new Point(150, 150), "PC");
            graph.AddVertex("PC_B", new Point(150, 250), "PC");
            graph.AddVertex("PC_C", new Point(150, 350), "PC");
            graph.AddVertex("Switch_1", new Point(350, 200), "Switch");
            graph.AddVertex("Switch_2", new Point(550, 200), "Switch");
            graph.AddVertex("Switch_3", new Point(750, 200), "Switch");
            graph.AddVertex("Router_1", new Point(950, 200), "Router");
            graph.AddVertex("Router_2", new Point(1150, 200), "Router");
            graph.AddVertex("Server_1", new Point(1350, 150), "Server");
            graph.AddVertex("Server_2", new Point(1350, 250), "Server");
            graph.AddVertex("AP_1", new Point(750, 100), "AP");
            graph.AddVertex("AP_2", new Point(750, 300), "AP");
            graph.AddVertex("Firewall", new Point(1150, 300), "Firewall");
            graph.AddVertex("NAS", new Point(1350, 350), "NAS");
            graph.AddVertex("Gateway", new Point(1550, 200), "Gateway");

            graph.AddEdge(0, 3, 5);
            graph.AddEdge(1, 3, 4);
            graph.AddEdge(2, 4, 6);
            graph.AddEdge(3, 4, 2);
            graph.AddEdge(4, 5, 3);
            graph.AddEdge(5, 6, 4);
            graph.AddEdge(6, 7, 2);
            graph.AddEdge(7, 12, 3);
            graph.AddEdge(12, 14, 2);
            graph.AddEdge(14, 9, 4);
            graph.AddEdge(6, 8, 5);
            graph.AddEdge(8, 13, 2);
            graph.AddEdge(13, 9, 3);
            graph.AddEdge(5, 10, 6);
            graph.AddEdge(10, 11, 2);
            graph.AddEdge(11, 4, 5);
        }

        private void InitializeComponent()
        {
            this.lblStart = new Label();
            this.txtStart = new TextBox();
            this.lblEnd = new Label();
            this.txtEnd = new TextBox();
            this.btnCalculate = new Button();
            this.lblResult = new Label();
            this.lstSteps = new ListBox();
            this.pnlGraph = new Panel();
            this.txtExplanation = new TextBox();
            this.timer = new Timer();

            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = new Size(1800, 1000);
            this.Name = "DijkstraForm";
            this.Text = "Dijkstra LAN Shortest Path";

            this.lblStart.Location = new Point(20, 10);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new Size(300, 20);
            this.lblStart.TabIndex = 0;
            this.lblStart.Text = "Nhập node nguồn (ví dụ: PC_A):";
            this.lblStart.Font = new Font("Arial", 10, FontStyle.Bold);

            this.txtStart.Location = new Point(20, 30);
            this.txtStart.Name = "txtStart";
            this.txtStart.Size = new Size(200, 20);
            this.txtStart.TabIndex = 1;
            this.txtStart.Text = "PC_A";
            this.txtStart.ForeColor = Color.Gray;
            this.txtStart.Enter += new EventHandler(this.txtStart_Enter);
            this.txtStart.Leave += new EventHandler(this.txtStart_Leave);

            this.lblEnd.Location = new Point(20, 60);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new Size(300, 20);
            this.lblEnd.TabIndex = 2;
            this.lblEnd.Text = "Nhập node đích (ví dụ: Server_1):";
            this.lblEnd.Font = new Font("Arial", 10, FontStyle.Bold);

            this.txtEnd.Location = new Point(20, 80);
            this.txtEnd.Name = "txtEnd";
            this.txtEnd.Size = new Size(200, 20);
            this.txtEnd.TabIndex = 3;
            this.txtEnd.Text = "Server_1";
            this.txtEnd.ForeColor = Color.Gray;
            this.txtEnd.Enter += new EventHandler(this.txtEnd_Enter);
            this.txtEnd.Leave += new EventHandler(this.txtEnd_Leave);

            this.btnCalculate.Location = new Point(20, 110);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new Size(200, 30);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "Tính đường đi";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new EventHandler(this.btnCalculate_Click);

            this.lblResult.Location = new Point(20, 150);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new Size(300, 50);
            this.lblResult.TabIndex = 5;
            this.lblResult.Text = "Kết quả sẽ hiển thị ở đây";
            this.lblResult.Font = new Font("Arial", 10, FontStyle.Bold);

            this.lstSteps.Location = new Point(20, 210);
            this.lstSteps.Name = "lstSteps";
            this.lstSteps.Size = new Size(300, 400);
            this.lstSteps.TabIndex = 6;

            this.pnlGraph.Location = new Point(350, 20);
            this.pnlGraph.Name = "pnlGraph";
            this.pnlGraph.Size = new Size(1400, 600);
            this.pnlGraph.BackColor = Color.White;
            this.pnlGraph.Paint += new PaintEventHandler(this.pnlGraph_Paint);
            this.pnlGraph.AutoScroll = true;
            this.pnlGraph.AutoScrollMinSize = new Size(1700, 700);
            this.pnlGraph.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.txtExplanation.Location = new Point(20, 650);
            this.txtExplanation.Name = "txtExplanation";
            this.txtExplanation.Size = new Size(1700, 250);
            this.txtExplanation.TabIndex = 7;
            this.txtExplanation.Text = "Giải thích kết quả sẽ hiển thị ở đây";
            this.txtExplanation.Font = new Font("Arial", 10, FontStyle.Regular);
            this.txtExplanation.BorderStyle = BorderStyle.FixedSingle;
            this.txtExplanation.Multiline = true;
            this.txtExplanation.ScrollBars = ScrollBars.Both;
            this.txtExplanation.ReadOnly = true;
            this.txtExplanation.WordWrap = false;
            this.txtExplanation.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            this.timer.Interval = 500;
            this.timer.Tick += new EventHandler(this.timer_Tick);

            this.Controls.Add(this.txtExplanation);
            this.Controls.Add(this.pnlGraph);
            this.Controls.Add(this.lstSteps);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.lblEnd);
            this.Controls.Add(this.txtEnd);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.txtStart);
        }

        private void txtStart_Enter(object sender, EventArgs e)
        {
            if (txtStart.Text == "PC_A")
            {
                txtStart.Text = "";
                txtStart.ForeColor = Color.Black;
            }
        }

        private void txtStart_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStart.Text))
            {
                txtStart.Text = "PC_A";
                txtStart.ForeColor = Color.Gray;
            }
        }

        private void txtEnd_Enter(object sender, EventArgs e)
        {
            if (txtEnd.Text == "Server_1")
            {
                txtEnd.Text = "";
                txtEnd.ForeColor = Color.Black;
            }
        }

        private void txtEnd_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEnd.Text))
            {
                txtEnd.Text = "Server_1";
                txtEnd.ForeColor = Color.Gray;
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                string start = txtStart.Text.Trim();
                string end = txtEnd.Text.Trim();

                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                {
                    lblResult.Text = "Vui lòng nhập đầy đủ node nguồn và đích.";
                    return;
                }

                lblResult.Text = graph.GetShortestPathString(start, end);
                lstSteps.Items.Clear();
                foreach (string step in graph.steps)
                {
                    lstSteps.Items.Add(step);
                }
                txtExplanation.Text = graph.GetExplanation(start, end);

                revealIndex = 0;
                revealing = false;
                if (graph.confirmedEdges != null && graph.confirmedEdges.Count > 0)
                {
                    revealing = true;
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                    pnlGraph.Invalidate();
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
            }
        }

        private void pnlGraph_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen = new Pen(Color.Black, 2);
            Font font = new Font("Arial", 8);
            Font labelFont = new Font("Arial", 7);

            Vertex[] vertices = graph.GetVertices();
            int nVerts = graph.GetNVerts();
            int[,] adjMatrix = graph.GetAdjMatrix();

            Point offset = pnlGraph.AutoScrollPosition;

            bool EdgeMatchesPair(int a, int b, Tuple<int, int> t)
            {
                return (t.Item1 == a && t.Item2 == b) || (t.Item1 == b && t.Item2 == a);
            }

            bool IsEdgeConfirmed(int a, int b, int revealCount)
            {
                if (graph.confirmedEdges == null) return false;
                int limit = Math.Min(revealCount, graph.confirmedEdges.Count);
                for (int k = 0; k < limit; k++)
                {
                    if (EdgeMatchesPair(a, b, graph.confirmedEdges[k]))
                        return true;
                }
                return false;
            }

            bool IsEdgeInFinalPath(int a, int b)
            {
                if (graph.finalPathEdges == null) return false;
                for (int k = 0; k < graph.finalPathEdges.Count; k++)
                {
                    if (EdgeMatchesPair(a, b, graph.finalPathEdges[k]))
                        return true;
                }
                return false;
            }

            for (int i = 0; i < nVerts; i++)
            {
                if (vertices[i] == null) continue;
                for (int j = i + 1; j < nVerts; j++)
                {
                    if (vertices[j] == null) continue;
                    if (adjMatrix[i, j] != 1000000)
                    {
                        Point p1 = new Point(vertices[i].position.X + offset.X, vertices[i].position.Y + offset.Y);
                        Point p2 = new Point(vertices[j].position.X + offset.X, vertices[j].position.Y + offset.Y);
                        Point mid = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                        bool confirmed = IsEdgeConfirmed(i, j, revealIndex);
                        bool inFinalPath = IsEdgeInFinalPath(i, j);

                        if (inFinalPath && revealIndex >= graph.confirmedEdges.Count && graph.finalPathEdges.Count > 0)
                        {
                            using (Pen finalPen = new Pen(Color.DarkBlue, 4))
                            {
                                finalPen.LineJoin = LineJoin.Round;
                                g.DrawLine(finalPen, p1, p2);
                            }
                            g.DrawString(adjMatrix[i, j].ToString(), font, Brushes.Black, mid);
                        }
                        else if (confirmed)
                        {
                            using (Pen confirmedPen = new Pen(Color.Black, 2))
                            {
                                g.DrawLine(confirmedPen, p1, p2);
                            }
                            g.DrawString(adjMatrix[i, j].ToString(), font, Brushes.Black, mid);
                        }
                        else
                        {
                            using (Pen fadedPen = new Pen(Color.FromArgb(70, Color.Black), 2))
                            {
                                g.DrawLine(fadedPen, p1, p2);
                            }
                            using (SolidBrush fadedBrush = new SolidBrush(Color.FromArgb(90, Color.Black)))
                            {
                                g.DrawString(adjMatrix[i, j].ToString(), font, fadedBrush, mid);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < nVerts; i++)
            {
                Vertex v = vertices[i];
                if (v == null) continue;

                Brush baseBrush = GetBrushForType(v.type);

                Point pos = new Point(v.position.X + offset.X, v.position.Y + offset.Y);

                int size = 40;
                Rectangle rect;

                switch (v.type)
                {
                    case "PC":
                        int w = 50;
                        int h = 40;
                        rect = new Rectangle(pos.X - w / 2, pos.Y - h / 2, w, h);
                        g.FillRectangle(baseBrush, rect);
                        g.DrawRectangle(pen, rect);
                        break;
                    case "Server":
                        rect = new Rectangle(pos.X - size / 2, pos.Y - size / 2, size, size);
                        g.FillRectangle(baseBrush, rect);
                        g.DrawRectangle(pen, rect);
                        break;
                    case "Switch":
                        rect = new Rectangle(pos.X - 24, pos.Y - 18, 48, 36);
                        using (GraphicsPath path = CreateRoundedRect(rect, 8))
                        {
                            g.FillPath(baseBrush, path);
                            g.DrawPath(pen, path);
                        }
                        break;
                    case "Router":
                        Point[] tri = new Point[]
                        {
                            new Point(pos.X - 20, pos.Y - 20),
                            new Point(pos.X - 20, pos.Y + 20),
                            new Point(pos.X + 20, pos.Y)
                        };
                        g.FillPolygon(baseBrush, tri);
                        g.DrawPolygon(pen, tri);
                        break;
                    case "Gateway":
                        Point[] diamondBase = new Point[]
                        {
                            new Point(pos.X, pos.Y - 20),
                            new Point(pos.X + 20, pos.Y),
                            new Point(pos.X, pos.Y + 20),
                            new Point(pos.X - 20, pos.Y)
                        };
                        g.FillPolygon(baseBrush, diamondBase);
                        g.DrawPolygon(pen, diamondBase);
                        break;
                    case "AP":
                    case "NAS":
                    case "Firewall":
                        rect = new Rectangle(pos.X - size / 2, pos.Y - size / 2, size, size);
                        g.FillEllipse(baseBrush, rect);
                        g.DrawEllipse(pen, rect);
                        break;
                    default:
                        rect = new Rectangle(pos.X - size / 2, pos.Y - size / 2, size, size);
                        g.FillEllipse(baseBrush, rect);
                        g.DrawEllipse(pen, rect);
                        break;
                }

                SizeF labelSize = g.MeasureString(v.label, labelFont);
                g.DrawString(v.label, labelFont, Brushes.Black, pos.X - labelSize.Width / 2, pos.Y + size / 2 + 2);

                baseBrush.Dispose();
            }

            pen.Dispose();
        }

        private GraphicsPath CreateRoundedRect(Rectangle r, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            if (diameter > r.Width) diameter = r.Width;
            if (diameter > r.Height) diameter = r.Height;
            Rectangle arc = new Rectangle(r.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = r.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = r.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = r.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Brush GetBrushForType(string type)
        {
            switch (type)
            {
                case "PC":
                    return new SolidBrush(Color.LightBlue);
                case "Switch":
                    return new SolidBrush(Color.LightGreen);
                case "Router":
                    return new SolidBrush(Color.Orange);
                case "Server":
                    return new SolidBrush(Color.LightSalmon);
                case "AP":
                    return new SolidBrush(Color.LightPink);
                case "Firewall":
                    return new SolidBrush(Color.Red);
                case "NAS":
                    return new SolidBrush(Color.MediumPurple);
                case "Gateway":
                    return new SolidBrush(Color.Gold);
                default:
                    return new SolidBrush(Color.Gray);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!revealing)
            {
                timer.Stop();
                return;
            }

            revealIndex++;
            if (revealIndex >= graph.confirmedEdges.Count)
            {
                revealIndex = graph.confirmedEdges.Count;
                revealing = false;
                timer.Stop();
            }

            pnlGraph.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            base.OnFormClosing(e);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DijkstraForm());
        }
    }
}
