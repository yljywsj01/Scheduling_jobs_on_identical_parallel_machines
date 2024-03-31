using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace algorithm
{
    public partial class Form1 : Form
    {
        List<Machine> machines;
        List<Job> jobs;
        Random rand;
        int c = 0;

        class Job
        {
            public int start_time;
            public int process_time;
            public bool isScheduled = false;
            public int getFinishTime()
            {
                return start_time + process_time;
            }
        };

        class Machine
        {
            public List<int> myJobs;
            public Machine(List<int> jobs)
            {
                myJobs = jobs;
            }

            public int getLast()
            {
                return myJobs[myJobs.Count - 1];
            }
        };

        public Form1()
        {
            InitializeComponent();
            rand = new Random() ;

            reset();
            Image img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = img;
            randomSchedule();
            draw();
        }

        void reset()
        {
            int numJob = 10;
            int numMachine = 5;
            machines = new List<Machine>();
            jobs = new List<Job>();
            for (int i = 0; i < numJob; i++)
            {
                Job job = new Job();
                job.process_time = rand.Next(1, 10);
                jobs.Add(job);
            }
            jobs = jobs.OrderByDescending(t => t.process_time)
                .ThenByDescending(x => x.start_time).ToList();
            for (int i = 0; i < numMachine; i++)
            {
                Machine machine = new Machine(new List<int>());
                machines.Add(machine);
            }
        }


        public void Cmax()
        {
            int ans = 0;
            for (int i= 0; i < machines.Count; i++)
            {
                int numJob = machines[i].myJobs.Count;
                if (numJob > 0 && (ans < jobs[machines[i].getLast()].getFinishTime()))
                    ans = jobs[machines[i].getLast()].getFinishTime();
            }
            label2.Text = ans.ToString();
        }

        public void randomSchedule()
        {
            for (int i = 0; i < machines.Count; i++)
                machines[i].myJobs.Clear();
            for (int i = 0; i < jobs.Count; i++)
            {
                jobs[i].isScheduled = true;
                int idx = rand.Next(0, 5);
                int numJob = machines[idx].myJobs.Count;
                if (numJob > 0)
                    jobs[i].start_time = jobs[machines[idx].myJobs[numJob - 1]].getFinishTime();
                else
                    jobs[i].start_time = 0;
                machines[idx].myJobs.Add(i);
            }

            Cmax();
        }

        private void DrawRectangle(int x)
        {
            /*using (Graphics g = this.CreateGraphics())
            using (Pen p = new Pen(Color.Black))
            using (SolidBrush b = new SolidBrush(Color.LightBlue))
            {
                Rectangle rec = new Rectangle(50, x, 80, 20);

                p.Width = 3;
                p.DashStyle = DashStyle.Solid;
                g.DrawRectangle(p, rec);
                g.FillRectangle(b, rec);
            }*/
            

        }

        private void DrawSheet()
        {
            using (Graphics g = this.CreateGraphics())

            //using (Pen p = new Pen(Color.Black))
            using (SolidBrush b = new SolidBrush(Color.LightBlue))
            {
                g.DrawLine(new Pen(Color.Black, 3), 50, 18, 50, 250);
                g.DrawLine(new Pen(Color.Black, 3), 50, 250, 550, 250);
            }
        }

        private void draw()
        {
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            //固定machine的數量跟jobs的數量
            int unitHeight = 40;
            int unitWidth = 25;

            Pen pen = new Pen(Color.Black, 3);

            int x;
            Point bottomLeft = new Point(pictureBox1.Location.X - 25, pictureBox1.Location.Y + pictureBox1.Height - 50);
            g.DrawLine(pen, bottomLeft.X, bottomLeft.Y, bottomLeft.X, bottomLeft.Y - 500);
            g.DrawLine(pen, bottomLeft.X, bottomLeft.Y, bottomLeft.X + 1000, bottomLeft.Y );

            Brush brush = new SolidBrush(pen.Color);
            for (int i = 0; bottomLeft.X + i * unitWidth <= pictureBox1.Location.X + pictureBox1.Width; i++)
            {
                int xPos = bottomLeft.X + i * unitWidth;
                g.DrawString(i.ToString(), this.Font, brush, xPos, bottomLeft.Y + 10);
            }

            for (int i = 0; i < machines.Count; i++)
            {
                for (int j = 0; j < machines[i].myJobs.Count; j++)
                {
                    if (jobs[machines[i].myJobs[j]].isScheduled == false)
                        continue;
                    if (j != 0)
                        x = bottomLeft.X + jobs[machines[i].myJobs[j - 1]].getFinishTime() * unitWidth;
                    else
                        x = bottomLeft.X;
                    Rectangle rectangle = new Rectangle(x, bottomLeft.Y - (i + 1) * unitHeight-15*(i+1), jobs[machines[i].myJobs[j]].process_time * unitWidth, unitHeight);
                    g.DrawRectangle(new Pen(Color.Black, 3), rectangle);
                }
            }
            pictureBox1.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /*
            base.OnPaint(e);

            for(int i = 1; i <= 5; i++)
            {
                DrawRectangle(i*40);
            }
            DrawSheet();
            draw();*/

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) //Random
        {
            timer2.Enabled = false;
            timer1.Enabled = false;
            randomSchedule();
            draw();
        }

        private void button2_Click(object sender, EventArgs e) //Local
        {
            //randomSchedule();
            //draw();
            timer2.Enabled = false;
            timer1.Enabled = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            local_search(sender);
        }

        private void local_search(object sender)
        {
            Timer timer = (Timer)sender;

            int idx = 0;
            int max_time = 0;
            int lastTime=0;
            //找到最後結束的Job
            for(int i = 0; i < machines.Count; i++)
            {
                if (machines[i].myJobs.Count > 0)
                    lastTime = jobs[machines[i].getLast()].getFinishTime();
                else
                    lastTime = 0;
                if (lastTime > max_time)
                {
                    idx = i;
                    max_time = lastTime;
                }
            }

            int min_time = jobs[machines[idx].getLast()].start_time;
            int move = 0;
            //找到最早閒置的其他機器
            for (int i = 0; i < machines.Count; i++)
            {
                if (i != idx)
                {
                    if (machines[i].myJobs.Count > 0)
                        lastTime = jobs[machines[i].getLast()].getFinishTime();
                    else
                        lastTime = 0;
                    if (lastTime < min_time)
                    {
                        move = i;
                        min_time = lastTime;
                    }
                }
            }
            if (min_time == jobs[machines[idx].getLast()].start_time)  //找不到更優解 即停止
            { 
                timer.Enabled = false;
                return;
            }    

            jobs[machines[idx].getLast()].start_time = min_time;
            machines[move].myJobs.Add(machines[idx].getLast());
            machines[idx].myJobs.RemoveAt(machines[idx].myJobs.Count - 1);

            draw();
            Cmax();
        }

        private void greedy(object sender)
        {
            Timer timer = (Timer)sender;
            int idx = 0;
            while (jobs[idx].isScheduled == true)
                idx++;
            int lastTime = 0;
            int min_time = int.MaxValue;
            int move = 0;
            //找到最早閒置的其他機器
            for (int i = 0; i < machines.Count; i++)
            {
                if (machines[i].myJobs.Count > 0)
                    lastTime = jobs[machines[i].getLast()].getFinishTime();
                else
                    lastTime = 0;
                if (lastTime < min_time)
                {
                    move = i;
                    min_time = lastTime;
                }
            }
            jobs[idx].isScheduled = true;
            jobs[idx].start_time = min_time;
            machines[move].myJobs.Add(idx);
            draw();
            if (idx == jobs.Count - 1)
            {
                timer2.Enabled = false;
                Cmax();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)  //Greedy
        {
            timer1.Enabled = false;
            for (int i = 0; i < jobs.Count; i++)
                jobs[i].isScheduled = false;
            for (int i = 0; i < machines.Count; i++)
                machines[i].myJobs.Clear(); 
            draw();
            label2.Text = "";
            timer2.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            greedy(sender);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            reset();
            randomSchedule();
            //draw();
        }
    }
}
