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
        Mutex mutexConsumer = new Mutex();

        Task produce;
        Task consume;
        Semaphore semaphore;

        public Form1()
        {
            InitializeComponent();

            ThreadPool.SetMaxThreads(10, 10);
            semaphore = new Semaphore(3, 3);

            //запуск Consume в потоке consume из пула
            consume = Task.Run(Consume);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //запуск Produce в потоке produce из пула
            produce = Task.Run(Produce);
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
            while (!isClosed)
            {
                var isAdd = false;

                mutex.WaitOne();
                if (bufer.Length > 0)
                {
                    if (consumerBuffer.Count < 5)
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
                    Task.Run(() => 
                    {
                        semaphore.WaitOne();
                        Thread.Sleep(1200);
                        Invoke(new Action(() =>
                        {
                            mutexConsumer.WaitOne();
                            listBox1.Items.Add(consumerBuffer.Dequeue());
                            listBox2.Items.Clear();
                            listBox2.Items.AddRange(consumerBuffer.ToArray());
                            mutexConsumer.ReleaseMutex();
                        }));

                        semaphore.Release();
                    });
                }
            }

          
        }

        bool isClosed = false;

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            isClosed = true;
        }
    }
}
