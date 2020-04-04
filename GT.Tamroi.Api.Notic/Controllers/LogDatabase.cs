using GT.Tamroi.Api.Notic.Models.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Controllers
{
    public class LogDatabase
    {
        public DataTable InsertDataToLocationLog(GetRequestDataRequests rq, DataTable rqIden, bool NoPosition , DataTable reqtableCurrent)
        {
            DataTable dtlog = new DataTable();
            DataRow dtlogRow;

            dtlog.Columns.Add("UserMobileId", typeof(String));
            dtlog.Columns.Add("DeviceId", typeof(String));
            dtlog.Columns.Add("Latitude", typeof(String));
            dtlog.Columns.Add("Longitude", typeof(String));
            dtlog.Columns.Add("DeviceDateTime", typeof(String));
            dtlog.Columns.Add("Battery", typeof(String));
            dtlog.Columns.Add("SOS", typeof(String));
            dtlog.Columns.Add("Device_Brand_Id", typeof(String));
            dtlog.Columns.Add("Source_Id", typeof(String));
            dtlog.Columns.Add("locationNameTh", typeof(String));
            dtlog.Columns.Add("locationNameEn", typeof(String));
            dtlog.Columns.Add("locationProvinceTh", typeof(String));
            dtlog.Columns.Add("locationDistrictTh", typeof(String));
            dtlog.Columns.Add("locationSubDistrictTh", typeof(String));
            dtlog.Columns.Add("locationProvinceEn", typeof(String));
            dtlog.Columns.Add("locationDistrictEn", typeof(String));
            dtlog.Columns.Add("locationSubDistrictEn", typeof(String));
            dtlog.Columns.Add("GPSAcc", typeof(String));
            dtlog.Columns.Add("AreaId", typeof(String));
            dtlog.Columns.Add("AreaStatus", typeof(String));
            dtlog.Columns.Add("Location_type", typeof(String));

            // ค่านี้ทุกตัวต้องมีเหมือนกันหมด
            //dtlogRow = dtlog.NewRow();
            //dtlogRow["UserMobileId"] = rq.userId.ToString();
            //dtlogRow["Battery"] = rq.batteryLevel.ToString();
            //dtlogRow["DeviceId"] = rq.deviceId.ToString();
            //dtlogRow["SOS"] = rq.sos.ToString();
            //dtlogRow["Device_Brand_Id"] = rq.device_Brand_Id.ToString(); ;
            //dtlogRow["Source_Id"] = rq.source_Id.ToString();
            //dtlogRow["GPSAcc"] = null;
            //dtlogRow["Accuracy"] = rq.accuracy.ToString();
            //dtlogRow["Location_type"] = rq.location_type.ToString();
            CheckPointSendNoticController cheknull = new CheckPointSendNoticController();

            //กรณีมี Position เเละ Iden ได้
            if (NoPosition == false && rqIden != null)
            {
                for (int i = 0; i < rq.position.Count(); i++)
                {
                    dtlogRow = dtlog.NewRow();
                    dtlogRow["UserMobileId"] = rq.userId.ToString();
                    dtlogRow["Battery"] = rq.batteryLevel.ToString();
                    dtlogRow["DeviceId"] = rq.deviceId.ToString();
                    dtlogRow["SOS"] = rq.sos.ToString();
                    dtlogRow["Device_Brand_Id"] = rq.device_Brand_Id.ToString(); ;
                    dtlogRow["Source_Id"] = rq.source_Id.ToString();
                    dtlogRow["GPSAcc"] = cheknull.checknull_setnull_todatabase(rq.accuracy);
                    dtlogRow["Location_type"] = cheknull.checknull_setnull_todatabase(rq.location_type);
                    

                    dtlogRow["Latitude"] = rq.position[i].latitude.ToString();
                    dtlogRow["Longitude"] = rq.position[i].longitude.ToString();
                    dtlogRow["DeviceDateTime"] = rq.position[i].gpsDateTime.ToString();

                    if (reqtableCurrent != null)
                    {
                        if (reqtableCurrent.Rows[0]["AreaId"].ToString() == "")
                        {
                            dtlogRow["AreaId"] = null;
                        }
                        else
                        {
                            dtlogRow["AreaId"] = reqtableCurrent.Rows[0]["AreaId"].ToString();
                        }

                        if (reqtableCurrent.Rows[0]["AreaStatus"].ToString() == "")
                        {
                            dtlogRow["AreaStatus"] = null;
                        }
                        else
                        {
                            dtlogRow["AreaStatus"] = reqtableCurrent.Rows[0]["AreaStatus"].ToString();
                        }
                    }

                    //บรรทัดสุดท้ายใส่ที่ Iden ลงไป
                    if (i == rq.position.Count() - 1)
                    {
                        dtlogRow["locationNameTh"] = rqIden.Rows[0]["Name_L"].ToString();
                        dtlogRow["locationNameEn"] = rqIden.Rows[0]["Name_E"].ToString();
                        dtlogRow["locationProvinceTh"] = rqIden.Rows[0]["AdminLevel1_L"].ToString();
                        dtlogRow["locationDistrictTh"] = rqIden.Rows[0]["AdminLevel2_L"].ToString();
                        dtlogRow["locationSubDistrictTh"] = rqIden.Rows[0]["AdminLevel3_L"].ToString();
                        dtlogRow["locationProvinceEn"] = rqIden.Rows[0]["AdminLevel1_E"].ToString();
                        dtlogRow["locationDistrictEn"] = rqIden.Rows[0]["AdminLevel2_E"].ToString();
                        dtlogRow["locationSubDistrictEn"] = rqIden.Rows[0]["AdminLevel3_E"].ToString();
                    }

                    dtlog.Rows.Add(dtlogRow);
                    
                }
            }
            //กรณีมี Position เเละ Iden ไม่ได้
            else if (NoPosition == false && rqIden == null)
            {
                for (int i = 0; i < rq.position.Count(); i++)
                {
                    dtlogRow = dtlog.NewRow();
                    dtlogRow["UserMobileId"] = rq.userId.ToString();
                    dtlogRow["Battery"] = rq.batteryLevel.ToString();
                    dtlogRow["DeviceId"] = rq.deviceId.ToString();
                    dtlogRow["SOS"] = rq.sos.ToString();
                    dtlogRow["Device_Brand_Id"] = rq.device_Brand_Id.ToString(); ;
                    dtlogRow["Source_Id"] = rq.source_Id.ToString();
                    dtlogRow["GPSAcc"] = cheknull.checknull_setnull_todatabase(rq.accuracy);
                    dtlogRow["Location_type"] = cheknull.checknull_setnull_todatabase(rq.location_type);

                    dtlogRow["Latitude"] = rq.position[i].latitude.ToString();
                    dtlogRow["Longitude"] = rq.position[i].longitude.ToString();
                    dtlogRow["DeviceDateTime"] = rq.position[i].gpsDateTime.ToString();

                        if (reqtableCurrent != null)
                        {
                            if(reqtableCurrent.Rows[0]["AreaId"].ToString() == "")
                            {
                                dtlogRow["AreaId"] = null;
                            }
                            else
                            {
                                dtlogRow["AreaId"] = reqtableCurrent.Rows[0]["AreaId"].ToString();
                            }

                            if(reqtableCurrent.Rows[0]["AreaStatus"].ToString() == "")
                            {
                                dtlogRow["AreaStatus"] = null;
                            }
                            else
                            {
                                dtlogRow["AreaStatus"] = reqtableCurrent.Rows[0]["AreaStatus"].ToString();
                            }
                        }

                    dtlog.Rows.Add(dtlogRow);
                }
            }
            //กรณีมี ไม่มี Position
            else if (NoPosition == true)
            {
                dtlogRow = dtlog.NewRow();
                dtlogRow["UserMobileId"] = rq.userId.ToString();
                dtlogRow["Battery"] = rq.batteryLevel.ToString();
                dtlogRow["DeviceId"] = rq.deviceId.ToString();
                dtlogRow["SOS"] = rq.sos.ToString();
                dtlogRow["Device_Brand_Id"] = rq.device_Brand_Id.ToString(); ;
                dtlogRow["Source_Id"] = rq.source_Id.ToString();
                dtlogRow["GPSAcc"] = cheknull.checknull_setnull_todatabase(rq.accuracy);
                dtlogRow["Location_type"] = cheknull.checknull_setnull_todatabase(rq.location_type);

                dtlog.Rows.Add(dtlogRow);
            }


            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
            SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_password"].ToString());
            SqlParameter[] sqlParameters = new SqlParameter[]{
                                                                                new SqlParameter("@dtlog",dtlog),
                                                                             };
            sqlCon.Open();
            string query = "tamroi.store_InsertLoglocation";
            DataTable dt = sqlServerDatabase.GetDataTableFromStore(sqlCon, query, sqlParameters);
            sqlCon.Close();
            return dt;
        }

        public DataTable InsertActivityLog(GetRequestDataRequests rq,string status , string checkInput)
        {
            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase();
            SqlConnection sqlCon = sqlServerDatabase.GetSqlConnection(ConfigurationSettings.AppSettings["db_server"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_databaseName"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_user"].ToString()
                                                                        , ConfigurationSettings.AppSettings["db_password"].ToString());
            SqlParameter[] sqlParameters = new SqlParameter[]{  
                                                                        new SqlParameter("@reqUser_Id",rq.userId),
                                                                        new SqlParameter("@reqDevice_Id",rq.deviceId),
                                                                        new SqlParameter("@reqBattery",rq.batteryLevel),
                                                                        new SqlParameter("@reqOnlineOffline_Id",rq.onlineStatus),
                                                                        new SqlParameter("@reqDeviceBrans_Id",rq.device_Brand_Id),
                                                                        new SqlParameter("@reqcheckActivity",status),
                                                                        new SqlParameter("@reqInOutArea",checkInput),
                                                                        };
            sqlCon.Open();
            string query = "tamroi.store_SaveActivityLog";
            DataTable dt = sqlServerDatabase.GetDataTableFromStore(sqlCon, query, sqlParameters);
            sqlCon.Close();
            return dt;
        }
    }
}