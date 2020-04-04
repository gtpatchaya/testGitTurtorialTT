using GT.Tamroi.Api.Notic.Controllers.Helpers;
using GT.Tamroi.Api.Notic.Models;
using GT.Tamroi.Api.Notic.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Windows;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using RestSharp;
using DataFormat = RestSharp.DataFormat;
using Newtonsoft.Json.Linq;

namespace GT.Tamroi.Api.Notic.Controllers
{
    public class CheckPointSendNoticController : ApiController
    {
        Authen _authen = new Authen();
        // GET: SetData
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("data/CheckPointSendNotic")]
        public IHttpActionResult CheckPointSendNotic([FromBody] GetRequestDataRequests req)
        {
            int stage;
            bool debugEmail = Convert.ToBoolean(ConfigurationManager.AppSettings["debugEmail"]);
            if (debugEmail == true)
            {
                MailCustom mailM = new MailCustom("005683@cdg.co.th", "patchaya.sonta01@gmail.com", "bayernmunichm21");
                mailM.sendMail("DeviceDataChanged", JsonConvert.SerializeObject(req));
            }
       
            #region Check Token
            var re = Request;
            var headers = re.Headers;
            string tokenHeader = "";
            if (headers.Contains("Authorization"))
            {
                tokenHeader = headers.Authorization.Parameter;
            }
                DeTokenProperties detoken = new DeTokenProperties();
                detoken.inputIp = GetUser_IP();
                detoken.inputToken = tokenHeader;
            if (!_authen.CheckToken(ref detoken))
            {
                return Unauthorized();
}
            #endregion
 
            try
            {
                TrackingResponse traksend = new TrackingResponse();
                LogDatabase updateLog = new LogDatabase();
                CallService callService = new CallService();

                int countPosition = 0;
                string lastLatPoint, lastLonPoint, areaStatusInOut, lastDifCurrent, inOutSaveZone, a;
                string returnStatus = null;
                string nameTh = string.Empty; 
                string nameEn = string.Empty; 

                bool NoPosition = false;

                DataTable dtResultIden = new DataTable();
                DataTable dtResultCallInOut = new DataTable();
                DataTable dtUpdateDeviceCurrent = new DataTable();
                DataTable dtInsertDataToLocationLog = new DataTable();
                DataTable dtReUpdateDeviceCurrent = new DataTable();
                DataTable dtInsertActivityLog = new DataTable();

                dtResultIden = null;
                dtReUpdateDeviceCurrent = null;
                dtInsertActivityLog = null;

                //ตรวจสอบข้อมูล เเละทำการส่ง SOS
                if (req.sos == 1)
                {
                    bool checkHaveTicket = callService.sosSend(req);

                }

                stage = 0;

                //ตรวจสอบมีจุดส่งมาหรือไม่ ถ้าไม่มีไม่ต้องไป Iden
                // ** จุดไม่มี , จุดมี
                if (req.position.Count() > 0)
                {
                    countPosition = req.position.Count() - 1;
                    lastLatPoint = req.position[countPosition].latitude.ToString();
                    lastLonPoint = req.position[countPosition].longitude.ToString();
                    dtResultIden = callService.Identify(lastLatPoint, lastLonPoint);
                    dtResultCallInOut = CheckInOutArea(lastLatPoint, lastLonPoint, req.deviceId);
                }
                else
                {
                    NoPosition = true;
                    lastLatPoint = string.Empty;
                    lastLonPoint = string.Empty;
                    dtResultCallInOut.Columns.Add("AreaStatus");
                    dtResultCallInOut.Columns.Add("AreaId");
                    DataRow row = dtResultCallInOut.NewRow();
                    row["AreaStatus"] = null;
                    row["AreaId"] = null;
                    dtResultCallInOut.Rows.Add(row);

                }
                
                if (dtResultCallInOut.Rows[0]["AreaStatus"].ToString() == "0" || dtResultCallInOut.Rows[0]["AreaStatus"].ToString() == null)
                {
                    dtResultCallInOut.Rows[0]["AreaStatus"] = DBNull.Value;
                }

                if (dtResultCallInOut.Rows[0]["AreaId"].ToString() == "0" || dtResultCallInOut.Rows[0]["AreaId"].ToString() == null)
                {
                    dtResultCallInOut.Rows[0]["AreaId"] = DBNull.Value;
                }


                //Call service :: Push Notification Event by Device
                // Online Offline 
                dtInsertActivityLog = updateLog.InsertActivityLog(req,"onlineoffline",dtResultCallInOut.Rows[0]["AreaStatus"].ToString());        
                bool dtPushEvent = callService.callMobilServiceEvent(dtInsertActivityLog);
                //battery Level
               
                dtInsertActivityLog = updateLog.InsertActivityLog(req,"baterryLevel", dtResultCallInOut.Rows[0]["AreaStatus"].ToString());
                dtPushEvent = callService.callMobilServiceEvent(dtInsertActivityLog);

                // In out Area
                dtInsertActivityLog = updateLog.InsertActivityLog(req,"InoutArea", dtResultCallInOut.Rows[0]["AreaStatus"].ToString());
                dtPushEvent = callService.callMobilServiceEvent(dtInsertActivityLog);

                stage = 4;

                // req : ตัวที่ได้มาจาก Raw data dtResultIden : ได้มาจาก Identify , NoPosition : ใช้ตรวจสอบว่ามี ตำแหน่งส่งมาหรือไม่ , dtResultCallInOut : ได้มาจากการ Update InoutArea , dtInsertActivityLog ได้มาจากตัว set Activitylog
                dtInsertDataToLocationLog = updateLog.InsertDataToLocationLog(req, dtResultIden, NoPosition, dtResultCallInOut);
                stage = 5;

                dtReUpdateDeviceCurrent = UpdateDeviceCurrent(dtResultIden, req, NoPosition , dtResultCallInOut);
                stage = 6;

                //การ set ค่า input status ที่จะส่งไปที่ signalR    
                returnStatus = dtReUpdateDeviceCurrent.Rows[0]["AreaStatus"].ToString();

                if (dtResultIden != null)
                {
                    nameTh = dtReUpdateDeviceCurrent.Rows[0]["locationNameTh"].ToString();
                    nameEn = dtReUpdateDeviceCurrent.Rows[0]["locationNameEn"].ToString();
                    traksend.latitude = Convert.ToDouble(dtReUpdateDeviceCurrent.Rows[0]["Latitude"]);
                    traksend.longitude = Convert.ToDouble(dtReUpdateDeviceCurrent.Rows[0]["Longitude"]);
                    traksend.inout_status = returnStatus;

                    if (nameTh == "" || nameTh == null)
                    {
                        traksend.thai_address = dtReUpdateDeviceCurrent.Rows[0]["locationProvinceTh"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationDistrictTh"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationSubDistrictTh"].ToString();
                    }
                    else
                    {
                        traksend.thai_address = dtReUpdateDeviceCurrent.Rows[0]["locationNameTh"].ToString();
                    }

                    if (nameEn == "" || nameEn == null)
                    {
                        traksend.english_address = dtReUpdateDeviceCurrent.Rows[0]["locationProvinceEn"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationDistrictEn"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationSubDistrictEn"].ToString();
                    }
                    else
                    {
                        traksend.english_address = dtReUpdateDeviceCurrent.Rows[0]["locationNameEn"].ToString();
                    }

                    traksend.battery = Convert.ToInt32(dtReUpdateDeviceCurrent.Rows[0]["BatteryLevel"]);
                    traksend.current_datetime = dtReUpdateDeviceCurrent.Rows[0]["Timestamp"].ToString();
                    traksend.online_status = dtReUpdateDeviceCurrent.Rows[0]["OnlineStatus"].ToString();
                }
                else
                {   // ตรงนี้ไม่เข้านะ
                    if(countPosition > 0)
                    {                      
                        traksend.latitude = Convert.ToDouble(req.position[countPosition].latitude);
                        traksend.longitude = Convert.ToDouble(req.position[countPosition].longitude);
                        traksend.thai_address = traksend.latitude.ToString()+","+ traksend.longitude.ToString();
                        traksend.english_address = traksend.latitude.ToString() + "," + traksend.longitude.ToString();

                        traksend.inout_status = returnStatus;
                        traksend.battery = Convert.ToInt32(req.batteryLevel);
                        traksend.current_datetime = req.position[countPosition].gpsDateTime.ToString();
                        traksend.online_status = dtReUpdateDeviceCurrent.Rows[0]["OnlineStatus"].ToString();
                    }
                    else
                    {
                        traksend.latitude = Convert.ToDouble(dtReUpdateDeviceCurrent.Rows[0]["Latitude"]);
                        traksend.longitude = Convert.ToDouble(dtReUpdateDeviceCurrent.Rows[0]["Longitude"]);
                        traksend.thai_address = dtReUpdateDeviceCurrent.Rows[0]["locationProvinceTh"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationDistrictTh"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationSubDistrictTh"].ToString();
                        traksend.english_address = dtReUpdateDeviceCurrent.Rows[0]["locationProvinceEn"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationDistrictEn"].ToString() + "," + dtReUpdateDeviceCurrent.Rows[0]["locationSubDistrictEn"].ToString();

                        traksend.battery = Convert.ToInt32(req.batteryLevel);
                        traksend.inout_status = returnStatus;
                        traksend.current_datetime = dtReUpdateDeviceCurrent.Rows[0]["Timestamp"].ToString();
                        traksend.online_status = dtReUpdateDeviceCurrent.Rows[0]["OnlineStatus"].ToString();
                    }
                }

                //Call service :: Signal R 
                bool Recalltrack = callService.CallTracking(traksend, req.imei);
                stage = 7;

                ReaponseNotic resPonse = new ReaponseNotic();
                resPonse.imei = req.imei;
                resPonse.deviceId = req.deviceId;
                resPonse.stage = stage;
                resPonse.userId = req.userId;
                return Ok(resPonse);
            }
            catch (Exception ex )
            {
                //MailCustom mailM = new MailCustom("gtpatchaya@gmail.com", "patchayamonitor@gmail.com", "gtpatchaya@005853");
                //mailM.sendMail("tamroi:error", ex.ToString());
                return InternalServerError(ex);
            }
        }
        
        public DataTable CheckInOutArea(string lastLatPoint, string lastLonPoint, string deviceid)
        {


            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
            SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_password"].ToString());
            SqlParameter[] sqlParameters = new SqlParameter[]{
                                                                                new SqlParameter("@lat",lastLatPoint),
                                                                                new SqlParameter("@lon",lastLonPoint),
                                                                                new SqlParameter("@deviceId",deviceid)
                                                                             };
            sqlCon.Open();
            string query = "tamroi.store_UpdateInOutStatus";
            DataTable dt = sqlServerDatabase.GetDataTableFromStore(sqlCon, query, sqlParameters);
            sqlCon.Close();

            return dt;
        }
        public DataTable UpdateDeviceCurrent(DataTable dtResultIden, GetRequestDataRequests rq, bool NoPosition , DataTable reqdtResultCallInOut )
        {

            //SET BATERY LEVEL 
            //string batupdate = reqbatupdate.Rows[0]["batupdate"].ToString();
            //string batvalue = "";
            //if (batupdate == "TRUE")
            //{
            //    // กรณีมีการ Update ข้อมูล แบตจาก Activity Log จะเอาออกมาpdate ที่ table current status 
            //    batvalue = reqbatupdate.Rows[0]["batvalue"].ToString();
            //}
            //else if (batupdate == "FALSE")
            //{
            //    // กรณีไม่มีการ Update ข้อมูล แบตจาก Activity Log
            //    batvalue = rq.batteryLevel.ToString();
            //}


            DataTable dt = new DataTable();

            //set ค่าที่ได้มาจาก Device 
            int countPosition = rq.position.Count() - 1;
            string DeviceId = rq.deviceId.ToString();
            string currentfrequency = rq.currentFrequency.ToString();  
            string onlineStatus = rq.onlineStatus.ToString();
            string BatteryLevel = rq.batteryLevel.ToString();
            string accuracy = checknull_setnull_todatabase(rq.accuracy);
            string location_type = checknull_setnull_todatabase(rq.location_type);
            string SignalStrength = checknull_setnull_todatabase(rq.signalGps);
           

            string Latitude = null;
            string Longitude = null;
            string locationNameTh = null;
            string locationNameEn = null;
            string locationProvinceTh = null;
            string locationDistrictTh = null;
            string locationSubDistrictTh = null;
            string locationProvinceEn = null;
            string locationDistrictEn = null;
            string locationSubDistrictEn = null;

            string areaStatus = checknull_setnull_todatabase(reqdtResultCallInOut.Rows[0]["AreaStatus"].ToString());
            string areaId = checknull_setnull_todatabase(reqdtResultCallInOut.Rows[0]["AreaId"].ToString());

            if (NoPosition == false)
            {
                Latitude = rq.position[countPosition].latitude.ToString();
                Longitude = rq.position[countPosition].longitude.ToString();
            }

            if (dtResultIden != null)
            {
                locationNameTh = dtResultIden.Rows[0]["Name_L"].ToString();
                locationNameEn = dtResultIden.Rows[0]["Name_E"].ToString();
                locationProvinceTh = dtResultIden.Rows[0]["AdminLevel1_L"].ToString();
                locationDistrictTh = dtResultIden.Rows[0]["AdminLevel2_L"].ToString();
                locationSubDistrictTh = dtResultIden.Rows[0]["AdminLevel3_L"].ToString();
                locationProvinceEn = dtResultIden.Rows[0]["AdminLevel1_E"].ToString();
                locationDistrictEn = dtResultIden.Rows[0]["AdminLevel2_E"].ToString();
                locationSubDistrictEn = dtResultIden.Rows[0]["AdminLevel3_E"].ToString();
            }


            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
            SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_password"].ToString());

            SqlParameter[] sqlParameters = new SqlParameter[]{
                                                                                new SqlParameter("@DeviceId",DeviceId),
                                                                                new SqlParameter("@Latitude",Latitude == null? (object)DBNull.Value : Latitude),
                                                                                new SqlParameter("@Longitude",Longitude== null? (object)DBNull.Value : Longitude),
                                                                                new SqlParameter("@BatteryLevel",BatteryLevel== null? (object)DBNull.Value : BatteryLevel),
                                                                                new SqlParameter("@SignalStrength",SignalStrength == null? (object)DBNull.Value : SignalStrength  ),
                                                                                new SqlParameter("@locationNameTh",locationNameTh== null? (object)DBNull.Value : locationNameTh),
                                                                                new SqlParameter("@locationNameEn",locationNameEn== null? (object)DBNull.Value : locationNameEn),
                                                                                new SqlParameter("@locationProvinceTh",locationProvinceTh== null? (object)DBNull.Value : locationProvinceTh),
                                                                                new SqlParameter("@locationDistrictTh",locationDistrictTh== null? (object)DBNull.Value : locationDistrictTh),
                                                                                new SqlParameter("@locationSubDistrictTh",locationSubDistrictTh== null? (object)DBNull.Value : locationSubDistrictTh),
                                                                                new SqlParameter("@locationProvinceEn",locationProvinceEn== null? (object)DBNull.Value : locationProvinceEn),
                                                                                new SqlParameter("@locationDistrictEn",locationDistrictEn== null? (object)DBNull.Value : locationDistrictEn),
                                                                                new SqlParameter("@locationSubDistrictEn",locationSubDistrictEn == null? (object)DBNull.Value : locationSubDistrictEn),
                                                                                new SqlParameter("@currentfrequency",currentfrequency == null? (object)DBNull.Value : currentfrequency),
                                                                                new SqlParameter("@onlineStatus",onlineStatus== null? (object)DBNull.Value : onlineStatus),
                                                                                new SqlParameter("@accuracy",accuracy== null? (object)DBNull.Value : accuracy),
                                                                                new SqlParameter("@location_type",location_type== null? (object)DBNull.Value : location_type),

                                                                                new SqlParameter("@areaId",areaId== null? (object)DBNull.Value : areaId),
                                                                                new SqlParameter("@areaStatus",areaStatus== null? (object)DBNull.Value : areaStatus)
                                                                               
                                                                             };
                sqlCon.Open();
                string query = "tamroi.store_UpdateDeviceCurrentStatus";
                dt = sqlServerDatabase.GetDataTableFromStore(sqlCon, query, sqlParameters);
                sqlCon.Close();
            return dt;
        }
        protected string GetUser_IP()
        {
            string VisitorsIPAddr = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
            }
            return VisitorsIPAddr;
        } 

        public string checknull_setnull_todatabase(string data)
        {
            if(data == null)
            {
                return null;
            }
            else
            {
                return data;
            }
        }
       


    }
}