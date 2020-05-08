using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using UIWindows;
using SVGToPrefab;
[assembly: InternalsVisibleTo("SVGToPrefab")]

namespace UIWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        
        private void color1_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[0] = color1.Text;
        }

        private void color2_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[1] = color1.Text;
        }

        private void color3_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[2] = color1.Text;
        }

        private void color4_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[3] = color1.Text;
        }

        private void color5_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[4] = color1.Text;
        }

        private void color6_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[5] = color1.Text;
        }

        private void color7_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[6] = color1.Text;
        }

        private void color8_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[7] = color1.Text;
        }

        private void color9_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[8] = color1.Text;
        }

        private void color10_TextChanged(object sender, EventArgs e)
        {
            Input.Colors.ids[9] = color1.Text;
        }
    }
}
