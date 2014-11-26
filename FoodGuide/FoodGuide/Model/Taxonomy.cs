using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace FoodGuide.Model
{
    [Serializable]
    [DataContract]
    public class Taxonomy
    {
        [XmlElement]
        [DataMember(Name="label")]
        public string Label { get; set; }

        [XmlElement]
        [DataMember(Name="id")]
        public string Id { get; set; }



        public bool Equals(Taxonomy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Label, Label) && Equals(other.Id, Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Taxonomy)) return false;
            return Equals((Taxonomy) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Label != null ? Label.GetHashCode() : 0)*397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return "ID: '" + Id + "', Label: '" + Label + "'";
        }
        
    }
}
