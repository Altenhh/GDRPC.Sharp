using System;
using System.Windows.Forms;

namespace Tsubasa.Forms
{
    public partial class LoginForm : Form
    {
        public string FormUsername;
        public string FormPassword;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            FormUsername = Username.Text;
            FormPassword = Password.Text;
        }
    }
}