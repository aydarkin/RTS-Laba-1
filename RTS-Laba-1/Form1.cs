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

        Thread consume;
        Thread produce;

        public Form1()
        {
            InitializeComponent();

            produce = new Thread(new ThreadStart(Produce));
            consume = new Thread(new ThreadStart(Consume));

            // запускаем потребителя
            
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            produce = new Thread(new ThreadStart(Produce));
            produce.Start();

            consume = new Thread(new ThreadStart(Consume));
            consume.Start();
        }

        void Produce()
        {
            lock (bufer)
            {
                counter++;
                bufer = $"Сообщение №{counter}";
                Invoke(new Action(() =>
                {
                    textBox1.Text = bufer;
                }));
            }
        }

        Queue<string> consumerBuffer = new Queue<string>(3);
        void Consume()
        {
            var isAdd = false;

            produce.Join();
            lock (bufer)
            {
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
                        }));

                        bufer = "";
                    }
                }
            }
            
            if (isAdd)
            {
                Thread.Sleep(1200);
                Invoke(new Action(() =>
                {
                    textBox1.Text = "";
                    listBox1.Items.Add(consumerBuffer.Dequeue());

                    listBox2.Items.Clear();
                    listBox2.Items.AddRange(consumerBuffer.ToArray());
                }));
            }
        }
    }
}
