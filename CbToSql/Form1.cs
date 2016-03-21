/* Created By:  
 * Mell Rosandich
 * 2016-03-21
 * http://www.ourace.com
 * http://www.ourace.com/158-clipboard-to-sql
*/


using System;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;

namespace CbToSql
{
    public partial class Form1 : Form
    {
        FormState state = new FormState();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("lastused.xml"))
            {
                loadLastConfig("lastused.xml");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveConfig("lastused.xml");
        }

        private void button1_Click(object sender, EventArgs e)
        {

            DataObject o = (DataObject)Clipboard.GetDataObject();

            char[] Sepr = new char[] { '\t' };
            if (radioButton1.Checked == true)
            {
                Sepr = new char[] { '\t' };
            }
            if (radioButton2.Checked == true)
            {
                Sepr = new char[] { ',' };
            }

            if (o.GetDataPresent(DataFormats.Text))
            {
                if (myDataGridView.Rows.Count > 0)
                    myDataGridView.Rows.Clear();
                if (myDataGridView.Columns.Count > 0)
                    myDataGridView.Columns.Clear();

                bool columnsAdded = false;
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                textBox1.Text = pastedRows.Count().ToString();
                foreach (string pastedRow in pastedRows)
                {

                    string[] pastedRowCells = pastedRow.Split(Sepr);

                    // build first set of rows
                    if (!columnsAdded)
                    {
                        for (int i = 0; i < pastedRowCells.Length; i++)
                        {
                            if (checkBox2.Checked == true)
                            {
                                myDataGridView.Columns.Add("col" + i, "c" + i);
                            }
                            else {
                                myDataGridView.Columns.Add("col" + i, pastedRowCells[i]);
                            }
                        }

                        columnsAdded = true;
                        if (checkBox2.Checked == true)
                        {
                            myDataGridView.Rows.Add(pastedRowCells);
                        }
                        continue;
                    }

                    myDataGridView.Rows.Add(pastedRowCells);
                }
            }

        }//end click


        private void button2_Click(object sender, EventArgs e)
        {
            string FinalText = "";
            int CountX = 0;
            foreach (DataGridViewRow row in myDataGridView.Rows)
            {

                CountX++;
                string Pal = textBox3.Text;
                int hitval = 0;
                for (int x = 0; x < row.Cells.Count; x++)
                {
                    string tempval = (row.Cells[x].Value != null) ? row.Cells[x].Value.ToString() : "";
                    tempval = encSQL(tempval);
                    if (tempval != "null" && tempval != "''")
                    {
                        hitval = 1;
                    }
                    Pal = Pal.Replace("!c" + x.ToString() + "!", tempval);
                }
                if (hitval == 1)
                {
                    FinalText += Pal + Environment.NewLine;
                }
                textBox4.Text = CountX.ToString();
                Application.DoEvents();
            }

            textBox2.Text = FinalText;


        }//end click

        public string encSQL(string valIn)
        {
            if (checkBox4.Checked == true)
            {
                valIn = valIn.Trim();
            }

            if (valIn == "")
            {
                if (checkBox1.Checked == true)
                {
                    valIn = "null";
                }
                else
                {

                    valIn = "''";

                }
            }
            else
            {


                if(radioButton3.Checked == true)
                {
                    valIn = valIn.Replace("'", "''");
                }
                if (radioButton4.Checked == true)
                {
                    valIn = valIn.Replace("'", "\\'");
                }


                if (checkBox3.Checked == true)
                {
                    valIn = "" + valIn + "";
                }
                else
                {
                    valIn = "'" + valIn + "'";
                }

            }
            return valIn;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != null && textBox2.Text != "")
            {
                Clipboard.SetText(textBox2.Text);
            }
            else
            {
                MessageBox.Show("You should Pull the data from your clipboard then click the proccess button");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                radioButton2.Checked = false;
            }
            else
            {
                radioButton2.Checked = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                radioButton1.Checked = false;
            }
            else
            {
                radioButton1.Checked = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DataObject o = (DataObject)Clipboard.GetDataObject();
            int Tabs = o.GetData(DataFormats.Text).ToString().Count(f => f == '\t');
            int Coms = o.GetData(DataFormats.Text).ToString().Count(f => f == ',');
            MessageBox.Show("Total Tabs=" + Tabs + Environment.NewLine + "Total commas=" + Coms);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog(); 
            if (result == DialogResult.OK)
            {
                saveConfig(saveFileDialog1.FileName);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) 
            {
                loadLastConfig(openFileDialog1.FileName);
            }

       }

        private void loadLastConfig(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show("File not found:" + fileName);
            }

            XmlSerializer ser = new XmlSerializer(typeof(FormState));
            using (FileStream fs = File.OpenRead(fileName))
            {
                state = (FormState)ser.Deserialize(fs);
            }

            checkBox2.Checked = state.NoHeaders;
            radioButton1.Checked = state.TabDel;
            radioButton2.Checked = state.CommaDel;
            checkBox1.Checked = state.UseKey;
            checkBox3.Checked = state.DontWrapValues;
            checkBox4.Checked = state.TrimValues;
            radioButton3.Checked = state.MsSqlEsc;
            radioButton4.Checked = state.MySQlEsc;
            radioButton5.Checked = state.NoEsc;
            textBox3.Text = state.CustomSQL;
        }

        private void saveConfig(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {

                state.NoHeaders = checkBox2.Checked;
                state.TabDel = radioButton1.Checked;
                state.CommaDel = radioButton2.Checked;
                state.UseKey = checkBox1.Checked;
                state.DontWrapValues = checkBox3.Checked;
                state.TrimValues = checkBox4.Checked;
                state.MsSqlEsc = radioButton3.Checked;
                state.MySQlEsc = radioButton4.Checked;
                state.NoEsc = radioButton5.Checked;
                state.CustomSQL = textBox3.Text;

                XmlSerializer ser = new XmlSerializer(typeof(FormState));
                ser.Serialize(sw, state);
                }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.ourace.com/158-clipboard-to-sql");
        }
    }//end class


    public class FormState
    {
        public bool NoHeaders { get; set; }

        public bool TabDel { get; set; }
        public bool CommaDel { get; set; }

        public bool UseKey { get; set; }
        public bool DontWrapValues { get; set; }
        public bool TrimValues { get; set; }

        public bool MsSqlEsc { get; set; }
        public bool MySQlEsc { get; set; }
        public bool NoEsc { get; set; }

        public string CustomSQL { get; set; }

    }


}//end name space
