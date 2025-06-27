using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP
{
   
    public sealed class EAPError
    {
        public const int OK = 0;

        /// <summary>
        /// <para>To handle error that are unable to log. </para>
        /// <para>Mostly used in log that haven't initialize.</para>
        /// </summary>
        public const int UNHANDLED_ERR = -1;

        /// <summary>
        /// 
        /// </summary>
        public const int NULL_ARG_ERR = -2;

        /// <summary>
        /// 
        /// </summary>
        public const int DRIVER_NOT_FOUND = -3;

        /// <summary>
        /// Data cannot be found.
        /// </summary>
        public const int DATA_NOT_FOUND = -4;

        /// <summary>
        /// Data not matching.
        /// </summary>
        public const int DATA_NOT_MATCH = -5;

        /// <summary>
        /// One or more directories are not exist.
        /// </summary>
        public const int DIR_NOT_FOUND = -6;

        /// <summary>
        /// One or more files are not exist.
        /// </summary>
        public const int FILE_NOT_FOUND = -7;

        /// <summary>
        /// Unable to get data.
        /// </summary>
        public const int FAIL_TO_GET_DATA = -8;


        /// <summary>
        /// The method is not implemented.
        /// </summary>
        public const int METHOD_NOT_IMPLEMENTED = -10;


        /// <summary>
        /// Error during SECS transaction
        /// </summary>
        public const int SECS_TRANS_ERR = -100;

        /// <summary>
        /// Received SECS abort transaction
        /// </summary>
        public const int SECS_TRANS_ABORT = -101;

        /// <summary>
        /// Expected acknowledge code does not match
        /// </summary>
        public const int SECS_ACK_ERR = -102;

        /// <summary>
        /// Unsupported SECS transaction error
        /// </summary>
        public const int UNSUPPORTED_SECSTRANS_ERR = -103;

        /// <summary>
        /// SECS Value compared, but not matched
        /// </summary>
        public const int SECS_VAL_NOT_MATCH = -104;

        /// <summary>
        /// Wait event report to happen, but timeout
        /// </summary>
        public const int SECS_EVENT_REPORT_TIMEOUT = -105;

        /// <summary>
        /// S9F9 happened, causing disconnect
        /// </summary>
        public const int SECS_S9F9_ERR = -106;

        /// <summary>
        /// Before initialize, update disconnect state
        /// </summary>
        public const int SECS_PENDING_INIT = -107;

        /// <summary>
        /// SECS Value compared, but invalid recipe
        /// </summary>
        public const int SECS_INVALID_RECIPE = -108;

        /// <summary>
        /// SECS Value compared, but not online remote
        /// </summary>
        public const int SECS_NOT_ONLINE_REMOTE = -109;

        /// <summary>
        /// SECS Value compared, but not idle mode
        /// </summary>
        public const int SECS_NOT_IDLE_MODE = -110;

        /// <summary>
        /// Job not found/exist
        /// </summary>
        public const int JOB_NOT_FOUND = -200;


        /// <summary>
        /// Recipe parameter validation failed
        /// </summary>
        public const int PARAM_VALIDATE_FAILED = -300;

        /// <summary>
        /// Recipe body is empty from SECS Message
        /// </summary>
        public const int RECIPE_BODY_NOT_EXIST = -301;

        /// <summary>
        /// Recipe body validation failed
        /// </summary>
        public const int RECIPE_BODY_VALIDATE_FAILED = -302;

        /// <summary>
        /// Invalid recipe file was found
        /// </summary>
        public const int INVALID_RECIPE_FOUND = -303;

        /// <summary>
        /// Timeout for recipe download
        /// </summary>
        public const int RECIPE_DOWNLOAD_TIMEOUT = -304;

        /// <summary>
        /// Strip not found/exist
        /// </summary>
        public const int STRIP_NOT_FOUND = -401;

        public const int STRIP_LOT_NOT_MATCH = -402;

        /// <summary>
        /// Mostly used to catched exception
        /// </summary>
        public const int UNKNOWN_ERR = -9999;
       
    }

    public sealed class SimpleItem
    {
        public object Value { get; set; }

        public SimpleItem[] Childs { get; set; }

        public string[] ValueArray { get; set; }
        public SimpleItem()
        {
        }

        public SimpleItem(object value)
        {
            Value = value;
        }

        public SimpleItem(string[] valueArray)
        {
            ValueArray = valueArray;
        }
    }

    public enum EAPEquipmentCommState
    {
        Communicating,
        NotCommunicating
    }
}
