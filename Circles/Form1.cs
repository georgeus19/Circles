using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Circles
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Graphics g;
        

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) {
            Parser parser;
            button1.Visible = false;
            g = CreateGraphics();
            StreamReader sr = null;
            try {
                sr = new StreamReader("input.txt");
                parser = new Parser(sr);
                var circles = parser.ReadCircles();
                Algorithm alg = new Algorithm(circles, new Vector(60f, 60f));
                var Diameter = alg.Run(g);
                for (int i = 0; i < circles.Count; i++) {
                    circles[i].DrawItself(g);
                }
            } 
            catch (UnauthorizedAccessException) {
                Console.WriteLine("File Error");
            } 
            catch (FileNotFoundException) {
                Console.WriteLine("File Error");
            } 
            finally {
                if (sr != null)
                    sr.Dispose();
            }
            

        }
    }
}
