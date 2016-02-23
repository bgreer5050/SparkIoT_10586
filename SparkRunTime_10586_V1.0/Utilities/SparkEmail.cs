using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SparkRunTime_10586_V1._0.Utilities
{
    public class SparkEmail
    {
        public static int intEmailCount;

        public static async void Send(string strBody)
        {

            SparkEmail.intEmailCount += 1;
            string strEmailMessage = "";
            bool blnSendEmail = true;

           
                if (SparkEmail.intEmailCount < 9)
                {

                     strEmailMessage = "to=bgreer@metal-matic.com&from=apollo@metal-matic.com&subject=Spark Issue&body=" + strBody;
                }
                else if(SparkEmail.intEmailCount == 9)
                {
                    strBody = Configuration.strAssetNumber + " Is generating too many emails.";
                    strEmailMessage = "to=bgreer@metal-matic.com&from=apollo@metal-matic.com&subject=Spark Issue&body=" + strBody;

                }
                else if(SparkEmail.intEmailCount > 9)
                {

                    blnSendEmail = false;
                }


                if (blnSendEmail)
                {
                try
                {
                    //create a request
                    var request = (HttpWebRequest)WebRequest.Create("http://10.0.100.16/Messaging/SendEmail");

                    byte[] buffer = Encoding.UTF8.GetBytes(strEmailMessage);


                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";


                    Task<Stream> postTask = request.GetRequestStreamAsync();

                    //We could do other stuff here since we have not called await on our task object yet.

                    Stream post = await postTask;
                    post.Write(buffer, 0, buffer.Length);
                    post.Flush();

                    Task<WebResponse> responseTask = request.GetResponseAsync();
                    WebResponse webResponse = await responseTask;

                    HttpWebResponse response = (HttpWebResponse)webResponse;
                    Stream responseStream = response.GetResponseStream();
                    Debug.Write(response.StatusCode.ToString());

                    StreamReader reader = new StreamReader(responseStream);
                    var responseValue = reader.ReadToEnd();


                }
            catch (Exception Ex)
            {

                Debug.Write(Ex.Message.ToString());

            } 
        }
        }
    }
}
