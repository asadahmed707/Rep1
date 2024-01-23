using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Web;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using RestSharp;
using System.Xml;
using static System.Windows.Forms.AxHost;
using CsvHelper;
using System.Globalization;
using System.Dynamic;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace UPLOAD_IN_ARCHIVE
{
    public partial class Form1 : Form
    {
        private static int FieldCount = 0;
        private static string[] SchemaFields;
        private static int[] Columns;
        private static StreamWriter logger;
        private static string currentRecord;
        private static int recordNumber;

        public static Form1 Instance { get; private set; }

        public Form1()
        {
            InitializeComponent();
            Instance = this;
        }

        static async Task<Schemas> GET_SCHEMAS_LIST()
        {
            try
            {
                var options = new RestClientOptions("https://127.0.0.1:9443/api/content/")
                //test
                //retr
               //var options = new RestClientOptions("http://192.168.8.128:8081/api/content/")
                {
                    Authenticator = new HttpBasicAuthenticator("bu_efert", "Admin@123"),
                    //Author = new HttpBasicAuthenticator("bu_efert", "Admin@123"),

                    //bypass ssl certificate
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };
                var client = new RestClient(options);
                var request = new RestRequest("stores");
                //var request = new RestRequest("schemas");
                // The cancellation token comes from the caller. You can still make a call without it.
                var response = await client.GetAsync(request);
                var result = response.Content.ToString();
                var stuff = JsonConvert.DeserializeObject<Schemas>(result);

                return stuff;
            } catch (Exception ex)
            {
                Logging("GET_SCHEMAS_LIST: " + ex.Message);
            }
            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Select XML file";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            openFileDialog1.Title = "Open XML file for Schemas";
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.FileName = "Select CSV file";
            openFileDialog2.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog2.Title = "Open CSV (Comma Separated Values) file";
            openFileDialog2.Multiselect = false;
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                textBox1.Text = "";
                foreach (String file in openFileDialog2.FileNames)
                {
                    textBox1.Text += file.ToString();
                }

                label6.Text = textBox1.Text.Replace("\\" + Path.GetFileName(textBox1.Text), "\\");
                label7.Text = Path.GetFileName(textBox1.Text);

                logger = new StreamWriter(textBox1.Text + ".log");
            }
            catch (Exception ex)
            {
                Logging("While Opening CSV File: " + ex.Message);
            }
        }

        private bool MapColumns(string[] pVals)
        {
            bool result = false;

            FieldCount = list3.Items.Count;
            if (FieldCount != pVals.Length - 1)     // CSV FILE HAS EXTRA COLUMN OF FILE_PATH / FILEPATH / FILE PATH
            {
                string msg = "Schemma has " + FieldCount + " fields while CSV File has " + (pVals.Length - 1) + " value columns. Please resolve before proceeding...";
                Logging(msg);
                logger.WriteLine(msg);
                return result;
            }

            SchemaFields = new string[FieldCount];
            Columns = new int[FieldCount];
            for (int j = 0; j <= FieldCount - 1; j++) //column read
            {
                SchemaFields[j] = list3.Items[j].ToString();
            }

            for (int i = 0; i < FieldCount; i++)
            {
                for (int j = 0; j < FieldCount; j++)
                {
                    if (pVals[i].Trim().ToUpper() == SchemaFields[j].Trim().ToUpper())
                    {
                        Columns[i] = j;
                        break;
                    }
                }
            }

            result = true;
            return result;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // IT WILL POPULATE SCHEMAS FROM ARCHIVE SERVER
            // DUMMY VALUES
            /*list1.Items.Clear();
            list1.Items.Add("EF_PROCESS_DEPT_PSI_PKG");
            list1.Items.Add("EF_PROCESS_DEPT_ISO_POCED");
            list1.Items.Add("EF_PROCESS_DEPT_PIDS");
            list1.Items.Add("Mail_Correspondence");
            list1.Items.Add("PL_LEAVE");
            */
            var data = await GET_SCHEMAS_LIST();

            if (data != null)
            {
                list1.Items.Clear();
                for (int i = 0; i < data.items.Count; i++)
                {
                    list1.Items.Add(data.items[i].name);
                }
            }
        }

        private static void Logging(string message)
        {
            try 
            {
                Instance.list2.Items.Insert(0, "");
                Instance.list2.Items.Insert(0, message);
                //Instance.list2.Items.Insert(0, "");

                if (logger == null)
                {
                    logger = new StreamWriter(Instance.textBox1.Text + ".log");
                }
                logger.WriteLine(message + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex +"Critical Error while creating log file.");
            }
        }

        private bool MapXMLColumns(string[] pVals)
        {
            bool result = false;

            // READ XML FILE
            XmlDocument xmlDocument = new XmlDocument();

            string XmlPath = ""; // textBox5.Text;
            xmlDocument.Load(XmlPath);

            XmlNodeList xmlNodeSchema = xmlDocument.GetElementsByTagName("Schema");
            //XmlElement xmlElement = xmlDocument.DocumentElement;
            for (int a = 0; a < xmlNodeSchema.Count; a++)
            {
                if (xmlNodeSchema[a].Attributes["id"].Value.Trim().ToUpper() == list1.Text.Trim().ToUpper())
                {
                    XmlNodeList xmlNode = xmlNodeSchema[a].ChildNodes;  //.GetElementsByTagName("Field"); //.ToString().Trim().ToUpper();
                    FieldCount = xmlNode.Count;
                    if (FieldCount != pVals.Length - 1)     // CSV FILE HAS EXTRA COLUMN OF FILE_PATH / FILEPATH / FILE PATH
                    {
                        string msg = "Schemma has " + FieldCount + " fields while CSV File has " + (pVals.Length - 1) + " value columns. Please resolve before proceeding...";
                        Logging(msg);
                        return result;
                    }

                    SchemaFields = new string[FieldCount];
                    Columns = new int[FieldCount];
                    for (int j = 0; j <= FieldCount - 1; j++) //column read
                    {
                        SchemaFields[j] = xmlNode[j].InnerText.ToString();
                    }
                    break;
                }
            }

            for (int i = 0; i < FieldCount; i++) 
            {
                for (int j = 0; j < FieldCount; j++)
                {
                    if (pVals[i].Trim().ToUpper() == SchemaFields[j].Trim().ToUpper()) 
                    {
                        Columns[i] = j;
                        break;
                    }
                }
            }

            result = true;
            return result;
        }

        

        private async void button4_Click(object sender, EventArgs e)
        {
            if (logger == null)
                logger = new StreamWriter(textBox1.Text + ".log");

            list2.Items.Clear();

            var storename = list1.Text;
           // MessageBox.Show(storename);
            if (storename == "")
            {
                Logging("No Archive Store Selected...");
                return;
            }
            
            if (textBox1.Text == "")
            {
                Logging("No CSV File Selected...");
                return;
            }

            // READ CSV FILE
            try
            {
               
                using (var rdr = new StreamReader(textBox1.Text))

                using (var csv = new CsvReader(rdr, CultureInfo.InvariantCulture))
                {
                  
                    
                    var records = csv.GetRecords<dynamic>();
                    recordNumber = 1;
                    foreach (dynamic record in records)
                    {
                        string jsonData = "";
                        string fileName = "";
                        currentRecord = "";
                        foreach (dynamic row in record) {
                            // EXCLUDE "FILE PATH"
                            if (row.Key == null || row.Value == null)
                            {
                                Logging("Error uploading Record #: " + row.Key + row.Value + recordNumber + "; Record Detail: " + currentRecord);
                                break;
                            }
                            else
                            {
                                if (row.Key == "FilePath")
                                {
                                    fileName = row.Value;
                                  
                                }
                                else
                                {
                                    jsonData += "\"" + row.Key.Trim() + "\" : \"" + row.Value.Trim() + "\", ";
                                }
                            }
                            currentRecord += row.Value + ", ";
                        }
                        jsonData = "{ \"store\":\"" + storename + "\", \"fields\":{ " + jsonData.Trim().Trim(',') + " }";
                        //MessageBox.Show(jsonData);

                        var created = false;

                        if (fileName != "" && !File.Exists(label6.Text + fileName))
                        {
                            Logging("File '" + fileName + "' Not Found for Record #: " + recordNumber + "; Record Detail: " + currentRecord);
                        }
                        else
                        {
                            if (fileName == "")
                            {
                                jsonData += " }";
                                created = await CreateDoc(jsonData);
                            }
                            else
                            {
                                created = await CreateDocWithFile(jsonData, fileName);
                            }
                        }
                        recordNumber++;
                    }
                }

                if (logger != null)
                {
                    logger.Close();
                    logger = null;
                }
                    

                MessageBox.Show("Uploading from CSV file completed successfully.");

                /*
                StreamReader reader = null;
                if (File.Exists(textBox1.Text))
                {
                    reader = new StreamReader(File.OpenRead(textBox1.Text));
                    int recCount = -1;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        

                        if (recCount == -1)
                        {
                            //success = MapXMLColumns(values);
                            success = MapColumns(values);
                            if (!success)
                                break;
                        }
                        else
                        {
                            bool first = true;
                            string jsondata = null;
                            int i = 0;
                            for (; i < FieldCount; i++)
                            {
                                if (first)
                                {
                                    jsondata = "{ \"store\":\""+storename+"\", \"fields\":{ \"" + SchemaFields[Columns[i]] + "\" : \"" + values[i] + "\" ";
                                    first = false;
                                }
                                else
                                {
                                    jsondata += ", \"" + SchemaFields[Columns[i]] + "\" : \"" + values[i] + "\" ";
                                }
                            }
                            jsondata += "},";

                            string fileName = values[i]; // FILE NAME AS LAST VALUE

                            Logging(jsondata);
                            logger.WriteLine(jsondata + " " + fileName);

                            //var jsondata = JsonConvert.SerializeObject();
                            var created = await CreateDoc(jsondata, fileName);
                            
                        }
                        recCount++;
                    }
                }*/
            }
            catch (Exception ex) 
            {
                Logging (ex.Message);
            }

            /*
                        record = "";
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        foreach (var item in values)
                        {
                            listA.Add(item);
                            record += item + "~";
                        }
                        Logging (record);
                    }
                }
                else
                {
                    Logging("File doesn't exist");
                }
            }
            catch (Exception ex)
            {
                Logging("E R R O R : " + ex.Message);
            }*/
        }

        static string ConvertToBinaryString(byte[] bytes)
        {
            StringBuilder binaryStringBuilder = new StringBuilder();

            foreach (byte b in bytes)
            {
                binaryStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return binaryStringBuilder.ToString();
        }


         public async Task<bool> CreateDocWithFile(string jsondata, string fileName)
         {
             try
             {
                 var options = new RestClientOptions("https://127.0.0.1:9443/api/content/")
                 //var options = new RestClientOptions("http://169.254.1.24:8081/api/content/")
                 //  var options = new RestClientOptions("https://10.100.20.16:9443/api/content/")
                 //var options = new RestClientOptions("https://10.100.60.4:9443/api/content/")
                 //var options = new RestClientOptions("http://uat-archive.sngpl.com.pk/api/content/")
                 // var options = new RestClientOptions("http://169.254.1.24:8081/api/content/")
                // var options = new RestClientOptions("http://192.168.8.128:8081/api/content/")
                 {
                     Authenticator = new HttpBasicAuthenticator("bu_efert", "Admin@123"),
                     RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                 };

                 var client = new RestClient(options);
                 //request.AddHeader("Content-Type", "application/docx");
                 var request = new RestRequest("files/upload", Method.Post);
                 byte[] filebytes = File.ReadAllBytes(label6.Text + fileName);
                 var binstring = ConvertToBinaryString(filebytes);
               // request.AddFile("ename", label6.Text + fileName);
                var mimeType = MimeMapping.GetMimeMapping(label6.Text + fileName);
                //var fl = File.ReadAllBytes(label6.Text + fileName);
                //request.AddHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                //request.AddParameter("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", binstring, ParameterType.RequestBody);
                //request.AddParameter(mimeType, fl, ParameterType.RequestBody);

                request.AddHeader("Content-Type", mimeType);
                request.AddParameter(mimeType, filebytes, ParameterType.RequestBody);
                //request.AddFile(fileName, label6.Text + fileName);
                var response = await client.ExecuteAsync(request);
              
                //request.RequestFormat = DataFormat.Binary;

                ////request.AddJsonBody(jsondata);
                ////request.AddHeader("Content-Type", "application/pdf");
                //// var mimeType = MimeMapping.GetMimeMapping(label6.Text + fileName);
                ////MessageBox.Show(mimeType);
                //request.AddHeader("Content-Type", "application/docx");
                //var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                 {
                     var stuff = JsonConvert.DeserializeObject<Upload>(response.Content.ToString());
                    
                    //d610e22f1c2841d3a40fca698ad7a274
                    jsondata += ", \"attachments\":[{\"uploadId\":\"" + stuff.uploadId + "\",\"name\":\"" + fileName + "\",\"contentType\":\""+stuff.contentType+"\"}]}";


                    //MessageBox.Show(jsondata);

                    return await CreateDoc(jsondata);
                 }
                 else
                 {
                     Logging("Error uploading Record #: " + recordNumber + "; Record Detail: " + currentRecord);
                    // MessageBox.Show(jsondata);
                 }
             }
             catch (Exception ex)
             {
                 Logging("CreateDocWithFile: " + ex.Message);
             }

             return false;
         }






        public async Task<bool> CreateDoc(string jsondata)
        {
            try 
            {
               // var options = new RestClientOptions("http://169.254.1.24:8081/api/content/")
                //var options = new RestClientOptions("https://10.100.20.16:9443/api/admin/")
                //var options = new RestClientOptions("https://10.100.60.4:9443/api/content/")
                var options = new RestClientOptions("https://127.0.0.1:9443/api/content/")
                //var options = new RestClientOptions("http://192.168.8.128:8081/api/content/")
                {
                    Authenticator = new HttpBasicAuthenticator("bu_efert", "Admin@123"),
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };

                var client = new RestClient(options);
                var request = new RestRequest("docs", Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(jsondata);
                request.AddHeader("Content-Type", "application/json");

                var response = await client.ExecuteAsync(request);


                if (response.IsSuccessful)
                {
                    Instance.list4.Items.Insert(0, "");
                    Instance.list4.Items.Insert(0, "Record #: " + recordNumber + "; Record Detail: " + currentRecord);
                    return true;
                }
                else
                {
                    Logging("Error uploading Record #: " + recordNumber + "; Record Detail: " + currentRecord);
                   // MessageBox.Show(jsondata);
                }
            }


            catch (Exception ex)
            {
                Logging("CreateDoc: " + ex.Message);
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label6.Text = "";
            label7.Text = "";
            button1_Click(sender, e);
           // System.Windows.Forms.Application.ExitThread();
        }

        static async Task<Schema> GET_SCHEMA_ATTRIBUTES(string schema)
        {
            try
            {
                //var options = new RestClientOptions("http://169.254.1.24:8081/api/content/schemas/")
              // var options = new RestClientOptions("http://192.168.8.128:8081/api/content/schemas")
                 var options = new RestClientOptions("https://127.0.0.1:9443/api/content/schemas")
                {
                    Authenticator = new HttpBasicAuthenticator("bu_efert", "Admin@123"),
                    //bypass ssl certificate
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };
                var client = new RestClient(options);

                var request = new RestRequest(schema);
                // The cancellation token comes from the caller. You can still make a call without it.
                var response = await client.GetAsync(request);
                var result = response.Content.ToString();
                var stuff = JsonConvert.DeserializeObject<Schema>(result);

                return stuff;
            }
            catch (Exception ex)
            {
                Logging("GET_SCHEMA_ATTRIBUTES: " + ex.Message);
            }
            return null;
        }

        private async void list1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var data = await GET_SCHEMA_ATTRIBUTES(list1.Text);

                list3.Items.Clear();
                textBox2.Text = "";
                if (data.attributes != null)
                {
                    for (int i = 0; i < data.attributes.Count; i++)
                    {
                        list3.Items.Add(data.attributes[i].name);
                        textBox2.Text += data.attributes[i].name + "\t";
                    }
                }
            } 
            catch (Exception ex)
            {
                Logging("list1_SelectedIndexChanged: " + ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (logger != null)
            {
                logger.Close();
                logger = null;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void list3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //this.Close();   
        }
    }

    public class Item
    {
        public Links _links { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string storeId { get; set; }
        public List<Schema> schemas { get; set; }
    }

    public class Links
    {
        public Self self { get; set; }
        public Query query { get; set; }
    }

    public class Query
    {
        public string rel { get; set; }
        public string href { get; set; }
    }

    public class Schemas
    {
        public Links _links { get; set; }
        public List<Item> items { get; set; }
    }

    public class Schema
    {
        public Links _links { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public List<Attribute> attributes { get; set; }
    }

    public class Self
    {
        public string rel { get; set; }
        public string href { get; set; }
    }

    public class Attribute
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public bool required { get; set; }
        public bool searchable { get; set; }
        public bool hitlistField { get; set; }
        public List<string> dataTypes { get; set; }
    }
    public class Upload
    {
        public int length { get; set; }
        public string uploadId { get; set; }
        public string md5 { get; set; }
        public string contentType { get; set; }
    }
}


