using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage.Common
{
    public class EquipmentEventPacket : BridgeMessagePacket
    {
        #region Private Field

        private string mFWEquipmentID;
        private string mEquipmentID;
        private SimpleItem mEventItems;
        private EventReport mEventReport;

        #endregion

        #region Properties

        public string FWEquipmentID
        {
            get { return mFWEquipmentID; }
        }

        public string EquipmentID
        {
            get { return mEquipmentID; }
        }

        public EventReport EventReport
        {
            get
            {
                return mEventReport;
            }
        }

        #endregion

        #region Constructor

        public EquipmentEventPacket()
        {

        }

        #endregion

        #region Public Method



        #endregion

        #region Protected Method

        protected override void AssignData()
        {
            mFWEquipmentID = GetBasicData("FWEQUIPMENTID").Value.ToString();
            mEquipmentID = GetBasicData("EQUIPMENTID").Value.ToString();
            mEventItems = GetBasicData("EVENTITEMS").Value as SimpleItem;

            mEventReport = ConvertToEventReport(mEventItems);
        }

        #endregion

        #region Private Method

        private EventReport ConvertToEventReport(SimpleItem eventItems)
        {
            var ceid = Convert.ToInt32(eventItems.Value);
            var reportItems = eventItems.Childs;
            var eventReport = new EventReport();
            eventReport.EventID = ceid;

            if (reportItems != null)
            {
                var reportList = new List<EventReport.ReportItem>();

                foreach (var reportItem in reportItems)
                {
                    var report = new EventReport.ReportItem();

                    report.ReportID = Convert.ToInt32(reportItem.Value);

                    var svidList = new List<object>();
                    var svItems = reportItem.Childs;

                    foreach (var svItem in svItems)
                    {
                        svidList.Add(svItem.Value);
                    }

                    report.SV = svidList.ToArray();
                    reportList.Add(report);
                }
                eventReport.Reports = reportList.ToArray();
            }

            return eventReport;
        }

        #endregion
    }
}
