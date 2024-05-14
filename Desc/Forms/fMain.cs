using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Desc.Classes;
using Newtonsoft.Json;

namespace Desc
{
    public partial class fMain : Form
    {
        public List<User> allUsers = new List<User>();
        public List<Project> lstProjects = new List<Project>();
        public List<Classes.Task> currentProjectTasks = new List<Classes.Task>();
        public static int selectedProjectId = -1;
        public int version = 1;

        int w = -1;
        public fMain()
        {
            InitializeComponent();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            dgv.Width = dgv.Width * 2;
            btnAdd.Left = (btnAdd.Left * 2) + 150;
            w = btnAdd.Left;
            lastWidth = dgv.Width;
            getProjects();

            if (File.Exists("selected.txt"))
            {
                StreamReader sr = File.OpenText("selected.txt");
                string s = sr.ReadLine();
                string s2 = sr.ReadLine();
                string[] strs = s2.Split('.');
                version = int.Parse(strs[2]);
                tabControl.SelectTab(int.Parse(s));
                LblVersion.Text = s2;
                sr.Close();
            }


        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter sw = File.CreateText("selected.txt");
            sw.WriteLine(tabControl.SelectedIndex);
            version++;
            sw.WriteLine("1.0."+(version));
            sw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl.SelectTab(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl.SelectTab(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl.SelectTab(2);
        }

        public void getProjects()
        { 
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> tskMsg = client.GetAsync("https://localhost:44321/api/Projects");
            tskMsg.Wait();
            HttpResponseMessage response = tskMsg.Result;
            Task<String> tr = response.Content.ReadAsStringAsync();
            tr.Wait();
            string json = tr.Result;
            lstProjects = JsonConvert.DeserializeObject<List<Project>>(json);
            selectedProjectId = lstProjects[0].IdProject;

            foreach (Project project in lstProjects)
            {
                Button btnProj = new Button();
                btnProj.Size = new Size(75,75);
                btnProj.Click += BtnProj_Click;
                btnProj.Tag = project.IdProject;
                if (project.FullTitle.Split(' ').Count() > 1)
                {
                    string[] ms = project.FullTitle.Split(' ');
                    string w1 = ms[0];
                    string w2 = ms[1];
                    btnProj.Text = w1[0] + w2[0].ToString();
                }
                else
                {
                    btnProj.Text = project.FullTitle[0]+project.FullTitle[1].ToString();
                }
                flowLayoutPanel1.Controls.Add(btnProj);
            }
        }

        private void BtnProj_Click(object sender, EventArgs e)
        {
            selectedProjectId = int.Parse(((Button)sender).Tag.ToString());
            tabControl_Selected(null, null);
            tbxSearch.Text = "";
            if (dgv.Width != lastWidth)
            {
                dgv.Width = lastWidth;
                btnAdd.Left = w;
                isCliced = false;
            }
        }

        private void tabControl_TabIndexChanged(object sender, EventArgs e)
        {
            
        }

        public void loadTasks(int idProject)
        {
            currentProjectTasks.Clear();
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> tskMsg = client.GetAsync("https://localhost:44321/api/Tasks");
            tskMsg.Wait();
            HttpResponseMessage response = tskMsg.Result;
            Task<String> tr = response.Content.ReadAsStringAsync();
            tr.Wait();
            string json = tr.Result;
            List<Classes.Task> allTasks = JsonConvert.DeserializeObject<List<Classes.Task>>(json);

            currentProjectTasks.AddRange(allTasks.Where(p => (p.ProjectId == selectedProjectId) && (p.StatusId == 1 && (p.Deadline.Subtract(DateTime.Now).TotalDays>0))));
            currentProjectTasks.AddRange(allTasks.Where(p => (p.ProjectId == selectedProjectId) && (p.StatusId == 3 && (p.Deadline.Subtract(DateTime.Now).TotalDays > 0))));
            currentProjectTasks.AddRange(allTasks.Where(p => (p.ProjectId == selectedProjectId) && (p.StatusId == 1 && (p.Deadline.Subtract(DateTime.Now).TotalDays <= 0))));
            currentProjectTasks.AddRange(allTasks.Where(p => (p.ProjectId == selectedProjectId) && (p.StatusId == 3 && (p.Deadline.Subtract(DateTime.Now).TotalDays <= 0))));
            currentProjectTasks.AddRange(allTasks.Where(p => (p.ProjectId == selectedProjectId) && (p.StatusId == 2)));
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
        }

        public void getUsers()
        {
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> tskMsg = client.GetAsync("https://localhost:44321/api/Users");
            tskMsg.Wait();
            HttpResponseMessage response = tskMsg.Result;
            Task<String> tr = response.Content.ReadAsStringAsync();
            tr.Wait();
            string json = tr.Result;
            allUsers = JsonConvert.DeserializeObject<List<Classes.User>>(json);
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl.SelectedIndex == 1)
            {
                dgv.Rows.Clear();
                loadTasks(selectedProjectId);
                getUsers();
                getStatuses();
                for (int i = 0; i<currentProjectTasks.Count;i++)
                {
                    dgv.Rows.Add();
                    DataGridViewRow row = dgv.Rows[i];
                    ColorConverter converter = new ColorConverter();
                    row.DefaultCellStyle.BackColor = (Color)converter.ConvertFromString(allStatuses.First(s => s.IdStatus == currentProjectTasks[i].StatusId).ColorHex);
                    row.Cells[4].Value = (allStatuses.First(s => s.IdStatus == currentProjectTasks[i].StatusId).ColorHex);
                    row.Cells[0].Value = currentProjectTasks[i].FullTitle;
                    row.Cells[1].Value = allUsers.Where(u => u.IdUser == currentProjectTasks[i].ExecutiveEmployeeId).ToList()[0].Name;
                    row.Cells[2].Value = currentProjectTasks[i].Deadline.ToString("dd.MM.yyyy");
                    row.Cells[3].Value = currentProjectTasks[i].IdTask;
                }
                this.Refresh();
                dgv.Refresh();
            }
        }

        int lastWidth;
        bool isCliced = false;

        public List<Status> allStatuses = new List<Status>();

        public void getStatuses()
        {
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> tskMsg = client.GetAsync("https://localhost:44321/api/TaskStatus");
            tskMsg.Wait();
            HttpResponseMessage response = tskMsg.Result;
            Task<String> tr = response.Content.ReadAsStringAsync();
            tr.Wait();
            string json = tr.Result;
            allStatuses = JsonConvert.DeserializeObject<List<Classes.Status>>(json);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                if (!isCliced)
                {
                    btnAdd.Left = (dgv.Left / 2) + 150;
                    dgv.Width = dgv.Width / 2;
                }

                btnAddt.Visible = false;
                isCliced = true;

                int id = int.Parse(dgv.Rows[e.RowIndex].Cells[3].Value.ToString());
                Classes.Task t = currentProjectTasks.First(p => p.IdTask == id);
                tbxFullName.Text = t.FullTitle;
                tbxShort.Text = t.ShortTitle;
                tbxDescription.Text = t.Description;
                cbxExec.Items.Add(allUsers.First(u => u.IdUser == t.ExecutiveEmployeeId).Name);
                cbxExec.SelectedIndex = 0;
                cbxExec.Enabled = false;
                cbxStatus.Items.Add(allStatuses.First(s => s.IdStatus == t.StatusId).Name);
                cbxStatus.SelectedIndex = 0;
                cbxStatus.Enabled = false;

                tbxFullName.Enabled = false;
                tbxShort.Enabled = false;
                tbxDescription.Enabled = false;

                try
                {
                    cbxLastTask.Items.Add(currentProjectTasks.First(cp => cp.PreviousTaskId == t.IdTask).FullTitle);
                    cbxLastTask.SelectedIndex = 0;
                    cbxLastTask.Enabled = false;
                }
                catch
                {
                    cbxLastTask.Items.Clear();
                    cbxLastTask.Enabled = false;
                }

                this.Refresh();
            }
        }

