using System.Runtime.Serialization;

namespace SimpleModelWrapper.Core
{
    public class Error
    {
        public Error(string errorID, string message)
        {
            ErrorID = errorID;
            Message = message;
        }

        [DataMember]
        public string ErrorID { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}