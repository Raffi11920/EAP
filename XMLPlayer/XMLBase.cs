using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qynix.EAP.Base.XMLPlayer
{
    /// <summary>
    /// Used for serializable configuration class
    /// </summary>
    /// <typeparam name="T">The configuration class, T must be Public</typeparam>
    public abstract class XMLBase<T>
    {
        #region Private Field

        private T mSerializedObject;
        private string mXmlFilePath;
        private bool mIsDeserialized;
        private XmlSerializer mXmlSerializer;

        #endregion

        #region Properties

        public T SerializedObject
        {
            get { return mSerializedObject; }
        }

        public bool IsDeserialized
        {
            get { return mIsDeserialized; }
        }

        #endregion

        #region Constructor

        public XMLBase()
        {
            mXmlSerializer = new XmlSerializer(typeof(T));
        }

        public XMLBase(string xmlFilePath)
        {
            mXmlFilePath = xmlFilePath;
            mXmlSerializer = new XmlSerializer(typeof(T));
        }

        #endregion

        #region Public Method

        public bool Deserialize()
        {
            try
            {
                TextReader textReader = new StreamReader(mXmlFilePath);

                SubscribeDeserializeEvent();

                mSerializedObject = (T)mXmlSerializer.Deserialize(textReader);
                mIsDeserialized = true;

                UnsubscribeDeserializeEvent();

                return true;
            }
            catch (Exception ex)
            {
                mIsDeserialized = false;
                throw ex;
            }
        }

        public bool DeserializeFromString(string text)
        {
            try
            {
                return Deserialize((TextReader)new StringReader(text));
            }
            catch (Exception ex)
            {
                this.mIsDeserialized = false;
                throw ex;
            }
        }

        public bool Deserialize(TextReader textReader)
        {
            try
            {
                SubscribeDeserializeEvent();
                mSerializedObject = (T)this.mXmlSerializer.Deserialize(textReader);
                mIsDeserialized = true;
                UnsubscribeDeserializeEvent();
                textReader.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                this.mIsDeserialized = false;
                throw ex;
            }
        }

        public bool Serialize()
        {
            try
            {
                TextWriter textWriter = new StreamWriter(mXmlFilePath, false);

                SubsribeSerializeEvent();

                mXmlSerializer.Serialize(textWriter, mSerializedObject);

                UnsubscribeSerializeEvent();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Private Method

        private void SubscribeDeserializeEvent()
        {
            mXmlSerializer.UnknownAttribute += XmlSerializer_UnknownAttribute;
            mXmlSerializer.UnknownElement += XmlSerializer_UnknownElement;
            mXmlSerializer.UnknownNode += XmlSerializer_UnknownNode;
        }

        /// <summary>
        /// Always unsubscribe event after usage to prevent memory leaks.
        /// </summary>
        private void UnsubscribeDeserializeEvent()
        {
            mXmlSerializer.UnknownAttribute -= XmlSerializer_UnknownAttribute;
            mXmlSerializer.UnknownElement -= XmlSerializer_UnknownElement;
            mXmlSerializer.UnknownNode -= XmlSerializer_UnknownNode;
        }

        private void SubsribeSerializeEvent()
        {
            mXmlSerializer.UnreferencedObject -= XmlSerializer_UnreferencedObject;
        }

        private void UnsubscribeSerializeEvent()
        {
            mXmlSerializer.UnreferencedObject -= XmlSerializer_UnreferencedObject;
        }

        #endregion

        #region Event Handler

        private void XmlSerializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            throw new FileLoadException("Unknown XML Node at Line " + e.LineNumber);
        }

        private void XmlSerializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void XmlSerializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void XmlSerializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
