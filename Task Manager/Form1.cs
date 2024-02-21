using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;

namespace Task_Manager
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null;// главный список в котором храняться все процессы 
        private ListViewitemComparer Comparer = null;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        // метод который работает только со списком бэкэндом 
        private void GetProcesses() // метод создает список и обновляет его 
        {
            processes.Clear();// очистка списка 
            processes = Process.GetProcesses().ToList<Process>();// заполнение списка заново получаем все системные процессы 

        }
        // метод который работает с листвью фронтендом 
        private void RefreshProcessesList()
        {
            try
            {
                listView1.Items.Clear();// очистим лист 
                double memSize = 0;// переменная при переборе всех процессов храниться память 
                foreach (Process p in processes) // с помощью цикла переберем все процессы 
                {
                    if (p != null)
                    {
                        memSize = 0;
                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = p.ProcessName;
                        memSize = (double)pc.NextValue() / (1000 * 1000);
                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };
                        listView1.Items.Add(new ListViewItem(row));
                        pc.Close();
                        pc.Dispose();
                    }


                }
                Text = "running processes:" + processes.Count.ToString();// текст для шапки формы 
                                                                         // Перегрузка метода фильтровать процессы по имени 


            }
            catch (Exception) { }
        }
        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            listView1.Items.Clear();// очистим лист 
            double memSize = 0;// переменная при переборе всех процессов храниться память 
            foreach (Process p in processes) // с помощью цикла переберем все процессы 
            {
                memSize = 0;
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;
                memSize = (double)pc.NextValue() / (1000 * 1000);
                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };
                listView1.Items.Add(new ListViewItem(row));
                pc.Close();
                pc.Dispose();

            }
            Text = $"running processes '{keyword}':" + processes.Count.ToString();// текст для шапки формы 


        }
        // метод для завершения одного процесса который мы выбрали 
        private void KillProcess(Process process)
        {
            process.Kill();
            process.WaitForExit();
        }
        // метод завершения дерева процессов 
        private void KillProcessAndChildrem(int pid)
        {
            if (pid == 0)
            {
                return; // проверка что пид не равен 0
            }
            // находит все связанные процессы по айди
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection objectCollection = searcher.Get();
            foreach (ManagementObject obj in objectCollection)
            {
                KillProcessAndChildrem(Convert.ToInt32(obj["ProcessID"]));
            }
            try
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit();
            }
            catch (ArgumentException)
            {

                
            }
        }
        // метод для получения айди родительского процесса
        private int GetParentProcessId(Process p)
        {
            int parentID = 0;
            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='"+ p.Id + "'");
                managementObject.Get();
                parentID = Convert.ToInt32(managementObject["ParentProcessId"]);



            }
            catch (Exception)
            {
                
            }
            return parentID;

        }

        private void Form1_Load(object sender, EventArgs e)// обработчик события в шапке 
        {
            processes = new List<Process>();// инициализация списка 
            GetProcesses();
            RefreshProcessesList();// заполняет собятия лист вью
            Comparer = new ListViewitemComparer();
            Comparer.ColumnIndex = 0;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)// обработчик кнопки обновить 
        {
            GetProcesses ();// вызываем 2 метода 
            RefreshProcessesList ();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)// обработчик кнопки завершить 
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcess (processToKill);
                    GetProcesses();
                    RefreshProcessesList();

                }
            }
            catch (Exception)
            {

                
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)// обработчик дерева процессов 
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcessAndChildrem(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception)
            {


            }
        }

        private void completeTheProcessTreeToolStripMenuItem_Click(object sender, EventArgs e)// обработчик дерева процессов 
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcessAndChildrem(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception)
            {


            }
        }

        private void startATaskToolStripMenuItem_Click(object sender, EventArgs e)// обработчик запустить задачу 
        {
            string path = Interaction.InputBox("enter the program name", "starting a new task");
            try
            {
                Process.Start(path);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();
            List<Process> filteredprocesses = processes.Where((x) =>
            x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();
            RefreshProcessesList(filteredprocesses, toolStripTextBox1.Text);


        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Comparer.ColumnIndex = e.Column;
            Comparer.SortDirection = Comparer.SortDirection == SortOrder.Ascending 
                ? SortOrder.Descending : SortOrder.Ascending;
            listView1.ListViewItemSorter = Comparer;
            listView1.Sort();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
