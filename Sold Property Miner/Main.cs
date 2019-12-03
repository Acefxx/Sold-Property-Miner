using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sold_Property_Miner
{
    public partial class Main : Form
    {
        #region Main Variables
        /* Main Public Data Types/Objects used in the App */
        List<SoldProperty> soldProperties;
        HashSet<string> setSuburbs = new HashSet<string>();
        HashSet<string> setDuplicateSoldPropertiesCheck = new HashSet<string>();
        int suburbCounter = 0;

        /* Defines Main Application Configeration Files */
        const string pathConfig = "./Data/App_config.cfg";
        #endregion

        #region Constructor & Main
        public Main()
        {
            InitializeComponent();
        }
        
        private void Main_Load(object sender, EventArgs e)
        {
            /* Makes sure All aplication configeration Files exist if not creates them */
            loadApplicationAndDataFiles();
        }
        #endregion

        #region Pre-Load Settings & Configeration
        private void loadApplicationAndDataFiles()
        {
            /* Makes sure All aplication configeration Files exist if not creates them */
            System.IO.Directory.CreateDirectory("Data");
            if (!File.Exists(pathConfig))
            {
                StreamWriter sw = File.AppendText(pathConfig);
                sw.WriteLine("Suburbs:./Data/Suburbs.txt");
                sw.Close();
            }
            laodAppConfig();
            /* Creates Default config with basic Values */
            if (!File.Exists(txtSuburbsFilePath.Text))
            {
                StreamWriter sw = File.AppendText(txtSuburbsFilePath.Text);
                sw.WriteLine("New South Wales");
                sw.Close();
            }
            readInSuburbs();
        }

        private void readInSuburbs()
        {
            int counter = 0;
            string line;

            /* Read the file and display it line by line. */
            System.IO.StreamReader file = new System.IO.StreamReader(txtSuburbsFilePath.Text);
            while ((line = file.ReadLine()) != null)
            {
                setSuburbs.Add(line);
                counter++;
            }
            suburbCounter = counter;
            file.Close();
        }

        private void laodAppConfig()
        {
            int counter = 0;
            string line;

            /* Read the file and display it line by line. */
            System.IO.StreamReader file = new System.IO.StreamReader(pathConfig);
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split(':');
                if(words[0] == "Suburbs")
                {
                    txtSuburbsFilePath.Text = words[1];
                }

                counter++;
            }
            file.Close();
        }
        #endregion

        #region Change/Edit Suburb List Config File
        private void btnFileExplore_Click(object sender, EventArgs e)
        {
            /* Saves the File location of the CSV Database File */
            using (FileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
                if (DialogResult.OK == fileDialog.ShowDialog())
                {
                    txtCSVFileLocation.Text = fileDialog.FileName;
                    StreamWriter sw = File.AppendText(txtCSVFileLocation.Text);
                    sw.WriteLine("Sold Price" + "," + "Type" + "," + "Address" + "," + "Suburb" + "," + "Date Sold" + "," + "Bedrooms" + "," + "Bathrooms" + "," + "Car Ports");
                    foreach (SoldProperty s in soldProperties)
                    {
                        if (File.Exists(txtCSVFileLocation.Text))
                        {
                            /* Output: Writes to CSV File */
                            sw.WriteLine(s.soldPrice.Replace(",", "") + "," + s.type + "," + s.address.Replace(",", "") + "," + s.suburb + "," + s.dateSold + "," + s.bedrooms + "," + s.bathrooms + "," + s.carPorts);
                        }
                    }
                    sw.Close();
                    MessageBox.Show("CSV File has now been exported to: " + txtCSVFileLocation.Text);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            nudNumSearchRecords.Value = 0;
            Process.Start("notepad.exe", txtSuburbsFilePath.Text);
        }
        #endregion

        #region Start Run Property Miner
        private void btnStartMining_Click(object sender, EventArgs e)
        {
            /* Starts Mining Thread - Free up GUI Thread */
            startMining();
        }

        private void startMining()
        {
            /* Runs all mining functions */
            if (nudNumSearchRecords.Value != 0)
            {
                pnlNotice1.Visible = false;
                btnFileExplore.Enabled = true;
                btnStartMining.Enabled = false;
                toolStatus.Text = "Processing. . . . . . . . .";
                toolStatus.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
                
                /* Main Maining Thread */
                miningThread.RunWorkerAsync();
            }
            else
            {
                /* Make user select Page number */
                pnlNotice1.Visible = true;
                MessageBox.Show("Please select number of pages per suburb to search! \nNote: Approx 20 Results per page for each suburb.\n\nTry 1 Page First if unsure.");
            }
        }
        #endregion

        #region HTTPWebRequest & Text Manipulation
        private string getHTMLSourceCode(string urlAddress)
        {

            /* Returns HTML Source Code of Selected URL Address */
            string data = string.Empty;
            try
             {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            
            request.Method = "GET";

            /* Raw Header and Cookie Data Test */
            ////request.ContentType = "text/html; charset=utf-8";
            //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

            /////* Adds cookie */
            //request.CookieContainer = new CookieContainer();
            ////request.CookieContainer.Add(new Cookie("_cb_ls", "1") { Domain = "www.realestate.com.au" });
            ////request.CookieContainer.Add(new Cookie("_cb", "VSlV6D-JuNXC0IeFB") { Domain = "www.realestate.com.au" });
            ////request.CookieContainer.Add(new Cookie("Country", "AU") { Domain = "www.realestate.com.au" });
            ////request.CookieContainer.Add(new Cookie("s_cc", "true") { Domain = "www.realestate.com.au" });

            //request.CookieContainer.Add(new Cookie("reauid", "4ef466687b5600002f38d75d6f030000cccf0f00") { Domain = "www.realestate.com.au" });
            //request.CookieContainer.Add(new Cookie("Country", "AU") { Domain = "www.realestate.com.au" });
            //request.CookieContainer.Add(new Cookie("ak_bmcs", "2d43a97a-050c-3634-b63c-9b41ab6300a4") { Domain = "www.realestate.com.au" });
            //request.CookieContainer.Add(new Cookie("kmam_lapoz", "zGlUuF%2F3ONdzjCfL3K2kHw%3D%3D%3A%3AghZiRB3HzxFbPTgP5KCJ906rcaKgR%2BTzmDReCflX2AcfZ8B4dEFZCxb4B4iNS9CjmlR%2BJmYdix84r8QJLKGt4DN0rOXaUFnXXHBMX3ecGBdbqHVe5DJ5Jw8UBZHdODV461ZE6ECS0EsKmW4rFovFiDYlgte77mj9oGLXi6CQdSof79WDn0El50XMrC3Cr1bVCQU4sgMhIFeCaM%2BEI%2B%2BxOFDcHldyhithEFDPIrkWAHXwPFzyDwCxNV%2BJqf%2BKmqt%2BAPWvbKF52Bm9EuNncjDFOXFtwdA2SXUtYLm1Wrz9GWUO8yeIRuvMeBY3gJwRRMIdxqKcuITYzpg3w9%2FS06KdPQwZbI3OnL2uX9CNDAjemtC%2B1tDW72DKhSIGX0mTJ%2FL89Ras7Iycu0UeL%2F4H31%2Bb0u9Hp2MRS8Dt2oyQusqAACKyRjM5BZyJS7JHkRVFCj9c1sboFijpEJtodtIM0wCNVpEx5vDFofFshFSDkVYd5OvB7lDynPx3UHKzAUtgneDoscfc7SKHL0%2BSU89qacMCdQ%3D%3D") { Domain = "www.realestate.com.au" });

            //WebHeaderCollection myWebHeaderCollection = request.Headers;
            ////myWebHeaderCollection.Add("Cache-Control:max-age=0");
            ////myWebHeaderCollection.Add("Accept-Encoding:gzip, deflate, br");
            ////myWebHeaderCollection.Add("Accept-Language: en-GB,en;q=0.9,en-US;q=0.8,de;q=0.7");

            ////myWebHeaderCollection.Add("Sec-Fetch-Mode: navigate");
            ////myWebHeaderCollection.Add("Sec-Fetch-Site: none");
            ////myWebHeaderCollection.Add("Sec-Fetch-User: ?1");
            ////myWebHeaderCollection.Add("Upgrade-Insecure-Requests: 1");

            //myWebHeaderCollection.Add("Cache-Control: max-age=0");
            ////request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            //myWebHeaderCollection.Add("Accept-Encoding: gzip, deflate, br");
            //myWebHeaderCollection.Add("Accept-Language: en-GB,en;q=0.9,en-US;q=0.8,de;q=0.7");
            ////myWebHeaderCollection.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            //myWebHeaderCollection.Add("DNT: 1");
            //myWebHeaderCollection.Add("Sec-Fetch-Mode: navigate");
            //myWebHeaderCollection.Add("Sec-Fetch-Site: same-origin");
            //myWebHeaderCollection.Add("Sec-Fetch-User: ?1");
            //myWebHeaderCollection.Add("Upgrade-Insecure-Requests: 1");


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    }

                    data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }


            } catch
            {
                /* HTML didn't load Possibly due to Lag/Net issues */
                data = string.Empty;
            }

            return data;
        }

        private List<SoldProperty> filterSiteSourceCode(string sourceCode)
        {
            /* Gets Raw Property HTML Data from Realestate.com.au website */
            List<string> rawPropertyData;
            string startStr = "results-card residential-card";
            string endStr = "listing-bookmark listing-bookmark--search-results";
            rawPropertyData = Helper.ExtractFromString(sourceCode, startStr, endStr);

            /* Seperates Raw data into meaningful Sold Property Data List */
            return breakRawPropertyData(rawPropertyData);
        }

        private List<SoldProperty> breakRawPropertyData(List<string> rawData)
        {
            List<SoldProperty> soldPropertiesTemp = new List<SoldProperty>();
            foreach (string x in rawData)
            {
                SoldProperty tempSoldProperty = new SoldProperty();

                /* Grabs Sold Property Price */
                String result = x.Split(new string[] { "property-price" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                tempSoldProperty.soldPrice = result.Split('>', '<')[1];

                /* Grabs Property Type */
                result = x.Split(new string[] { "residential-card__property-type" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                tempSoldProperty.type = result.Split('>', '<')[1];

                /* Grabs Property Address */
                string tempBreakdown = x.Split(new string[] { "PropertyImage" }, StringSplitOptions.None)[1].Split(new string[] { "property-image__img" }, StringSplitOptions.None)[0].Trim();
                tempSoldProperty.address = tempBreakdown.Split(new string[] { "alt=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0].Trim();

                /* Grabs Property Suburb */
                ; int count = tempSoldProperty.address.Split(',').Length - 1;
                if (count == 2)
                {
                    tempSoldProperty.suburb = tempSoldProperty.address.Split(',', ',')[1];
                } else if (count > 2)
                {
                    tempSoldProperty.suburb = tempSoldProperty.address.Split(',', ',')[2];
                }

                /* Grabs Property Date of Sale */
                try
                {
                    tempSoldProperty.dateSold = x.Split(new string[] { "Sold on " }, StringSplitOptions.None)[1].Split(new string[] { "<" }, StringSplitOptions.None)[0].Trim();
                } catch
                {
                    /* No Value Given */
                    tempSoldProperty.dateSold = "01 Jan 1000";
                }
                
                /* Grabs Number of Bedrooms */
                try
                {
                    result = x.Split(new string[] { "general-features__beds" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    result = result.Split(new string[] { "!-- --" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    tempSoldProperty.bedrooms = Convert.ToInt32(result.Split('>', '<')[1]);
                } catch
                {
                    tempSoldProperty.bedrooms = 0;
                }


                /* Grabs Number of Bathrooms */
                try
                {
                    result = x.Split(new string[] { "general-features__baths" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    result = result.Split(new string[] { "!-- --" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    tempSoldProperty.bathrooms = Convert.ToInt32(result.Split('>', '<')[1]);
                } catch
                {
                    tempSoldProperty.bathrooms = 0;
                }

                /* Grabs Number of Car Ports */
                try
                {
                    result = x.Split(new string[] { "general-features__cars" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    result = result.Split(new string[] { "!-- --" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Trim();
                    tempSoldProperty.carPorts = Convert.ToInt32(result.Split('>', '<')[1]);
                } catch
                {
                    tempSoldProperty.carPorts = 0;
                }
                if(setDuplicateSoldPropertiesCheck.Add(tempSoldProperty.soldPrice + tempSoldProperty.address))
                {
                    /* ONLY ADDS NEW SOLD PROPERTY RECORD if NOT Duplicate */
                    soldPropertiesTemp.Add(tempSoldProperty);
                } else
                {
                    /* Duplicated Sold Adress Found */
                    //return null;
                }
            }
            return soldPropertiesTemp;
        }
        #endregion

        #region Mining Thread Work & Reporting
        private void miningThread_DoWork(object sender, DoWorkEventArgs e)
        {
            /* Runs all mining functions */
            List<SoldProperty> temp = new List<SoldProperty>();
            List<SoldProperty> tempCheck = new List<SoldProperty>();
            int numOfSearchRecordPages = Convert.ToInt32(nudNumSearchRecords.Text);
            int num = 0;
            /* Does the Mining work for each Suburb x amount of Pages */
            int index = 0;
            foreach (string strSuburbName in setSuburbs)
            {
                string suburbName = strSuburbName.Replace(' ', '+');
                for (int x = 1; x < numOfSearchRecordPages + 1; x++)
                {
                    /* Report Progress to Progress Bar */
                    int y = x + index;
                    num = y * 100 / (numOfSearchRecordPages * suburbCounter);
                    miningThread.ReportProgress(num);

                    temp.AddRange(getSoldPropertyData(x, suburbName));

                    //tempCheck = getSoldPropertyData(x, suburbName);
                    //if (tempCheck == null)
                    //{
                    //    /* Duplicate Sold Property Found Moving onto Next Suburb */
                    //    break;
                    //}
                    //else
                    //{
                    //    /* Valid Unique Sold Property Found */
                    //    temp.AddRange(tempCheck);
                    //}
                }
                setDuplicateSoldPropertiesCheck.Clear();
                index += numOfSearchRecordPages;
            }
            e.Result = temp;
        }

        private List<SoldProperty> getSoldPropertyData(int num, string strSuburbName)
        {
            /* All Functions Used to Collect Sold Property Data */
            string tempWebLink = "https://www.realestate.com.au/sold/in-" + strSuburbName + "%3b/list-" + num + "?activeSort=solddate";
            string sourceCode = getHTMLSourceCode(tempWebLink);
            return filterSiteSourceCode(sourceCode);
        }

        private void miningThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 100)
            {
                /* Out Of percent Boundries */
            }
            else
            {
                toolStripProgressBar1.Value = e.ProgressPercentage;
            }
        }

        private void miningThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /* Updates the Status bar as Completed */
            toolStatus.Text = "Complete";
            toolStatus.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);

            /* Gets the finished Sold Property results from Background Worker */
            soldProperties = (List<SoldProperty>)e.Result;

            /* Clears the DataGridView before Use */
            dataDisplay.Columns.Clear();

            /* All of the DataTable Types/Columns */
            DataTable table = new DataTable();
            table.Columns.Add("Sold Price", typeof(double));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Address", typeof(string));
            table.Columns.Add("Suburb", typeof(string));
            table.Columns.Add("Sale Date", typeof(DateTime));
            table.Columns.Add("Number Of Bedrooms", typeof(int));
            table.Columns.Add("Number Of Bathrooms", typeof(int));
            table.Columns.Add("Number Of Car Ports", typeof(int));

            /* Displays the results to DataGridView */
            foreach (SoldProperty currentElement in soldProperties)
            {
                /* Adds the Rows to DataTable */
                string str = currentElement.soldPrice.Replace("$", "");
                str = str.Replace(",", "");
                double tempD = 0.0;
                try
                {
                    tempD = Convert.ToInt32(str);
                } catch
                {
                    /* Value Not Given */
                    tempD = 0.0;
                }
                table.Rows.Add(tempD, currentElement.type, currentElement.address, currentElement.suburb, currentElement.dateSold, currentElement.bedrooms, currentElement.bathrooms, currentElement.carPorts);
            }
            /* Loads the DataTable Quickly to the DataGridView - Rather than 1 by 1 */
            dataDisplay.DataSource = table;
            dataDisplay.AutoResizeColumns();
            int recordNum = dataDisplay.RowCount - 1;
            toolStripStatusLabel2.Text = recordNum.ToString();
            toolStripProgressBar1.Value = 0;
            dataDisplay.Columns[0].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("en-AU");
            dataDisplay.Columns[0].DefaultCellStyle.Format = "c";
            btnStartMining.Enabled = true;
        }
        #endregion

        #region About & Extra 
        private void nudNumSearchRecords_ValueChanged(object sender, EventArgs e)
        {
            /* Calculates Results Estimate */
            if (nudNumSearchRecords.Value == 1)
            {
                loadApplicationAndDataFiles();
            }
            lblApproxResults.Text = ((nudNumSearchRecords.Value * 20) * suburbCounter).ToString();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            About abt = new About();
            abt.Show();
        }

        private void mainTabControl_Selected(object sender, TabControlEventArgs e)
        {
            /* Enters Graph Tab */
            if (mainTabControl.SelectedIndex == 1)
            {
                /* Generate Suburb Lists */
                foreach (String s in setSuburbs)
                {
                    cmbSuburbs.Items.Add(s);
                }
            }
        }

        private void btnGraph_Click(object sender, EventArgs e)
        {
            /* Gernerate Graph */
            if (cmbSuburbs.SelectedIndex != -1)
            {
                /* Sets the Graph Title */
                chartDisplay.Titles.Clear();
                chartDisplay.Titles.Add(cmbSuburbs.Text);
            }
        }
    #endregion
    }
}
