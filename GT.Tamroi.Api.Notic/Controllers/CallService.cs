using GT.Tamroi.Api.Notic.Models.Requests;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GT.Tamroi.Api.Notic.Controllers
{
    public class CallService
    {
        public DataTable Identify(string lastLatPoint, string lastLonPoint)
        {
            string URI = "http://search.nostramap.com/searchiden_th/search.asmx/Identify";
            string myParameters = "lat=" + lastLatPoint + "&lon=" + lastLonPoint + "&token=F@rT3stS3@rchTh@1&userKey=tamroi&ipClient=14";
            DataTable callIden = new DataTable();

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Encoding = UTF8Encoding.UTF8;
                string HtmlResult = wc.UploadString(URI, myParameters);
                StringReader theReader = new StringReader(HtmlResult);
                callIden.ReadXml(theReader);
            }

            if (callIden.Rows[0][0].ToString() == "NotFound")
            {
                callIden = null;
            }
            return callIden;
        }

        public bool CallTracking(TrackingResponse dataNotic, string imei)
        {
            try
            {
                var client = new RestClient("https://tamroi-test.nostramap.com/tamroi/realtime/api/Tracking/" + imei);
                var request = new RestRequest(Method.PUT);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(new { latitude = dataNotic.latitude, longitude = dataNotic.longitude, inout_status = dataNotic.inout_status, battery = dataNotic.battery, current_datetime = dataNotic.current_datetime, thai_address = dataNotic.thai_address, english_address = dataNotic.english_address , online_status = dataNotic.online_status });
                IRestResponse response = client.Execute(request);
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;
        }
        public bool sosSend(GetRequestDataRequests req)
        {
            try
            {
                bool checkticket = callMoServiceCheckTicket(req);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public bool callMoServiceCheckTicket(GetRequestDataRequests req)
        {
            try
            {

                int countPosition = 0;
                double lastLatPoint = 0;
                double lastLonPoint = 0;
            

                int deviceId = Convert.ToInt32(req.deviceId);
                if(req.position.Count > 0)
                {
                    countPosition = req.position.Count() - 1;
                    lastLatPoint = req.position[countPosition].latitude;
                    lastLonPoint = req.position[countPosition].longitude;
                }
      
                var url_check_ticket = new RestClient("https://tamroi-test.nostramap.com/node/tamroi/api/sosdevice/devicesend");
                var request_check_ticket = new RestRequest(Method.POST);
                request_check_ticket.AddHeader("content-type", "application/json");
                request_check_ticket.AddHeader("x-auth-token", "bearer  5aa8c34731a4ff7c7e823e3fe8e448812b6d7bac");
                request_check_ticket.RequestFormat = DataFormat.Json;
                request_check_ticket.AddBody(new { deviceid = deviceId, latitude = lastLatPoint, longitude = lastLonPoint});
                IRestResponse response_request_ticket = url_check_ticket.Execute(request_check_ticket);

                var resultCheckTicket = JsonConvert.DeserializeObject<sosServiceResponse>(response_request_ticket.Content);
                bool ippush = Convert.ToBoolean(resultCheckTicket.ispush);

                bool sendSos = false;

                if (ippush == true)
                {
                    sendSos = callMoServiceSendSos(resultCheckTicket, req, lastLatPoint, lastLonPoint );
                }


                if(sendSos == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }   
            }catch(Exception ex)
            {
                return false;
            }
        }
        public bool callMoServiceSendSos(sosServiceResponse resultCheckTicket, GetRequestDataRequests req, double lastLatPoint, double lastLonPoint)
        {
            try
            {
                //MailCustom mailM = new MailCustom("005683@cdg.co.th", "patchaya.sonta01@gmail.com", "bayernmunichm21");
                //mailM.sendMail("aaaa", JsonConvert.SerializeObject(req));

                string id = resultCheckTicket.id.ToString();

                SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@groupId", "xx") };

                SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_password"].ToString());

                string deviceQuery = @"select * FROM [Tamroi_App].[tamroi].[Device] WHERE Id IN(" + req.deviceId + ")";
                DataTable deviceDetail = sqlServerDatabase.SelectData(sqlCon, deviceQuery, sqlParameters);

                string get_imei = deviceDetail.Rows[0]["IMEI"].ToString().Trim();
                string deviceName = deviceDetail.Rows[0]["DeviceName"].ToString().Trim();
                string ticketId = id.ToString();

                //call notification GOT MOBILE
                var url_send_sos_mob = new RestClient("https://tamroi-test.nostramap.com/node/notification/send/sos");
                var request_Call_Mobservice = new RestRequest(Method.POST);
                request_Call_Mobservice.AddHeader("content-type", "application/json");
                request_Call_Mobservice.AddHeader("x-auth-token", "bearer  5aa8c34731a4ff7c7e823e3fe8e448812b6d7bac");
                request_Call_Mobservice.RequestFormat = DataFormat.Json;
                request_Call_Mobservice.AddBody(new { imei = get_imei, devicename = deviceName , ticketid = ticketId });
                IRestResponse response = url_send_sos_mob.Execute(request_Call_Mobservice);

                //call notification GOT WEB
                bool statusCallNotiweb = callWebServiceNoticfication(get_imei, deviceName,req.userId, ticketId, lastLatPoint, lastLonPoint);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool callWebServiceNoticfication(string imei ,string deviceName , string userId , string ticketId_mob , double lat, double lon )
        {
            try
            {

                SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@groupId", "xx") };

                SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                            , ConfigurationSettings.AppSettings["db_password"].ToString());

                string deviceQuery = @"select * FROM [Tamroi_App].[tamroi].[Ticket] WHERE Id IN(" + ticketId_mob + ")";
                DataTable deviceDetail = sqlServerDatabase.SelectData(sqlCon, deviceQuery, sqlParameters);
                string ticketSOSId = deviceDetail.Rows[0]["TicketSOSId"].ToString();

                var client = new RestClient("https://tamroi-test.nostramap.com/tamroi/realtime/api/emergency/" + imei);
                var request = new RestRequest(Method.PUT);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(new { devicename = deviceName, userid = userId, ticketid = ticketSOSId, latitude = lat, longitude = lon, status = "WT" });
                IRestResponse response = client.Execute(request);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool callMobilServiceEvent(DataTable datafromactivitylogcheck)
        {
            try
            {
                var url_send_sos_mob = new RestClient("https://tamroi-test.nostramap.com/node/notification/send/event");
                var request_Call_Mobservice = new RestRequest(Method.POST);
                request_Call_Mobservice.AddHeader("content-type", "application/json");
                request_Call_Mobservice.AddHeader("x-auth-token", "bearer 5aa8c34731a4ff7c7e823e3fe8e448812b6d7bac");
                request_Call_Mobservice.RequestFormat = DataFormat.Json;

                int checkPushSendEvent = Convert.ToInt32(datafromactivitylogcheck.Rows[0]["pusheventstatus"]);

                // ถ้า checkPushSendEvent เป็น 1 คือ ให้มีการ push ข้อมูล Event , 0 คือไม่มีการ Push ข้อมูล Event 
                if (checkPushSendEvent == 1 )
                {
                    request_Call_Mobservice.AddBody(new
                    {
                        imei = datafromactivitylogcheck.Rows[0]["imei"].ToString(),
                        devicename = datafromactivitylogcheck.Rows[0]["devicename"].ToString(),
                        eventid = datafromactivitylogcheck.Rows[0]["eventid"].ToString(),
                        deviceid = datafromactivitylogcheck.Rows[0]["deviceid"].ToString()
                    });
                    IRestResponse response = url_send_sos_mob.Execute(request_Call_Mobservice);
                } 
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }       
    }
}