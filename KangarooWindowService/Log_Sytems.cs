using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KangarooWindowService
{
    class Log_Sytems
    {
        private string sFormat;
        private string sTime;
        private string path = "";

        public Log_Sytems()
        {
            sFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " --- ";

            sTime = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;

        }

        public void ErrorLog(string sErrMsg, string stackTrace)
        {
            try
            {
                string name = "";
                string user = "";
                if (Environment.MachineName != null)
                    name = Environment.MachineName;
                if (Environment.UserName != null)
                    user = Environment.UserName;

                string curFileName = "LogKangarooService";
                string folPath = Directory.GetCurrentDirectory() + "\\LogKangarooService";
                if (!Directory.Exists(folPath))
                {
                    System.IO.Directory.CreateDirectory(folPath);
                    path = folPath;
                }
                else
                {
                    path = folPath;
                }


                bool _FileUse = false;
                while (!_FileUse)
                {
                    try
                    {

                        string filePath = path + "\\" + sTime + "_" + curFileName + "_Error.txt";
                        string Content = "";
                        if (File.Exists(filePath))
                        {
                            StreamReader sr = new StreamReader(filePath);
                            Content = sr.ReadToEnd();
                            sr.Close();
                        }
                        string Err = "";
                        Err += name + "/" + user + "####" + sFormat + " " + sErrMsg;
                        Err += "\r\n" + "Stack Trace :" + stackTrace;
                        Err += "\r\n";

                        Err += Content;
                        StreamWriter strWriter = new StreamWriter(filePath, false);
                        strWriter.Write(Err);

                        strWriter.AutoFlush = true;
                        strWriter.Flush();
                        strWriter.Dispose();
                        strWriter.Close();

                        _FileUse = true;



                    }
                    catch (Exception ex)
                    {
                        _FileUse = false;
                        throw ex;
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
