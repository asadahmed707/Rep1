using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UPLOAD_IN_ARCHIVE
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        //public bool callUser()
        //{
        //    try
        //    {
                
        //        // var options = new RestClientOptions("https://127.0.0.1:9443/api/content/")
        //        //  var options = new RestClientOptions("https://10.100.20.16:9443/api/content/")
        //ewevdwhebcwhbchwcb
        //        // var options = new RestClientOptions("https://10.100.60.4:9443/api/content/")
        //        //var options = new RestClientOptions("http://169.254.1.24:8081/api/content/")
        //        //var options = new RestClientOptions("http://uat-archive.sngpl.com.pk/api/content/")
        //        var options = new RestClientOptions("http://169.254.1.24:8081/api/content/")
        //        {
                    
        //            Authenticator = new HttpBasicAuthenticator(txtUserName.Text, txtPassword.Text ),
                    
        //            //bypass ssl certificate
        //            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        //        };
        //        var client = new RestClient(options);
        //        var request = new RestRequest("stores");
        //        var response = client.GetAsync(request);
        //        //MessageBox.Show(client.);

        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
                
        //    }
        //    return false;
        //}

        private void btnlogin_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text == "superadmin" && txtPassword.Text == "super")
            {
                Form1 objForm = new Form1();
                objForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("The UserName and Password is InValid");
            }
        }
    }
}
