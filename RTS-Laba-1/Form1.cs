using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTS_Laba_1
{
    public partial class Form1 : Form
    {
        int counter = 0;
        string bufer = "";
        Mutex mutex = new Mutex();

        Thread consume;
        Thread produce;

        public Form1()
        {
            InitializeComponent();

            produce = new Thread(new ThreadStart(Produce));
            consume = new Thread(new ThreadStart(Consume));
            consume.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            produce = new Thread(new ThreadStart(Produce));
            produce.Start();

            //consume = new Thread(new ThreadStart(Consume));
            //consume.Start();
        }

        void Produce()
        {
            mutex.WaitOne();

            counter++;
            bufer = $"Сообщение №{counter}";
            Invoke(new Action(() =>
            {
                textBox1.Text = bufer;
            }));
            Thread.Sleep(50);

            mutex.ReleaseMutex();
        }

        Queue<string> consumerBuffer = new Queue<string>(3);
        void Consume()
        {
            while (true)
            {
                var isAdd = false;

                mutex.WaitOne();
                if (bufer.Length > 0)
                {
                    if (consumerBuffer.Count < 3)
                    {
                        isAdd = true;
                        consumerBuffer.Enqueue(bufer);

                        // в UI потоке печатаем 
                        Invoke(new Action(() =>
                        {
                            listBox2.Items.Clear();
                            listBox2.Items.AddRange(consumerBuffer.ToArray());
                            textBox1.Text = "";
                        }));

                        bufer = "";
                    }
                }
                mutex.ReleaseMutex();

                if (isAdd)
                {
                    var t = new Thread(new ThreadStart(new Action(() => 
                    {
                        Thread.Sleep(1200);
                        Invoke(new Action(() =>
                        {
                            listBox1.Items.Add(consumerBuffer.Dequeue());

                            listBox2.Items.Clear();
                            listBox2.Items.AddRange(consumerBuffer.ToArray());
                        }));
                    })));
                    t.Start();
                }
            }

          
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            consume.Abort();
        }
    }
}
