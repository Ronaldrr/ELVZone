using System.Runtime.Serialization;

namespace ELVZone.Models
{
    [DataContract]
    public class ZoneStyleSetting
    {
        [DataMember(Order = 1)]
        public bool FillEnabled { get; set; }

        [DataMember(Order = 2)]
        public bool LineEnabled { get; set; }

        [DataMember(Order = 3)]
        public string FilledRegionTypeName { get; set; }

        [DataMember(Order = 4)]
        public string LineStyleName { get; set; }

        public ZoneStyleSetting()
        {
            FillEnabled = true;
            LineEnabled = true;
            FilledRegionTypeName = string.Empty;
            LineStyleName = string.Empty;
        }
    }
}
