using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Services
{
    using EAPMessage.Receive;
    using EventManager;
    using EventManager.EventArguments;
    using System.IO;
    using Utilities.ExtensionPlug;

    public class RequestStripMapService : SOAPServiceBase
    {
        #region Private Field

        private SOAPRequest mParent;

        #endregion

        #region Public Field

        public event EventDelegates.ServiceEventHandler OnMapRetrieved;

        #endregion

        #region Constructor

        public RequestStripMapService(Helper helper, Logger logger, Dictionary<string, string> webServicePathDict, SOAPRequest parent) : base(helper, logger, webServicePathDict)
        {
            mParent = parent;
        }

        #endregion

        #region Public Method

        public void PerformRetrieveStripMapAndReply(RequestStripMap requestStripMapMessage)
        {
            Task.Run(new Action(() => RetrieveStripMapAndReply(requestStripMapMessage)));
        }

        public bool IsStripMapExist(string stripId)
        {
            try
            {
                string soapResult = RequestService("RetrieveStripMap", stripId);

                Logger.LogHelper.LogDebug("soapResult={0}".FillArguments(soapResult));

                if (soapResult.Contains("FAIL"))
                {
                    if (soapResult.Contains("There is no row at position 0"))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return true;
            }
        }

        public bool IsStripMapWithCorrectLotId(string lotId, string stripId, ref string otherLotId)
        {
            try
            {
                string soapResult = RequestService("RetrieveStripMapWithLotId", lotId+":"+stripId);

                Logger.LogHelper.LogDebug("soapResult={0}".FillArguments(soapResult));

                if (soapResult.Contains("FAIL"))
                {
                    if (soapResult.Contains("STRIP BELONG TO"))
                    {
                        var splitMsg = soapResult.Split(' ');
                        otherLotId = splitMsg[splitMsg.Length - 1];
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return true;
            }
        }

        public bool IsStripMapWithCorrectChildLotId(string lotId, string stripId)
        {
            try
            {
                string soapResult = RequestService("RetrieveStripMapWithLotId", lotId + ":" + stripId);

                Logger.LogHelper.LogDebug("soapResult={0}".FillArguments(soapResult));

                if (soapResult.Contains("FAIL"))
                {
                    if (soapResult.Contains("There is no row at position 0"))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return true;
            }
        }

        public string RetrieveEMapLocation(string stripId)
        {
            try
            {
                string EMapResult = RequestService("RetrieveEMapLoc", stripId);

                Logger.LogHelper.LogDebug("soapResult={0}".FillArguments(EMapResult));

                return EMapResult;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return null;
            }
        }

        public string GetFWLotID(string parameter)
        {
            try
            {
                string soapResult = RequestService("GetFWLotID", parameter);
                Logger.LogHelper.LogDebug("soapResult=" + soapResult);
                return soapResult;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return ex.Message;
            }
        }
        #endregion

        #region Private Method

        private void RetrieveStripMapAndReply(RequestStripMap requestStripMapMessage)
        {
            try
            {
                string soapResult = RequestService("RetrieveStripMap", requestStripMapMessage.StripID);
                RequestStripMap request = new RequestStripMap(false);
                if (!string.IsNullOrEmpty(soapResult))
                {
                    RetrieveStripMapInfo(soapResult, requestStripMapMessage);
                    //request.CompileReplyData();
                    //mParent.AddStripMap(request);
                }
                else
                {
                    request.CompileReplyData();
                    mParent.SendMessage(request);
                }

                //mParent.SendMessage(request);
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
            }
        }

        private RequestStripMap RetrieveStripMapInfo(string soapResponse, RequestStripMap request)
        {
            try
            {
                int ur = 0;
                int uc = 0;
                int cr = 0;
                int cc = 0;
                string mapFile = "";
                string[] temp = soapResponse.Split('@');
                string[] usetting = null;
                string[] csetting = null;
                string lotId = "";
                mapFile = temp[0];
                lotId = temp[3];

                Logger.LogHelper.LogDebug("Splitting content...");
                foreach (string content in temp)
                {
                    if (content.Contains("UR") && content.Contains("UC"))
                    {
                        usetting = content.Split(',');
                    }
                    else if (content.Contains("CR") && content.Contains("CC"))
                    {
                        csetting = content.Split(','); 
                    }
                }
                string returnResult;

                Logger.LogHelper.LogDebug("Extracting content...");
                returnResult = ExtractRowColumn(usetting, "UR");
                if (!string.IsNullOrEmpty(returnResult))
                {
                    ur = Convert.ToInt32(returnResult);
                }

                returnResult = ExtractRowColumn(usetting, "UC");
                if (!string.IsNullOrEmpty(returnResult))
                {
                    uc = Convert.ToInt32(returnResult);
                }
                 
                returnResult = ExtractRowColumn(usetting, "CR");
                if (!string.IsNullOrEmpty(returnResult))
                {
                    cr = Convert.ToInt32(returnResult);
                }

                returnResult = ExtractRowColumn(usetting, "CC");
                if (!string.IsNullOrEmpty(returnResult))
                {
                    cc = Convert.ToInt32(returnResult);
                }

                request.ReplyMessage = new RequestStripMap.Reply();
                request.ReplyMessage.Result = true;
                request.ReplyMessage.StripID = request.StripID;
                request.ReplyMessage.Row = ur * cr;
                request.ReplyMessage.Column = uc * cc;
                request.ReplyMessage.OriginLocation = 1;
                request.ReplyMessage.MapFile = mapFile;
                request.ReplyMessage.LotID = lotId;
                var defectText = RetrieveEMapLocation(request.StripID);
                request.ReplyMessage.EMapLoc = string.IsNullOrEmpty(defectText.Trim()) ? null : HandleEmapDefectCode(defectText);
                string stripMapExistPath;
                string stripMapFromSharedDrive = string.Empty;
                if (!string.IsNullOrEmpty(mParent.DriveMapPath) && !string.IsNullOrEmpty(mParent.DriveUploadPath))
                {
                    stripMapExistPath = mParent.DriveMapPath + mParent.DriveUploadPath;
                    stripMapFromSharedDrive = ReadStripMapFromSharedDrive(stripMapExistPath, request.StripID);
                }
                request.ReplyMessage.stripMapData = stripMapFromSharedDrive;
                Logger.LogHelper.LogDebug("Firing OnMapRetrieved...");
                var args = new ServiceArgs(request.FWEquipmentID, request.EquipmentID, request.StripID, mapFile, uc, ur, cc, cr, request);
                OnMapRetrieved?.Invoke(this, args);

                return request;
            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return null;
            }
        }

        private string ExtractRowColumn(string[] setting, string target)
        {
            string result = "";

            if (setting == null)
            {
                Logger.LogHelper.LogError("No info for {0}", target);
                return string.Empty;
            }

            foreach (string rc in setting)
            {
                var splitted = rc.Split(':');

                if (splitted[0] == target)
                {
                    result = splitted[1];

                    return result;
                }
                else
                {
                    result = "";
                }
            }

            return result;
        }

        private Tuple<int, int, string>[] HandleEmapDefectCode(string emap)
        {
            //var emapSplit = emap.Split(';');
            //List<EMapDefect> eMapDefect = new List<EMapDefect>();
            //foreach (string emapSplitter in emapSplit)
            //{
            //    string[] insertEMapDefect = emapSplitter.Split('@');
            //    EMapDefect defectInfo = new EMapDefect(insertEMapDefect[0], insertEMapDefect[1], insertEMapDefect[2]);
            //    eMapDefect.Add(defectInfo);
            //}
            //var returnTupleArray = eMapDefect.Select(x => { return new Tuple<int, int>(Convert.ToInt32(x.row), Convert.ToInt32(x.column)); });
            //return returnTupleArray.ToArray();

            //if (string.IsNullOrEmpty(emap.Trim()) || emap.Trim() == "")
            //{
            //    Logger.LogHelper.LogDebug("EMapLoc is empty, creating emptyDefect");
            //    Tuple<int, int>[] emptyDefect = { Tuple.Create(0, 0) };
            //    Logger.LogHelper.LogDebug("EMapLoc is empty, created emptyDefect");
            //    return emptyDefect;
            //}
            var defects =
                from a in emap.Split(';')
                select Tuple.Create(a.Split('@')[1].ToInt(), a.Split('@')[2].ToInt(), a.Split('@')[0].ToString());

            return defects.ToArray();
        }

        private string ReadStripMapFromSharedDrive(string stripMapPath, string stripId)
        {
            try
            {
                string[] filesName = Directory.GetFiles(stripMapPath, stripId + "_*");
                if (filesName.Count() != 0)
                {
                    string latestFile = filesName.Last();
                    Logger.LogHelper.LogDebug("Latest Strip Map is {0}".FillArguments(latestFile));
                    var fileread = File.ReadAllLines(latestFile);
                    string singleLineStripMap = string.Empty;
                    foreach (string lines in fileread)
                    {
                        singleLineStripMap += lines;
                    }
                    return singleLineStripMap;
                }
            }
            catch(Exception ex)
            {
                Logger.LogHelper.LogError("Get StripMap Failed.");
                Logger.LogHelper.LogException(ex);
            }
            return string.Empty;
        }
        #endregion

    }
}
