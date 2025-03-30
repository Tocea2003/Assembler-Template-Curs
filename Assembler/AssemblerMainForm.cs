/***********************************************************************************************
 * Project: Simulator of an microprogrammed Didactical Processor (DP)
 * Module: Assembler for DP
 * Filename: AssemblerMainForm.cs
 * Users: Students in Computer Science (3rd year of study)
 * Creator: Horia V. Caprita
 * Date: 10.03.2015 
***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;




namespace Assembler
{


    


    public partial class AssemblerMainForm : Form
    {


        private Dictionary<string, string> opcodeTable = new Dictionary<string, string>
{
    {"MOV", "0000"},
    {"ADD", "0001"},
    {"SUB", "0010"},
    {"CMP", "0011"},
    {"AND", "0100"},
    {"OR", "0101"},
    {"XOR", "0110"},
    {"CLR", "1000000000"},
    {"NEG", "1000000001"},
    {"INC", "1000000010"},
    {"DEC", "1000000011"},
    {"ASL", "1000000100"},
    {"ASR", "1000000101"},
    {"LSR", "1000000110"},
    {"ROL", "1000000111"},
    {"ROR", "1000001000"},
    {"RLC", "1000001001"},
    {"RRC", "1000001010"},
    {"JMP", "1000001011"},
    {"CALL", "1000001100"},
    {"PUSH", "1000001101"},
    {"POP", "1000001010"},
    {"BR", "11000000"},
    {"BNE", "11000001"},
    {"BEQ", "11000010"},
    {"BPL", "11000011"},
    {"BMI", "11000100"},
    {"BCS", "11000101"},
    {"BCC", "11000110"},
    {"BVS", "11000111"},
    {"BVC", "11001000"},
    {"CLC", "1110000000000000"},
    {"CLV", "1110000000000001"},
    {"CLZ", "1110000000000010"},
    {"CLS", "1110000000000011"},
    {"CCC", "1110000000000111"},
    {"SEC", "1110000000010000"},
    {"SEV", "1110000000010001"},
    {"SEZ", "1110000000010010"},
    {"SES", "1110000000010011"},
    {"SCC", "1110000000011000"},
    {"NOP", "1110000000011001"},
    {"RET", "1110000000011010"},
    {"RETI", "1110000000011011"},
    {"HALT", "1110000000100000"},
    {"WAIT", "1110000000100001"},
    {"PUSHPC", "1110000000100010"},
    {"POPPC", "1110000000100011"},
    {"PUSHFLAG", "1110000000100100"},
    {"POPFLAG", "1110000000100101"}
};
        private String filename;

        public AssemblerMainForm()
        {
            InitializeComponent();
        }

        /* Function used to obtain the ASM filename (*.asm) */
        private String getFileName(String filter)
        {
            try
            {
                /* Local variable used to store the filename */
                String fileNameWithPath = "";
                /* Instantiate an OpenFileDialog */
                OpenFileDialog of = new OpenFileDialog();
                /* Set the filter */
                of.Filter = filter;
                /* Get the working directory */
                of.InitialDirectory = Path.GetFullPath("..\\Debug");
                of.RestoreDirectory = true;
                /* Display the Open File dialog */
                if (of.ShowDialog() == DialogResult.OK)
                {
                    /* Get only the filename with full path */
                    fileNameWithPath = of.FileName;
                    /* Get only the filename without path */
                    filename = of.SafeFileName;
                }
                /* Return the filename with complete path */
                return fileNameWithPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void ParseFileButton_Click(object sender, EventArgs e)
        {
            List<String> asmElements = new List<String>(); // Move this line outside the try block
            try
            {
                /* local variable used for debugging only */
                int lineCounter = 0;
                /* Create a parser object used for ASM file
                    REMEMBER: this parser can be used for all kind of text files!!!
                 */
                TextFieldParser parser = new TextFieldParser(filename);
                /* Reinitialize the Text property of OutputTextBox */
                OutputTextBox.Text = "";
                /* Define delimiters in ASM file */
                String[] delimiters = { ":", ",", " ", "(", ")" };
                /* Specify that the elements in ASM file are delimited by some characters */
                parser.TextFieldType = FieldType.Delimited;
                /* Set-up the specified delimiters */
                parser.SetDelimiters(delimiters);
                /* Parse the entire ASM file based on previous specifications*/
                while (!parser.EndOfData)
                {
                    /* Read an entire line in ASM file
                       and split this line in many strings delimited by delimiters */
                    string[] asmFields = parser.ReadFields();
                    /* Store each string as a single element in the list
                       if this string is not empty */
                    foreach (string s in asmFields)
                    {
                        if (!s.Equals(""))
                        {
                            asmElements.Add(s);
                        }
                    }
                    /* Counting the number of lines stored in ASM file */
                    lineCounter++;
                }

                /* Close the parser */
                parser.Close();
                /* If the file is empty, trigger a new exception which
                   in turn will display an error message */
                if (lineCounter == 0)
                {
                    Exception exc = new Exception("The ASM file is empty!");
                    throw exc;
                }
                else
                {
                    /* Display every token in OutputTextBox */
                    foreach (String s in asmElements)
                    {
                        OutputTextBox.Text += s + Environment.NewLine;
                    }
                    /* Display an information about the process completion */
                    MessageBox.Show("Parsing is completed!", "Assembler information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            GenerateObjectFile(asmElements, ASMFileTextBox.Text);
        }

        private void GenerateObjectFile(List<string> tokens, string asmPath)
        {
            try
            {
                // Crează folderul special pentru .obj
                string objFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeneratedOBJ");
                Directory.CreateDirectory(objFolder);

                // Nume fișier .obj
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(asmPath);
                string objPath = Path.Combine(objFolder, fileNameWithoutExt + ".obj");

                // Curăță fereastra UI
                textBox1.Text = "";

                using (StreamWriter sw = new StreamWriter(objPath))
                {
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        string token = tokens[i].ToUpper();

                        if (opcodeTable.ContainsKey(token))
                        {
                            string line = opcodeTable[token];
                            int j = i + 1;

                            while (j < tokens.Count && !opcodeTable.ContainsKey(tokens[j].ToUpper()))
                            {
                                line += " " + tokens[j];
                                j++;
                            }

                            sw.WriteLine(line);                // Scrie în fișier
                            textBox1.Text += line + "\r\n"; // Afișează în UI
                        }
                    }
                }

                MessageBox.Show("Fișierul .obj a fost salvat în:\n" + objPath,
                    "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la generarea fișierului .obj:\n" + ex.Message,
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                /* String used to be displayed in ASMFileTextBox */
                String filename = "";
                /* Reinitialize the Text property of OutputTextBox */
                OutputTextBox.Text = "";
                /* Take the filename selected by user */
                filename = getFileName("ASM file for didactical processor(*.asm)|*.asm");
                /* Display the filename in ASMFileTextBox */
                ASMFileTextBox.Text = filename != null ? filename : ASMFileTextBox.Text;
                /* Enable/Disable the ParseFileButton depending of user choice */
                if (!filename.Equals(""))
                {
                    ParseFileButton.Enabled = true;
                }
                else
                {
                    ParseFileButton.Enabled = false;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssemblerMainForm_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
