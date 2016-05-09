using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TestEuclid.Tests;

namespace TestEuclid
{
    public partial class Form1 : Form
    {
        private Type _testClass = typeof(Tests),
            _targetDelegate = typeof(TestMethod);
        public Form1()
        {
            InitializeComponent();
            ListAllTestMethods();
        }

        private void ListAllTestMethods()
        {

            MethodInfo[] methods = _testClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                bool fitsTarget = (Delegate.CreateDelegate(_targetDelegate, methodInfo, false) != null);
                if (fitsTarget)
                {
                    ListViewItem item = new ListViewItem(methodInfo.Name);
                    item.SubItems.Add("x");
                    item.Tag = methodInfo;
                    testLvw.Items.Add(item);
                }
            }
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in testLvw.SelectedItems)
            {
                Delegate test = Delegate.CreateDelegate(_targetDelegate, item.Tag as MethodInfo, false);
                if (test != null)
                {
                    bool result = (bool) test.Method.Invoke(null, null);
                    item.SubItems[1].Text = result.ToString();
                }
            }
        }
    }
}