        private void dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ColorConverter converter = new ColorConverter();
            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = (Color)converter.ConvertFromString(dgv.Rows[e.RowIndex].Cells[4].Value.ToString());
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            isCliced = false;
            btnAdd.Left = w;
            dgv.Width = lastWidth;
        }

        private void tbxSearch_TextChanged(object sender, EventArgs e)
        {
            if (tbxSearch.Text != "")
            {
                dgv.Rows.Clear();
                List<Classes.Task> sortedTasks = currentProjectTasks.FindAll(p => p.FullTitle.Contains(tbxSearch.Text) || p.Description.Contains(tbxSearch.Text));
                foreach (Classes.Task t in sortedTasks)
                {
                    int i = dgv.Rows.Add();
                    DataGridViewRow row = dgv.Rows[i];
                    ColorConverter converter = new ColorConverter();
                    row.DefaultCellStyle.BackColor = (Color)converter.ConvertFromString(allStatuses.First(s => s.IdStatus == sortedTasks[i].StatusId).ColorHex);
                    row.Cells[4].Value = (allStatuses.First(s => s.IdStatus == sortedTasks[i].StatusId).ColorHex);
                    row.Cells[0].Value = sortedTasks[i].FullTitle;
                    row.Cells[1].Value = allUsers.Where(u => u.IdUser == sortedTasks[i].ExecutiveEmployeeId).ToList()[0].Name;
                    row.Cells[2].Value = sortedTasks[i].Deadline.ToString("dd.MM.yyyy");
                    row.Cells[3].Value = sortedTasks[i].IdTask;
                }
            }
            else
            {
                tabControl_Selected(null,null);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!isCliced)
            {
                btnAdd.Left = (dgv.Left / 2) + 150;
                dgv.Width = dgv.Width / 2;
            }
            btnAddt.Visible = true;

            isCliced = true;

            tbxFullName.Text = "";
            tbxDescription.Text = "";
            tbxShort.Text = "";
            cbxExec.Items.Clear();
            cbxLastTask.Items.Clear();
            cbxStatus.Items.Clear();

            tbxFullName.Enabled = true;
            tbxDescription.Enabled = true;
            tbxShort.Enabled = true;
            cbxStatus.Enabled = true;
            cbxExec.Enabled = true;
            cbxLastTask.Enabled = true;

            getStatuses();
            getUsers();
            loadTasks(selectedProjectId);

            foreach (Classes.Task t in currentProjectTasks)
            { 
                cbxLastTask.Items.Add(t.FullTitle);
            }

            foreach (Classes.Status s in allStatuses)
            {
                cbxStatus.Items.Add(s.Name);
            }

            foreach (Classes.User u in allUsers)
            {
                cbxExec.Items.Add(u.Name);
            }

        }

