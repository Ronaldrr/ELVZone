using System.Runtime.Serialization;

namespace ELVZone.Models
{
    [DataContract]
    public class ParameterSetting
    {
        [DataMember(Order = 1)]
        public bool IsEnabled { get; set; }

        [DataMember(Order = 2)]
        public string ParameterName { get; set; }

        public ParameterSetting()
        {
            IsEnabled = true;
            ParameterName = string.Empty;
        }
    }
}
