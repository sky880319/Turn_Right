using System;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Turn_Right
{
    public partial class Form1 : Form
    {
        private Point startPoint;
        bool isStart = false;
        int score = 0;
        int topScore = 0;
        int totalTime = 0;
        static int maxBox = 10;
        Random rd = new Random();
        System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
        Graphics g;
        SizeF boxSize = new SizeF(12,12);
        RectangleF[,] boxList = new RectangleF[4, maxBox];
        Color[,] boxColor = new Color[4, maxBox];
        float[] edge = { 0, 0, 0, 0 };
        float[] oriEdge = { 0, 0, 0, 0 };
        int[] boxCount = { 0, 0, 0, 0 };
        
        RectangleF box(PointF bPosition, SizeF bSize)
        {
            float x, y;
            x = bPosition.X - bSize.Width / 2;
            y = bPosition.Y - bSize.Height / 2;
            return new RectangleF(x, y, bSize.Width,bSize.Height);
        }

        RectangleF[] readyBox = new RectangleF[2];
        Color[] readyBoxColor = new Color[2];

        RectangleF runningBox = new RectangleF(0, 0, 0, 0);
        Color currentColor = Color.WhiteSmoke;
        int currentDir = -1;
        int colorVar = 4;

        void createBox()
        {
            isStart = true;
            currentDir = rd.Next(4);
            PointF startPosi = new PointF(0, 0);
            switch(currentDir) //方向
            {
                case 0: //上
                    startPosi = new PointF(panel2.Width / 2, boxSize.Width / 2);
                    break;
                case 1: //下
                    startPosi = new PointF(panel2.Width / 2, panel2.Width - boxSize.Width / 2);
                    break;
                case 2: //左
                    startPosi = new PointF(boxSize.Height / 2, panel2.Height / 2);
                    break;
                case 3: //右
                    startPosi = new PointF(panel2.Height - boxSize.Height / 2, panel2.Height / 2);
                    break;
            }

            Brush b = new SolidBrush(currentColor);

            currentColor = readyBoxColor[0];
            readyBoxColor[0] = readyBoxColor[1];
            readyBoxColor[1] = colorSelector(rd.Next(colorVar));
            b = new SolidBrush(readyBoxColor[0]);
            g.FillRectangle(b, readyBox[0]);
            b = new SolidBrush(readyBoxColor[1]);
            g.FillRectangle(b, readyBox[1]);

            runningBox = box(startPosi, boxSize);
            g.FillRectangle(b, runningBox);
            //MessageBox.Show("");
            timer1.Start();
        }

        private Color colorSelector(int n)
        {
            switch (n)
            {
                case 0:
                    return Color.Sienna;
                case 1:
                    return Color.DarkOrange;
                case 2:
                    return Color.DarkSeaGreen;
                case 3:
                    return Color.DodgerBlue;
                case 4:
                    return Color.LightPink;
                case 5:
                    return Color.DarkGray;
                default:
                    return panel1.BackColor;
            }
        }

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            label1.Text = this.Text;
            readyBox[0] = box(new PointF(24f, 60f), new SizeF(10, 10));
            readyBox[1] = box(new PointF(24f + boxSize.Width + 1, 60f), new SizeF(10, 10));
            if (File.Exists(Application.StartupPath + "\\score.tr"))
            {
                using (StreamReader sr = new StreamReader(Application.StartupPath + "\\score.tr"))
                {
                    topScore = int.Parse(sr.ReadLine());
                }
            }
            label5.Text = String.Format(label5.Text, topScore);
            init();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            if (!isStart)
            {
                init();
                Brush b = new SolidBrush(Color.Gainsboro);
                g.FillRectangle(b, readyBox[0]);
                g.FillRectangle(b, readyBox[1]);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = new Point(-e.X, -e.Y);

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                //新視窗的位置
                mousePos.Offset(startPoint.X, startPoint.Y);
                //改變視窗位置
                Location = mousePos;
            }
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void saveBox()
        {
            if ((boxCount[currentDir] != 0) && (currentColor == boxColor[currentDir, boxCount[currentDir] - 1]))
            {
                Brush b = new SolidBrush(panel2.BackColor);
                g.FillRectangle(b, runningBox);
                g.FillRectangle(b, boxList[currentDir, boxCount[currentDir] - 1]);
                boxCount[currentDir]--;
                switch (currentDir) //方向
                {
                    case 0: //上
                        edge[0] += boxSize.Height;
                        break;
                    case 1: //下
                        edge[1] -= boxSize.Height;
                        break;
                    case 2: //左
                        edge[2] += boxSize.Width;
                        break;
                    case 3: //右
                        edge[3] -= boxSize.Width;
                        break;
                }
                score += 2;
                label2.Text = "分數：" + score;
                createBox();
            }
            else
            {
                switch (currentDir) //方向
                {
                    case 0: //上
                        edge[0] -= boxSize.Height;
                        break;
                    case 1: //下
                        edge[1] += boxSize.Height;
                        break;
                    case 2: //左
                        edge[2] -= boxSize.Width;
                        break;
                    case 3: //右
                        edge[3] += boxSize.Width;
                        break;
                }
                if (boxCount[currentDir] + 1 > maxBox)
                {
                    gameOver();
                }
                else
                {
                    boxList[currentDir, boxCount[currentDir]] = runningBox;
                    boxColor[currentDir, boxCount[currentDir]] = currentColor;
                    boxCount[currentDir]++;
                    score++;
                    label2.Text = "分數：" + score;
                    createBox();
                }
            }
        }

        float speed = 1.0f;

        private void timer1_Tick(object sender, EventArgs e)
        {
            Brush b = new SolidBrush(panel2.BackColor);
            if (currentDir == 0)
            {
                if (runningBox.Y + runningBox.Height < edge[currentDir])
                {
                    g.FillRectangle(b, runningBox);
                    if (runningBox.Y + runningBox.Height > edge[currentDir] - speed)
                        runningBox.Y = edge[currentDir] - runningBox.Height;
                    else
                        runningBox.Y += speed;
                    b = new SolidBrush(currentColor);
                    g.FillRectangle(b, runningBox);
                }
                else
                {
                    timer1.Stop();
                    saveBox();
                }
            }
            else if (currentDir == 1)
            {
                if (runningBox.Y > edge[currentDir])
                {
                    g.FillRectangle(b, runningBox);
                    if (runningBox.Y < edge[currentDir] + speed)
                        runningBox.Y = edge[currentDir];
                    else
                        runningBox.Y -= speed;
                    b = new SolidBrush(currentColor);
                    g.FillRectangle(b, runningBox);
                }
                else
                {
                    timer1.Stop();
                    saveBox();
                }
            }
            else if (currentDir == 2)
            {
                if (runningBox.X + runningBox.Width < edge[currentDir])
                {
                    g.FillRectangle(b, runningBox);
                    if (runningBox.X + runningBox.Width > edge[currentDir] - speed)
                        runningBox.X = edge[currentDir] - runningBox.Width;
                    else
                        runningBox.X += speed;
                    b = new SolidBrush(currentColor);
                    g.FillRectangle(b, runningBox);
                }
                else
                {
                    timer1.Stop();
                    saveBox();
                }
            }
            else if (currentDir == 3)
            {
                if (runningBox.X > edge[currentDir])
                {
                    g.FillRectangle(b, runningBox);
                    if (runningBox.X < edge[currentDir] + speed)
                        runningBox.X = edge[currentDir];
                    else
                        runningBox.X -= speed;
                    b = new SolidBrush(currentColor);
                    g.FillRectangle(b, runningBox);
                }
                else
                {
                    timer1.Stop();
                    saveBox();
                }
            }
        }

        private void rotate()
        {
            sp.Stream = Properties.Resources.switch8;
            sp.Play();
            Brush b = new SolidBrush(panel2.BackColor);
            if (boxCount[0] > 0)
                g.FillRectangle(b, boxList[0, boxCount[0] - 1].X, boxList[0, boxCount[0] - 1].Y, boxList[0, boxCount[0] - 1].Width, boxList[0, boxCount[0] - 1].Height * (boxCount[0]));
            if (boxCount[1] > 0)
                g.FillRectangle(b, boxList[1, 0].X, boxList[1, 0].Y, boxList[1, 0].Width, boxList[1, 0].Height * boxCount[1]);
            if (boxCount[2] > 0)
                g.FillRectangle(b, boxList[2, boxCount[2] - 1].X, boxList[2, boxCount[2] - 1].Y, boxList[2, boxCount[2] - 1].Width * (boxCount[2]), boxList[2, boxCount[2] - 1].Height);
            if (boxCount[3] > 0)
                g.FillRectangle(b, boxList[3, 0].X, boxList[3, 0].Y, boxList[3, 0].Width * boxCount[3], boxList[3, 0].Height);

            RectangleF[] temp = new RectangleF[maxBox];
            Color[] tempC = new Color[maxBox];
            for (int j = 0; j < maxBox; j++)
            {
                temp[j] = boxList[0, j];
                boxList[0, j] = boxList[2, j];
                boxList[2, j] = boxList[1, j];
                boxList[1, j] = boxList[3, j];
                boxList[3, j] = temp[j];

                tempC[j] = boxColor[0, j];
                boxColor[0, j] = boxColor[2, j];
                boxColor[2, j] = boxColor[1, j];
                boxColor[1, j] = boxColor[3, j];
                boxColor[3, j] = tempC[j];
            }

            int tempB = boxCount[0];
            boxCount[0] = boxCount[2];
            boxCount[2] = boxCount[1];
            boxCount[1] = boxCount[3];
            boxCount[3] = tempB;

            edge[0] = (panel2.Height - boxSize.Height) / 2 - boxSize.Height * boxCount[0];
            edge[1] = (panel2.Height + boxSize.Height) / 2 + boxSize.Height * boxCount[1];
            edge[2] = (panel2.Width - boxSize.Width) / 2 - boxSize.Width * boxCount[2];
            edge[3] = (panel2.Width + boxSize.Width) / 2 + boxSize.Width * boxCount[3];

            for (int i = 0; i < 4; i++)
            {
                float x = 1, y = 1;
                switch (i)
                {
                    case 0:
                        x = 1;
                        y = -1;
                        break;
                    case 2:
                        x = -1;
                        y = -1;
                        break;
                    case 1:
                        x = -1;
                        y = 1;
                        break;
                    case 3:
                        x = 1;
                        y = 1;
                        break;
                }
                for (int j = 0; j < boxCount[i]; j++)
                {
                    boxList[i, j].X += boxSize.Width * (j + 1) * x;
                    boxList[i, j].Y += boxSize.Height * (j + 1) * y;
                    g.FillRectangle(new SolidBrush(boxColor[i, j]), boxList[i, j]);
                }

            }

            switch (currentDir) //方向
            {
                case 0: //上
                    if (oriEdge[0] - runningBox.Y - boxSize.Height < boxCount[0] * boxSize.Height)
                        gameOver();
                    break;
                case 1: //下
                    if (runningBox.Y - oriEdge[1] < boxCount[1] * boxSize.Height)
                        gameOver();
                    break;
                case 2: //左
                    if (oriEdge[2] - runningBox.X - boxSize.Width < boxCount[2] * boxSize.Width)
                        gameOver();
                    break;
                case 3: //右
                    if (runningBox.X - oriEdge[3] < boxCount[3] * boxSize.Width)
                        gameOver();
                    break;
            }
        }

        //private void rotate_r()
        //{
        //    sp.Stream = Properties.Resources.switch8;
        //    sp.Play();
        //    Brush b = new SolidBrush(panel2.BackColor);
        //    if (boxCount[0] > 0)
        //        g.FillRectangle(b, boxList[0, boxCount[0] - 1].X, boxList[0, boxCount[0] - 1].Y, boxList[0, boxCount[0] - 1].Width, boxList[0, boxCount[0] - 1].Height * (boxCount[0]));
        //    if (boxCount[1] > 0)
        //        g.FillRectangle(b, boxList[1, 0].X, boxList[1, 0].Y, boxList[1, 0].Width, boxList[1, 0].Height * boxCount[1]);
        //    if (boxCount[2] > 0)
        //        g.FillRectangle(b, boxList[2, boxCount[2] - 1].X, boxList[2, boxCount[2] - 1].Y, boxList[2, boxCount[2] - 1].Width * (boxCount[2]), boxList[2, boxCount[2] - 1].Height);
        //    if (boxCount[3] > 0)
        //        g.FillRectangle(b, boxList[3, 0].X, boxList[3, 0].Y, boxList[3, 0].Width * boxCount[3], boxList[3, 0].Height);

        //    RectangleF[] temp = new RectangleF[maxBox];
        //    Color[] tempC = new Color[maxBox];
        //    for (int j = 0; j < maxBox; j++)
        //    {
        //        temp[j] = boxList[0, j];
        //        boxList[0, j] = boxList[3, j];
        //        boxList[3, j] = boxList[1, j];
        //        boxList[1, j] = boxList[2, j];
        //        boxList[2, j] = temp[j];

        //        tempC[j] = boxColor[0, j];
        //        boxColor[0, j] = boxColor[3, j];
        //        boxColor[3, j] = boxColor[1, j];
        //        boxColor[1, j] = boxColor[2, j];
        //        boxColor[2, j] = tempC[j];
        //    }

        //    int tempB = boxCount[0];
        //    boxCount[0] = boxCount[3];
        //    boxCount[3] = boxCount[1];
        //    boxCount[1] = boxCount[2];
        //    boxCount[2] = tempB;

        //    edge[0] = (panel2.Height - boxSize.Height) / 2 - boxSize.Height * boxCount[0];
        //    edge[1] = (panel2.Height + boxSize.Height) / 2 + boxSize.Height * boxCount[1];
        //    edge[2] = (panel2.Width - boxSize.Width) / 2 - boxSize.Width * boxCount[2];
        //    edge[3] = (panel2.Width + boxSize.Width) / 2 + boxSize.Width * boxCount[3];

        //    for (int i = 0; i < 4; i++)
        //    {
        //        float x = 1, y = 1;
        //        switch (i)
        //        {
        //            case 0:
        //                x = -1;
        //                y = -1;
        //                break;
        //            case 3:
        //                x = 1;
        //                y = -1;
        //                break;
        //            case 1:
        //                x = 1;
        //                y = 1;
        //                break;
        //            case 2:
        //                x = -1;
        //                y = 1;
        //                break;
        //        }
        //        for (int j = 0; j < boxCount[i]; j++)
        //        {
        //            boxList[i, j].X += boxSize.Width * (j + 1) * x;
        //            boxList[i, j].Y += boxSize.Height * (j + 1) * y;
        //            g.FillRectangle(new SolidBrush(boxColor[i, j]), boxList[i, j]);
        //        }
        //    }

        //    switch (currentDir) //方向
        //    {
        //        case 0: //上
        //            if (oriEdge[0] - runningBox.Y - boxSize.Height < boxCount[0] * boxSize.Height)
        //                gameOver();
        //            break;
        //        case 1: //下
        //            if (runningBox.Y - oriEdge[1] < boxCount[1] * boxSize.Height)
        //                gameOver();
        //            break;
        //        case 2: //左
        //            if (oriEdge[2] - runningBox.X - boxSize.Width < boxCount[2] * boxSize.Width)
        //                gameOver();
        //            break;
        //        case 3: //右
        //            if (runningBox.X - oriEdge[3] < boxCount[3] * boxSize.Width)
        //                gameOver();
        //            break;
        //    }
        //}

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (!panel3.Visible)
            {
                if (!isStart)
                {
                    sp.Stream = Properties.Resources.click5;
                    sp.Play();
                    init();
                    playTimer.Start();
                    createBox();
                }
                else
                {
                    //if (e.Button == MouseButtons.Left)
                    {
                        rotate();
                    }
                    //else if(e.Button == MouseButtons.Right)
                    //{
                    //    rotate_r();
                    //}
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (!panel3.Visible)
                {
                    if (!isStart)
                    {
                        sp.Stream = Properties.Resources.click5;
                        sp.Play();
                        init();
                        playTimer.Start();
                        createBox();
                    }
                    else
                    {
                        //if (e.Button == MouseButtons.Left)
                        {
                            rotate();
                        }
                        //else if(e.Button == MouseButtons.Right)
                        //{
                        //    rotate_r();
                        //}
                    }
                }
            }
        }

        void init()
        {
            g = panel2.CreateGraphics();
            Brush b = new SolidBrush(panel2.BackColor);
            readyBoxColor[0] = colorSelector(rd.Next(colorVar));
            readyBoxColor[1] = colorSelector(rd.Next(colorVar));
            b = new SolidBrush(readyBoxColor[0]);
            g.FillRectangle(b, readyBox[0]);
            b = new SolidBrush(readyBoxColor[1]);
            g.FillRectangle(b, readyBox[1]);
            g.Clear(panel2.BackColor);
            b = new SolidBrush(Color.FromArgb(64, 64, 64));
            g.FillRectangle(b, box(new Point(panel2.Width / 2, panel2.Height / 2), boxSize));
            boxList = new RectangleF[4, maxBox];
            boxColor = new Color[4, maxBox];
            edge[0] = (panel2.Height - boxSize.Height) / 2;
            edge[1] = (panel2.Height + boxSize.Height) / 2;
            edge[2] = (panel2.Width - boxSize.Width) / 2;
            edge[3] = (panel2.Width + boxSize.Width) / 2;
            for (int i = 0; i < edge.Length; i++)
                oriEdge[i] = edge[i];
            boxCount = new int[4];
            score = 0;
            totalTime = 0;
            speed = 1.0f;
            colorVar = 4;
            label2.Text = "分數：" + score;
        }

        void gameOver()
        {
            timer1.Stop();
            sp.Stream = Properties.Resources.chipLay3;
            sp.Play();
            if (score > topScore)
            {
                label4.Text = "新紀錄保持者";
                topScore = score;
                using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\score.tr"))
                {
                    sw.Write(topScore);
                    sw.Close();
                }
            }
            else
            {
                label4.Text = "距最高分還差 " + (topScore - score);
            }
            label5.Text = String.Format("Top Score: {0}", topScore);
            panel3.Visible = true;
            playTimer.Stop();
        }

        private void playTimer_Tick(object sender, EventArgs e)
        {
            totalTime++;
            if (totalTime < 10)
                speed = 1.0f;
            else if (totalTime < 30)
                speed = 1.2f;
            else if (totalTime < 60)
                speed = 1.5f;
            else if (totalTime < 90)
                colorVar = 5;
            else if (totalTime < 120)
                speed = 1.85f;
            else if (totalTime < 180)
            {
                colorVar = 6;
                speed = 2.0f;
            }
            else if (totalTime >= 180)
                speed = 2.5f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sp.Stream = Properties.Resources.click5;
            sp.Play();
            panel3.Visible = false;
            isStart = false;
            this.Focus();
        }
    }
}