        private void btnAddt_Click(object sender, EventArgs e)
        {

            if (tbxShort.Text != "" && tbxFullName.Text != "" && tbxDescription.Text != "" && cbxExec.SelectedIndex != -1 && cbxStatus.SelectedIndex != -1)
            {
                Classes.Task tsk = new Classes.Task();

                if (cbxLastTask.SelectedIndex == -1)
                {
                    
                }
                else
                {
                    tsk.PreviousTaskId = currentProjectTasks.Find(p => p.FullTitle == cbxLastTask.SelectedItem.ToString()).IdTask;
                }

                tsk.FullTitle = tbxFullName.Text;
                tsk.ShortTitle = tbxShort.Text;
                tsk.Description = tbxDescription.Text;
                tsk.ProjectId = currentProjectTasks.First().ProjectId;
                tsk.ExecutiveEmployeeId = allUsers.Find(u => u.Name == cbxExec.SelectedItem.ToString()).IdUser;
                tsk.StatusId = allStatuses.Find(f => f.Name == cbxStatus.SelectedItem.ToString()).IdStatus;
                tsk.CreatedTime = DateTime.Now.ToString("hh:mm");

                createNewTask(tsk);
            }
            else
            {
                MessageBox.Show("Для того чтобы добавить новую задачу вы должны заполнить все поля");
            }
        }

        private void createNewTask(Classes.Task tsk)
        {
            HttpClient client = new HttpClient();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(tsk);
            StringContent content = new StringContent(json,Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> tskMsg = client.PostAsync("https://localhost:44321/api/Tasks",content);
            tskMsg.Wait();
            HttpResponseMessage response = tskMsg.Result;
            loadTasks(selectedProjectId);
            tabControl_Selected(null, null);
        }
    }
}
