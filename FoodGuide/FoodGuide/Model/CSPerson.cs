using FoodGuide.Core;

namespace FoodGuide.Model
{
    public class CSPerson
    {
        public CSPerson()
        {
            // TODO: Complete member initialization
        }

        public string LoginName { get; set; }

        public string DisplayName { get; set; }

        public int Id { get; set; }

      /*  public long ProfileRecordId
        {
            get
            {
                if (string.IsNullOrEmpty(LoginName))
                {
                    //Log.Warn("CSPerson.LoginName is null or empty! (DisplayName: " + DisplayName + ", id: " + Id + ")");
                    return -1;
                }
                return SPUtil.GetProfileRecordId(LoginName);
            }
        }

        public string MyProfileLink
        {
            get
            {
                if (string.IsNullOrEmpty(LoginName))
                {
                    //Log.Warn("CSPerson.LoginName is null or empty! (DisplayName: " + DisplayName + ", id: " + Id + ")");
                    return null;
                }
                return SPUtil.GetProfileUrl(LoginName);
            }
        }*/
    }
}
