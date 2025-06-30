using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver.Services
{
    using EAPMessage.Receive;
    using EventManager;
    using EventManager.EventArguments;
    using Objects;
    using Utilities.ExtensionPlug;

    public class InsertDataService : SOAPServiceBase, IDisposable
    {
        #region Private Field

        SOAPRequest mParent;
        Thread mInsertThread;

        #endregion

        #region Public Field

        public event EventDelegates.ServiceEventHandler OnMapInserted;
        public event EventDelegates.ServiceEventHandler OnMapInsertFail;

        #endregion

        #region Constructor

        public InsertDataService(Helper helper, Logger logger, Dictionary<string, string> webServicePathDict, SOAPRequest parent): base(helper, logger, webServicePathDict)
        {
            this.mParent = parent;
        }

        #endregion

        #region Public Method

        public void PerformInsertStripMapAndReply(StripInfo stripInfo, bool notifyUser)
        {
            //Task.Run(new Action(() => InsertStripMap(stripInfo, notifyUser)));
            mInsertThread = new Thread(() =>
                {
                    InsertStripMap(stripInfo, notifyUser);
                });

            mInsertThread.IsBackground = true;
            mInsertThread.Priority = ThreadPriority.Normal;
            mInsertThread.Start();
        }

        public void Dispose()
        {
            mInsertThread?.Abort();
            mInsertThread = null;
        }

        #endregion

        #region Private Method

        public bool InsertStripMap(StripInfo stripInfo, bool notifyUser)
        {
            return InsertStripMap(stripInfo, notifyUser, false);
        }

        public bool InsertStripMap(StripInfo stripInfo, bool notifyUser, bool newStrip)
        {
            try
            {
                string soapInput;

                soapInput = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}",
                    stripInfo.Package,
                    stripInfo.Location,
                    DateTime.Now.ToString("yyyyMMddHHmmss"),
                    stripInfo.LotID,
                    stripInfo.StripID,
                    stripInfo.Operator,
                    newStrip ? string.Empty :
                    GenerateDefectCode(
                        stripInfo.LotID,
                        stripInfo.StripID,
                        stripInfo.DefectCode,
                        stripInfo.Row,
                        stripInfo.Column,
                        stripInfo.OriginLocation));
                
                var resultContent = RequestService("InsertData", soapInput);

                if (!string.IsNullOrEmpty(resultContent))
                {
                    if (resultContent.Contains("FAIL"))
                    {
                        if (resultContent.Contains("Invalid Map File"))
                        {
                            OnMapInsertFail?.Invoke(this, new ServiceArgs(stripInfo.FWEquipmentID, stripInfo.EquipmentID, stripInfo.StripID, notifyUser));
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        OnMapInserted?.Invoke(this, new ServiceArgs(stripInfo.FWEquipmentID, stripInfo.EquipmentID, stripInfo.StripID, false));
                        return true;
                    }
                }
                else
                {
                    OnMapInsertFail?.Invoke(this, new ServiceArgs(stripInfo.FWEquipmentID, stripInfo.EquipmentID, stripInfo.StripID, notifyUser));
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return false;
            }
        }

        public bool InsertWaferMap(List<WaferInfo> waferInfo)
        {
            try
            {
                string soapInput = "";
                string waferId = "";
                string joinCoord = "";
                string resultContent = "";
                Logger.LogHelper.LogInfo("Start Loop");
                foreach(var wm in waferInfo)
                {
                    if(waferId != "")
                    {
                        if (wm.WaferID != waferId)
                        {
                            Logger.LogHelper.LogInfo("1: " + waferId + " " + wm.WaferID);
                            soapInput = string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            wm.EquipmentID,
                                            wm.LotID,
                                            wm.StripID,
                                            waferId,
                                            wm.NotchLoc,
                                            joinCoord.Remove(joinCoord.Length - 1, 1)
                                            );

                            if (soapInput != "")
                            {
                                if (soapInput.Last().Equals(';'))
                                {
                                    soapInput = soapInput.Remove(soapInput.Length - 1);
                                }

                                resultContent = RequestService("InsertWafer", soapInput);
                                Logger.LogHelper.LogInfo("result: " + resultContent);
                            }

                            joinCoord = "";
                        }
                    }

                    waferId = wm.WaferID;
                    //Logger.LogHelper.LogInfo("joinCoord: " + joinCoord);
                    //Logger.LogHelper.LogInfo("wm.Wafer_X: " + wm.Wafer_X);
                    //Logger.LogHelper.LogInfo("wm.Wafer_Y: " + wm.Wafer_Y);
                    //Logger.LogHelper.LogInfo("wm.Strip_X: " + wm.Strip_X);
                    //Logger.LogHelper.LogInfo("wm.Strip_Y: " + wm.Strip_Y);

                    joinCoord += wm.Wafer_X + "|" + wm.Wafer_Y + "|" + wm.Strip_X + "|" + wm.Strip_Y + ";";
                }
               
                Logger.LogHelper.LogInfo("End Loop");

                soapInput = string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                    waferInfo[0].EquipmentID,
                    waferInfo[0].LotID,
                    waferInfo[0].StripID,
                    waferId,
                    waferInfo[0].NotchLoc,
                    joinCoord.Remove(joinCoord.Length - 1, 1)
                );

                if (soapInput.Last().Equals(';'))
                {
                    soapInput = soapInput.Remove(soapInput.Length - 1);
                }

                resultContent = RequestService("InsertWafer", soapInput);

                if (!string.IsNullOrEmpty(resultContent))
                {
                    if (resultContent.Contains("FAIL"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogHelper.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defectCodeArray"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="mr"></param>
        /// <param name="mc"></param>
        /// <param name="sr"></param>
        /// <param name="sc"></param>
        /// <param name="originLocation">
        ///     <para>1 - Upper Right</para>
        ///     <para>2 - Upper Left</para>
        ///     <para>3 - Lower Left</para>
        ///     <para>4 - Lower Right</para>
        /// </param>
        /// <returns></returns>
            private string GenerateDefectCode(string lotId, string stripId, string[] defectCodeArray, int row, int column, int originLocation)
        {
            try
            {
                string result = string.Empty;
                int totalDie = defectCodeArray.Length;
                int goodDie = 0;
                int badDie = 0;

                if (defectCodeArray == null)
                    return result;

                var defectCodeMatrix = GenerateMachineMatrix(defectCodeArray, row, column, originLocation);

                for (int i = 0; i < row; i++)
                {
                    //var sortI = sr == "Y" ? row - i - 1 : i;

                    for (int j = 0; j < column; j++)
                    {
                        //var sortJ = sc == "Y" ? column - j - 1 : j;
                        if (defectCodeMatrix[i][j] == "0")
                        {
                            goodDie++;
                            continue;
                        }

                        result += GenerateEMAPDefectCode(defectCodeMatrix, i, j);
                        badDie++;
                    }
                }

                Logger.LogHelper.LogInfo("LotID={0}, StripID={1}, TotalDie={2}, GoodDie={3}, BadDie={4}".FillArguments(
                    lotId, stripId, totalDie, goodDie, badDie));

                if (result.Last().Equals(';'))
                {
                    result = result.Remove(result.Length - 1);
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string[][] GenerateMachineMatrix(string[] defectCodeArray, int row, int column, int originLocation)
        {
            try
            {
                var matrix = new string[row][];

                for (int i = 0; i < row; i++)
                {
                    matrix[i] = new string[column];
                    for (int j = 0; j < column; j++)
                    {
                        switch (originLocation)
                        {
                            case 1:
                                matrix[i][j] = defectCodeArray[(i + 1) * column - j - 1];
                                break;

                            case 2:
                                matrix[i][j] = defectCodeArray[i * column + j];
                                break;

                            case 3:
                                matrix[i][j] = defectCodeArray[(row - i - 1) * column + j];
                                break;

                            case 4:
                                matrix[i][j] = defectCodeArray[row * column - (i + 1) * j - 1];
                                break;
                        }
                    }
                }                             

                return matrix;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual string GenerateEMAPDefectCode(int[][] defectCodeMatrix, int row, int column)
        {
            return "{0}@{1}@{2};".FillArguments(Helper.HandleDefectCode(defectCodeMatrix[row][column]), (row + 1).ToString(), (column + 1).ToString());
        }
        public virtual string GenerateEMAPDefectCode(string[][] defectCodeMatrix, int row, int column)
        {
            return "{0}@{1}@{2};".FillArguments(Helper.HandleDefectCode(defectCodeMatrix[row][column]), (row + 1).ToString(), (column + 1).ToString());
        }
        #endregion

    }
}
