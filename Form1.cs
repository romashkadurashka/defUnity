using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace defUnity
{
    public partial class Form1 : Form
    {
        //variables
        string path;
        string extractPath = @"temp";

        public Form1()
        {
            InitializeComponent();
            //init state
            progressBar1.Visible = false;
            lblInfo.Visible = false;
        }

        private void openSTUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //очищаем таблицу от предыдущих открытий, скрываем лишние элементы с формы
            dataGridView1.Rows.Clear();
            lblInfo.Visible = false;
            progressBar1.Value = 0;

            //выбираем папку с stu, формируем путь 
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                readFile(path);
            }

        }

        private void panel1_Click(object sender, EventArgs e)
        {
            //очищаем таблицу от предыдущих открытий, скрываем лишние элементы с формы
            dataGridView1.Rows.Clear();
            lblInfo.Visible = false;
            progressBar1.Value = 0;

            //выбираем папку с stu
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                readFile(path);
            }

        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            //очищаем таблицу от предыдущих открытий, скрываем лишние элементы с формы
            dataGridView1.Rows.Clear();
            progressBar1.Visible = true;
            lblInfo.Visible = false;

            //открываем файлы
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                lblInfo.Visible = true;

                string path = files[0];
                //string fullpath = path.Substring(path.LastIndexOf("\\"));

                //если открываемое - файл, то выдаем сообщение об ошибке
                if (System.IO.File.Exists(path))
                {
                    progressBar1.Visible = false;
                    lblInfo.ForeColor = Color.Red;
                    lblInfo.Text = "Ошибка, необходимо указать/выбрать папку, содержащую файлы *.STU";
                //если открываемое - папка, то передаем в readFile путь path
                }else if (System.IO.Directory.Exists(path))
                {
                    progressBar1.Visible = true;
                    readFile(path);
                }
            }
        }

        private void readFile(string path)
        {
            //среди открываемых папок ищем по маске *.stu во всех директориях
            string[] files = Directory.GetFiles(path, "*.stu", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                lblInfo.ForeColor = Color.Red;
                lblInfo.Text = "В папке '" + path + "' файлы *.STU не найдены";

            }
            else
            {
                lblInfo.ForeColor = Color.Black;
                lblInfo.Text = "Количество найденных файлов *.STU в папке '" + path + "': " + files.Length;
            
            }
            foreach (string item in files)
            {
                progressBar1.Visible = true;
                progressBar1.Maximum = files.Length + 1;
                if(progressBar1.Value < progressBar1.Maximum)
                {
                    progressBar1.Value++;
                }
                else
                {
                    progressBar1.Value = 0;
                }
                
                string fn = new FileInfo(item).Name;
                //dataGridView2.Rows.Add(fn, item);

                //распаковываем архив stu
                ZipFile.ExtractToDirectory(item, extractPath + "\\" + fn);

                FileStream stream = new FileStream(extractPath + "\\" + fn + "\\STATION.CTX", FileMode.Open);
                StreamReader reader = new StreamReader(stream);
                string str = reader.ReadToEnd();
                stream.Close();

                //парсим полученную строку из файла station.ctx
                String[] words = str.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                //выводим версию 
                if (words[1].Contains("Unity"))
                {
                    //выводим версию
                    string version = words[1].Substring(0, 20);

                    //заполняем таблицу
                    dataGridView1.Rows.Add(fn, version, item);
                }

                if (words[1].Contains("EcoStruxure"))
                {
                    //выводим версию текущего открытого файла
                    string version = words[1].Substring(0, 34);

                    //заполняем таблицу ранее открытых stu
                    //сокращаем название control expert для экономии места
                    string shortVersion = version.Replace("EcoStruxure Control Expert", "ES CE");

                    //заполняем таблицу
                    dataGridView1.Rows.Add(fn, shortVersion, item);
                }

                //удаляем временные файлы
                Directory.Delete(extractPath + "\\" + fn, true);
                
            }
            progressBar1.Visible = false;
            lblInfo.Visible = true;
            Directory.Delete(extractPath, true);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About formAbout = new About();
            formAbout.Show();
        }
    }
}
