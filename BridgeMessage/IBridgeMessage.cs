using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage
{
    public interface IBridgeMessage : IErrorInfo
    {
        string MessageID { get; set; }
        string Subject { get; set; }
        string Destination { get; set; }
        string Sender { get; set; }
        string PassBySender { get; set; }
        bool IsReply { get; set; }
        IEnumerable<BasicData> Data { get; }
        IEnumerable<BasicData> ReplyData { get; }

        /// <summary>
        /// Add variable to basic data enumerable
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The variable value.</param>
        /// <param name="dataType">The variable data type.</param>
        void AddBasicData(string name, object value, Type dataType);

        /// <summary>
        /// Add variable to reply data enumerable
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The variable value.</param>
        /// <param name="dataType">The variable data type.</param>
        void AddReplyBasicData(string name, object value, Type dataType);

        /// <summary>
        /// Retrieve data using name
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <returns>The BasicData object</returns>
        BasicData GetBasicData(string name);

        /// <summary>
        /// Compile the variable into enumerable of data.
        /// </summary>
        void CompileData();

        /// <summary>
        /// Compile the variable into enumerable of reply data.
        /// </summary>
        void CompileReplyData();
    }
}
