using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Net;
using System.IO;
using System.Drawing;

namespace AdminPanel
{
    public partial class ecommerce_products : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindData();
            }
        }
        protected void BindData()
        {
            string cmdstr = "Select * from Urunler" + " ";
            string cmdstr2 = " " + "order by Id";
            cmdstr = cmdstr + cmdstr2;

            DataSet ds = new DataSet();
            ds = dbFunction.GetDataSet(cmdstr);//Get data from database with getdataset function in c# file

            ListView1.DataSource = ds.Tables[0]; //Read data from database with listview
            ListView1.DataBind();
        }
        protected void GetProducts_Click(object sender, EventArgs e)
        {
            string uri = "https://listing-external-sit.hepsiburada.com/listings/merchantid/{merchantid}​";
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Accept = "application/xml";
            request.Headers["Authorization"] = "Basic a3VsbGFu/WP9YWT9Ov5pZnJl"; //Send the username and password in the HTTP Authorization Header as base64
            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            //Get xml file
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            XmlDocument xmlDoc = new XmlDocument();
            XmlTextReader read = new XmlTextReader(new StringReader(responseFromServer)); // xmltextreader gets file path, stringreader gets xml content
            xmlDoc.Load(read);

            dbFunction.ProductInfo info = new dbFunction.ProductInfo();

            //Read xml file
            XmlNodeList nodeList;
            XmlNode root = xmlDoc.DocumentElement;

            nodeList = root.SelectNodes("Listings/Listing");

            foreach (XmlNode node in nodeList)
            {
               info.HepsiburadaSku = node.ChildNodes[1].InnerText;
               info.MerchantSku = node.ChildNodes[2].InnerText;
               info.Price = node.ChildNodes[3].InnerText;
               info.AvailableStock = node.ChildNodes[4].InnerText;
               info.DispatchTime = node.ChildNodes[5].InnerText;
               info.CargoCompany1 = node.ChildNodes[6].InnerText;
               info.CargoCompany2 = node.ChildNodes[7].InnerText;
               info.CargoCompany3 = node.ChildNodes[8].InnerText;
               info.ShippingAddressLabel = node.ChildNodes[9].InnerText;
               info.ClaimAddressLabel = node.ChildNodes[10].InnerText;
               info.MaximumPurchasableQuantity = node.ChildNodes[11].InnerText;

               //Save the products to the database with a function in the c# file.
              AdminPanel.dbFunction.UrunKaydet(info.HepsiburadaSku,info.MerchantSku,info.Price,info.AvailableStock,info.DispatchTime,info.MaximumPurchasableQuantity,info.CargoCompany1,info.CargoCompany2,info.CargoCompany3);
            }
           
            reader.Close();
            dataStream.Close();
            response.Close();
            BindData();
        }
    }
}