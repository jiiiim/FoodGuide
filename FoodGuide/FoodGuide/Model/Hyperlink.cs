using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FoodGuide.Model
{
    [DataContract(Namespace = "")]
    public class Hyperlink
    {
        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }
    }
}
