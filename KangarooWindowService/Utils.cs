using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KangarooWindowService
{
    public class Utils
    {
        public static Location GetToaDoByDiaDiem(string DiaDiem)
        {
            //log4net.ILog log = log4net.LogManager.GetLogger(typeof(LayViTriTheoToaDo));
            Location obj = new Location();

            // string apiKey = "AIzaSyBnwO1ETMtZC7AonESIQbpnwNaPvBhqVnI";
            string apiKey = "";
            if (ConfigurationManager.AppSettings["GOOGLEAPIKEY"] != null)
            {
                apiKey = ConfigurationManager.AppSettings["GOOGLEAPIKEY"];

            }
            string url = "https://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false&key=" + apiKey;
            url = string.Format(url, DiaDiem);
            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(reader.ReadToEnd());
                        //  obj.status = doc.DocumentElement.ChildNodes[0].ChildNodes[0].InnerText;
                        XmlNode result = doc.DocumentElement.ChildNodes[1];
                        if (result != null)
                        {
                            foreach (XmlNode address_component in result.ChildNodes)
                            {
                                if (address_component.Name == "geometry")
                                {
                                    //check type

                                    XmlNodeList type = address_component.SelectNodes("location");

                                    foreach (XmlNode typenode in type)
                                    {
                                        foreach (XmlNode loca in typenode.ChildNodes)
                                        {
                                            if (loca.Name == "lat")
                                                obj.Latitude = double.Parse(loca.InnerText);
                                            else if (loca.Name == "lng")
                                                obj.Longitude = double.Parse(loca.InnerText);
                                        }

                                    }

                                }
                                else if (address_component.Name == "address_component")
                                {
                                    //check type

                                    XmlNodeList type = address_component.SelectNodes("type");
                                    bool isKhac = true;
                                    foreach (XmlNode typenode in type)
                                    {
                                        if (typenode.InnerText == "country")
                                        {
                                            isKhac = false;
                                            break;
                                        }
                                        else if (typenode.InnerText == "locality")
                                        {
                                            isKhac = false;
                                            break;
                                        }
                                        if (typenode.InnerText == "administrative_area_level_1")
                                        {
                                            isKhac = false;
                                            obj.Tinh = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                        else if (typenode.InnerText == "administrative_area_level_2")
                                        {
                                            isKhac = false;
                                            obj.QuanHuyen = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                        else if (typenode.InnerText == "sublocality_level_1")
                                        {
                                            isKhac = false;
                                            obj.PhuongXa = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                        else if (typenode.InnerText == "route")
                                        {
                                            isKhac = false;
                                            obj.Duong = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                        else if (typenode.InnerText == "street_number")
                                        {
                                            isKhac = false;
                                            obj.SoNha = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                        else if (typenode.InnerText == "neighborhood")
                                        {
                                            isKhac = false;
                                            obj.LangBan = address_component.ChildNodes[0].InnerText;
                                            break;
                                        }
                                    }
                                    if (isKhac)
                                    {
                                        if (obj.Khac != null && obj.Khac.Length > 0)
                                        {
                                            obj.Khac += ", ";
                                        }
                                        obj.Khac += address_component.ChildNodes[0].InnerText;
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //log.Error(ex);
                    }

                    return obj;
                    //return dsResult.Tables["result"].Rows[0]["formatted_address"].ToString();
                }
            }
        }

        private static readonly string[] VietnameseSigns = new string[] {
        "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ"
    };

        public static string RemoveVietNameseSign(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 1; i < VietnameseSigns.Length; i++)
                {
                    for (int j = 0; j < VietnameseSigns[i].Length; j++)
                        str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
            }
            else
            {
                return string.Empty;
            }

            return str;
        }
    }
}
