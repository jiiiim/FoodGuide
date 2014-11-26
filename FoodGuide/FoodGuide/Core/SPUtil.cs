using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Taxonomy;
using FoodGuide.Model;

namespace FoodGuide.Core
{
    public class SPUtil
    {

        //public static Caching.Cache<string,string> ProfileCache = new  Caching.Cache<string,string>();

        /// <summary>
        /// Get the service Context, so that we don't need the web server (e.g. http://t410-mk) anymore - created on 2011-08-24, M.K.
        /// </summary>
        /// <returns></returns>
        /*public static SPServiceContext GetServiceContext()
        {
            string siteUrl = null;
            SPServiceContext ctx = null;
            try
            {
                siteUrl = ConfigurationCache.GetConfigurationValue(Constants.GlobalSettingFrontendSiteurl);
                using (SPSite site = new SPSite(siteUrl))
                {
                    ctx = SPServiceContext.GetContext(site);
                }

            }
            catch (Exception)
            {
            }
            if (ctx == null)
            {
                ctx = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);
            }
            return ctx;
        }*/

        public static UserProfile GetUserProfile(string loginName)
        {
            UserProfile result = null;
            try
            {
                Log.Verbose("Fetching user profile for user " + loginName);
               // UserProfileManager upm = new UserProfileManager(SPServiceContext.Current != null ? SPServiceContext.Current : GetServiceContext());

               // result = upm.GetUserProfile(loginName);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get user profile", ex);
            }
            return result;
        }

        public static UserProfile GetCurrentUserProfile()
        {
            UserProfile result = null;
            try
            {
                SPUser currentUser = SPContext.Current.Web.CurrentUser;
                result = GetUserProfile(currentUser.LoginName);
            }
            catch (Exception ex)
            {
                Log.Error("Could not get current user profile", ex);
            }
            return result;
        }

        public static string GetStringValue(SPListItem item, string internalName)
        {
            //string retVal;

            string value = (item[item.Fields.GetFieldByInternalName(internalName).Id] + "").Trim();
           /* using (Stream stream = value.ToStream())
            {
                // needed to remove special characters which are not allowed in XML
                using (XmlSanitizingStream reader = new XmlSanitizingStream(stream))
                {
                    retVal = reader.ReadToEnd();
                }
            }*/
            return value;
        }

        public static DateTime? GetNullableDateValue(SPListItem item, string internalName)
        {
            DateTime? result = new DateTime();
            DateTime innerResult = new DateTime();
            if (!DateTime.TryParse(item[item.Fields.GetFieldByInternalName(internalName).Id] + "", out innerResult))
            {
                result = null;
            }
            else
            {
                result = innerResult;
            }
            return result;
        }
        

        public static DateTime GetDateValue(SPListItem item, string internalName)
        {
            DateTime result = new DateTime();
            if (!DateTime.TryParse(item[item.Fields.GetFieldByInternalName(internalName).Id] + "", out result))
            {
            
            }
            return result; 
        }

        public static int GetLookupId(SPListItem item, string internalName)
        {
            int retVal;
            try
            {
                retVal = new SPFieldLookupValue(item[internalName] + "").LookupId;
            }
            catch (Exception)
            {
                retVal = -1;
            }
            return retVal;
        }

        public static List<int> GetLookupIds(SPListItem item, string internalName)
        {
            List<int> retval = new List<int>();
            try
            {
                SPFieldLookupValueCollection lookupValueCollection = new SPFieldLookupValueCollection(item[internalName] + "");
                retval = lookupValueCollection.Select(value => value.LookupId).ToList();
            }
            catch (Exception)
            {
            }
            return retval;
        }

        public static int GetIntValue(SPListItem item, string internalName)
        {
            return GetIntValue(item, internalName, 0);
        }

        public static int GetIntValue(SPListItem item, string internalName, int fallbackValueIfEmpty)
        {
            int result;
            if (!Int32.TryParse(item[internalName] + "", out result))
            {
                //Log.Error("Cannot parse " + item[internalName] + " in colum " + internalName + " to integer. Defaulting to 0.");
                result = fallbackValueIfEmpty;
            }
            return result;
        }

        public static double GetDoubleValue(SPListItem item, string internalName)
        {
            double result;
            if (!Double.TryParse(item[internalName] + "", out result))
            {
                //Log.Error("Cannot parse " + item[internalName] + " in colum " + internalName + " to double. Defaulting to 0.");
                result = 0;
            }
            return result;
        }

        public static bool GetBoolValue(SPListItem item, string internalName)
        {
            return item[internalName] != null && (bool)item[internalName];
        }

        public static string GetValue(TaxonomyFieldValue value)
        {
            return value != null ? value.Label : "";
        }

        public static string GetId(TaxonomyFieldValue value)
        {
            return value != null ? value.TermGuid : "";
        }

        public static Taxonomy GetTaxonomyValue(TaxonomyFieldValue value)
        {
            Taxonomy result = null;
            string stringValue = GetValue(value);
            string id = GetId(value);
            if (!string.IsNullOrEmpty(stringValue) && !string.IsNullOrEmpty(id))
            {
                result = new Taxonomy { Id = id, Label = stringValue };
            }
            return result;
        }

        public static List<string> GetMetaDataStrings(SPListItem item, string internalName)
        {
            List<Taxonomy> metadatas = GetMetaDatas(item, internalName);
            List<string> result = metadatas.Select(metadata => metadata.Label).ToList();
            return result;
        }

        public static List<Taxonomy> GetMetaDatas(SPListItem item, string internalName)
        {
            List<Taxonomy> result = new List<Taxonomy>();
            try
            {
                object itemValue = item[internalName];
                if (itemValue is TaxonomyFieldValueCollection)
                {
                    foreach (TaxonomyFieldValue value in (TaxonomyFieldValueCollection)itemValue)
                    {
                        Taxonomy tax = GetTaxonomyValue(value);
                        if (tax != null)
                        {
                            result.Add(tax);
                        }
                    }
                }
                else if (itemValue is TaxonomyFieldValue)
                {
                    Taxonomy tax = GetTaxonomyValue((TaxonomyFieldValue)itemValue);
                    if (tax != null)
                    {
                        result.Add(tax);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Cannot read column field '" + internalName + "' from list '" + item.ParentList.Title + "' ('" + item.ParentList.ID + "'): " + ex.Message);
            }
            return result;
        }

        public static string GetMetaDataString(SPListItem item, string internalName)
        {
            string result = "";
            object itemValue = item[internalName];
            if (itemValue is TaxonomyFieldValueCollection)
            {
                foreach (TaxonomyFieldValue value in (TaxonomyFieldValueCollection)itemValue)
                {
                    string stringValue = GetValue(value);
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        result = stringValue;
                    }
                }
            }
            else if (itemValue is TaxonomyFieldValue)
            {
                result = GetValue((TaxonomyFieldValue)itemValue);
            }
            return result;
        }

        public static Hyperlink GetLink(SPListItem item, string internalName)
        {
            Hyperlink result = null;
            string stringValue = item[internalName] + "";
            if (!string.IsNullOrEmpty(stringValue))
            {
                SPFieldUrlValue url = new SPFieldUrlValue(stringValue);
                result = new Hyperlink { Title = url.Description, Url = url.Url };
            }
            return result;
        }

        public static List<string> GetLookupValues(SPListItem item, string internalName)
        {
            List<string> result = new List<string>();
            string stringValue = item[internalName] + "";
            //Log.Debug("Value: '" + stringValue + "'");
            if (!string.IsNullOrEmpty(stringValue))
            {
                SPFieldLookupValueCollection lookupValueCollection = new SPFieldLookupValueCollection(stringValue);
                result = lookupValueCollection.Select(value => value.LookupValue).ToList();
            }
            return result;
        }

        public static string GetLookupValue(SPListItem item, string internalName)
        {
            return new SPFieldLookupValue(item[internalName] + "").LookupValue;
        }

        public static CSPerson GetUser(SPListItem item, Guid fieldId)
        {
            string fieldValue = null;
            SPFieldUser fieldUser = null;
            try
            {
                fieldValue = item[fieldId] + "";
                fieldUser = item.Fields[fieldId] as SPFieldUser;
            }
            catch (Exception ex)
            {
                Log.Warn("Cannot read column field '" + fieldId + "' from list '" + item.ParentList.Title + "' ('" + item.ParentList.ID + "'): " + ex.Message);
            }

            return GetSPUser(item, fieldUser, fieldValue);
        }

        public static CSPerson GetUser(SPListItem item, string internalName)
        {
            string fieldValue = null;
            SPFieldUser fieldUser = null;
            try
            {
                fieldValue = item[internalName] + "";
                fieldUser = item.Fields.GetFieldByInternalName(internalName) as SPFieldUser;
            }
            catch (Exception ex)
            {
                Log.Warn("Cannot read column field '" + internalName + "' from list '" + item.ParentList.Title + "' ('" + item.ParentList.ID + "'): " + ex.Message);
            }
            return GetSPUser(item, fieldUser, fieldValue);
        }

        public static CSPerson GetSPUser(SPListItem item, SPFieldUser userField, string fieldValue)
        {
            CSPerson result = new CSPerson();
            result.DisplayName = "n/a";

            if (!string.IsNullOrEmpty(fieldValue))
            {
                try
                {
                    SPFieldUserValue userFieldValue = userField.GetFieldValue(fieldValue) as SPFieldUserValue;
                    SPUser user = userFieldValue.User;
                    result.DisplayName = CleanUserName(user);
                    result.Id = user.ID;
                    result.LoginName = user.LoginName;

                    //ProfileUrlAndRecordId puari = GetProfileUrlAndRecordId(user.LoginName);

                    //result.MyProfileLink = puari.MyProfileLink;
                    //result.ProfileRecordId = puari.ProfileRecordId;
                }
                catch (Exception ex)
                {
                    Log.ErrorVerbose("Cannot load user " + fieldValue, ex);
                    result.DisplayName = "n/a";
                    result.Id = -1;
                    result.LoginName = "n/a";
                }
            }
            return result;
        }

        public static List<CSPerson> GetUsers(SPListItem item, string internalName)
        {
            string fieldValue = null;
            SPFieldUser fieldUser = null;
            try
            {
                fieldValue = item[internalName] + "";
                fieldUser = item.Fields.GetFieldByInternalName(internalName) as SPFieldUser;
            }
            catch (Exception ex)
            {
                Log.Warn("Cannot read column field '" + internalName + "' from list '" + item.ParentList.Title + "' ('" + item.ParentList.ID + "'): " + ex.Message);
            }
            return GetUsers(item, fieldUser, fieldValue);
        }

        public static List<CSPerson> GetUsers(SPListItem item, SPFieldUser userField, string fieldValue)
        {
            List<CSPerson> result = new List<CSPerson>();

            if (!string.IsNullOrEmpty(fieldValue))
            {
                try
                {
                    SPFieldUserValueCollection userFieldValueCollection = userField.GetFieldValue(fieldValue) as SPFieldUserValueCollection;

                    foreach (SPFieldUserValue userFieldValue in userFieldValueCollection)
                    {
                        if (userFieldValue.User != null)
                        {
                            SPUser user = userFieldValue.User;
                            CSPerson csPerson = new CSPerson();
                            csPerson.DisplayName = CleanUserName(user);
                            csPerson.Id = user.ID;
                            csPerson.LoginName = user.LoginName;
                            result.Add(csPerson);
                        }
                    }


                    //result.MyProfileLink = puari.MyProfileLink;
                    //result.ProfileRecordId = puari.ProfileRecordId;
                }
                catch (Exception ex)
                {
                    //Log.Error("Cannot load user " + fieldValue, ex);
                }
            }
            return result;
        }


        public static List<int> GetGroupIds(SPListItem item, string internalName)
        {
            string fieldValue = null;
            SPFieldUser fieldUser = null;
            try
            {
                fieldValue = item[internalName] + "";
                fieldUser = item.Fields.GetFieldByInternalName(internalName) as SPFieldUser;
            }
            catch (Exception ex)
            {
                Log.Warn("Cannot read column field '" + internalName + "' from list '" + item.ParentList.Title + "' ('" + item.ParentList.ID + "'): " + ex.Message);
            }
            return GetGroupIds(item, fieldUser, fieldValue);
        }

        public static List<int> GetGroupIds(SPListItem item, SPFieldUser userField, string fieldValue)
        {
            List<int> result = new List<int>();

            if (!string.IsNullOrEmpty(fieldValue))
            {
                try
                {
                    SPFieldUserValueCollection userFieldValueCollection = userField.GetFieldValue(fieldValue) as SPFieldUserValueCollection;

                    foreach (SPFieldUserValue userFieldValue in userFieldValueCollection)
                    {
                        if (userFieldValue.User == null)
                        {
                            result.Add(userFieldValue.LookupId); // LookupID keeps SPGroup.ID
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log.Error("Cannot load user " + fieldValue, ex);
                }
            }
            return result;
        }
        
        public static string CleanUserName(SPUser user)
        {
            string cleanName = String.IsNullOrEmpty(user.Name) ? user.LoginName : user.Name;
            if (cleanName.Contains(" ("))
                cleanName = cleanName.Substring(0, cleanName.IndexOf('(') - 1);

            return cleanName;
        }

        

        //public static T GetFromCache<T>(string key) where T : class
        //{
        //    //return HttpContext.Current != null ? HttpContext.Current.Cache[key] as T : null;
        //}

        //public static void SetCache(object obj, string key, int minutes)
        //{
        //    if (HttpContext.Current != null && obj != null)
        //    {
        //        HttpContext.Current.Cache.Add(key, obj, null, DateTime.Now + TimeSpan.FromMinutes(minutes),
        //                                      Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        //    }
        //}
       /* public static string GetProfileUrl(string loginName)
        {
            return GetProfileUrl(loginName, SPServiceContext.Current);
        }

         public static string GetProfileUrl(string loginName, SPServiceContext serviceContext)
         {
             try
             {
                 return GetProfileUrlAndRecordId(loginName).MyProfileLink;
             }
             catch (Exception)
             {
                 return null;
             }
             // Code below is combinede in GetProfileUrlAndRecordId(string loginName) including caching

             // The farm account does not have a mysite and trying to retrieve it wastes a lot of time
             if (loginName.Equals(@"SHAREPOINT\system", StringComparison.InvariantCultureIgnoreCase))
                 return null;

             string key = "contentstore.profileurl." + loginName;
             // check if profile url for this user has already been cached
             string result = ProfileCache[key];

             // The mysite URL could not be retreived the last time
             if (result == "nourl")
                 return null;

             if (result == null)
             {
                 try
                 {
                     // if not, get user profile manager, retrieve url and store in cache if "valid"
                     UserProfileManager upm;
                     if (serviceContext == null)
                     {
                         using (SPSite tmpSite = new SPSite(SPUtil.GetFrontendSiteurl()))
                         {
                             upm = new UserProfileManager(SPServiceContext.GetContext(tmpSite));
                         }
                     }
                     else
                         upm = new UserProfileManager(serviceContext);

                     string url = upm.GetUserProfile(loginName).PublicUrl + "";
                     Log.Debug("SPUtil: " + loginName + " <-> " + url);
                     if (!string.IsNullOrEmpty(url))
                     {
                         ProfileCache.Set(key, url, TimeSpan.FromHours(24));
                         result = url;
                     }
                 }
                 catch (Exception)
                 {
                     Log.Warn("SPUtil: " + loginName + " <-> [NO_URL]");
                     ProfileCache.Set(key, "nourl", TimeSpan.FromHours(24));
                     Log.Warn("SPUtil: Could not retrieve MyProfile URL for '" + loginName + "'");
                     //Log.Error("SPUtil: Could not retrieve MyProfile URL:", ex);
                 }
             }
             return result;
         }

         public static long GetProfileRecordId(string loginName)
         {
             try
             {
                 return GetProfileUrlAndRecordId(loginName).ProfileRecordId;
             }
             catch (Exception)
             {
                 return -1;
             }

             return GetProfileUrlAndRecordId(loginName).ProfileRecordId;
             // Code below is combinede in GetProfileUrlAndRecordId(string loginName) including caching

             // The farm account does not have a mysite and trying to retrieve it wastes a lot of time
             if (loginName.Equals(@"SHAREPOINT\system", StringComparison.InvariantCultureIgnoreCase))
                 return -1;

             string key = "contentstore.profilerecordid." + loginName;
             // check if profile url for this user has already been cached
             string result = ProfileCache[key];

             // The profilerecordid could not be retreived the last time
             if (result == "noid")
                 return -1;

             if (result == null)
             {
                 try
                 {
                     // if not, get user profile manager, retrieve profilerecordid and store in cache if "valid"
                     UserProfileManager upm;
                     if( SPServiceContext.Current == null )
                     {
                         using (SPSite tmpSite = new SPSite(SPUtil.GetFrontendSiteurl()))
                          {
                               upm = new UserProfileManager(SPServiceContext.GetContext(tmpSite));
                          }
                     }
                     else
                         upm = new UserProfileManager(SPServiceContext.Current);
                    
                     long profileId = upm.GetUserProfile(loginName).RecordId;
                     ProfileCache.Set(key, profileId.ToString(), TimeSpan.FromHours(24));
                     result = profileId.ToString();
                     Log.Debug("SPUtil: " + loginName + " <-> RecordId: " + profileId);

                 }
                 catch (Exception)
                 {
                     Log.Warn("SPUtil: " + loginName + " <-> [NO_profilerecordid]");
                     ProfileCache.Set(key,"noid",TimeSpan.FromHours(24));
                     result = "-1";
                     Log.Warn("SPUtil: Could not retrieve profilerecordid for '" + loginName + "'");
                 }
             }
             return long.Parse(result);
         }

         public class ProfileUrlAndRecordId
         {
             public string MyProfileLink = null;
             public long ProfileRecordId = -1;
         }

         public static ProfileUrlAndRecordId GetProfileUrlAndRecordId(string loginName)
         {
             ProfileUrlAndRecordId retInfo = new ProfileUrlAndRecordId();

             // The farm account does not have a mysite and trying to retrieve it wastes a lot of time
             if (! loginName.Equals(@"SHAREPOINT\system", StringComparison.InvariantCultureIgnoreCase))
             {

                 string keyUrl = "contentstore.profileurl." + loginName;
                 // check if profile url for this user has already been cached
                 string resultUrl = ProfileCache[keyUrl];


                 string keyId = "contentstore.profilerecordid." + loginName;
                 // check if profile url for this user has already been cached
                 string resultId = ProfileCache[keyId];



                 if (resultUrl == null || resultId == null)
                 {
                     try
                     {
                         // if not, get user profile manager, retrieve url and store in cache if "valid"
                         SPServiceContext serviceContext = SPServiceContext.Current;
                         UserProfileManager upm;
                         if (serviceContext == null)
                         {
                             using (SPSite tmpSite = new SPSite(SPUtil.GetFrontendSiteurl()))
                             {
                                 upm = new UserProfileManager(SPServiceContext.GetContext(tmpSite));
                             }
                         }
                         else
                         {
                             upm = new UserProfileManager(serviceContext);
                         }

                         UserProfile up = upm.GetUserProfile(loginName);

                         if (resultUrl == null)
                         {
                             try
                             {
                                 string url = up.PublicUrl + "";
                                 resultUrl = string.IsNullOrEmpty(url) ? "nourl" : url;
                                 Log.Debug("SPUtil: " + loginName + " <-> " + resultUrl);
                             }
                             catch (Exception ex)
                             {
                                 Log.Error(
                                     "SPUtil.GetProfileUrlAndRecordId: Error retrieving PublicUrl for '" + loginName + "'",
                                     ex);
                                 resultUrl = "nourl";
                             }
                         }

                         if (resultId == null)
                         {
                             try
                             {
                                 long profileId = up.RecordId;
                                 resultId = string.IsNullOrEmpty(profileId + "") ? "noid" : profileId + "";
                                 Log.Debug("SPUtil: " + loginName + " <-> RecordId: " + resultId);
                             }
                             catch (Exception ex)
                             {
                                 Log.Error(
                                     "SPUtil.GetProfileUrlAndRecordId: Error retrieving RecordId for '" + loginName + "'", ex);
                                 resultId = "noid";
                             }
                         }
                     }
                     catch (Exception)
                     {
                         Log.Warn("SPUtil.GetProfileUrlAndRecordId: " + loginName +
                                  " - Expected, likely there is no userprofile");

                         Log.Warn("SPUtil: " + loginName + " <-> [NO_URL]");
                         resultUrl = "nourl";

                         Log.Warn("SPUtil: " + loginName + " <-> [NO_ID]");
                         resultId = "noid";
                     }
                     ProfileCache.Set(keyUrl, resultUrl, TimeSpan.FromHours(24));
                     ProfileCache.Set(keyId, resultId, TimeSpan.FromHours(24));
                 }


                 // The mysite URL could not be retreived the last time
                 retInfo.MyProfileLink = resultUrl == "nourl" ? null : resultUrl;
                 // The profilerecordid could not be retreived the last time
                 retInfo.ProfileRecordId = resultId == "noid" ? -1 : long.Parse(resultId + "");
             }

             return retInfo;
         }

         public static string GetFrontendSiteurl()
         {
             string frontendSiteUrl = "";
             string urlMapping = (ConfigurationCache.GetConfigurationValue(Constants.GlobalSettingFrontendSiteurlMapping) + "").Trim().ToLower();
            
             if (string.IsNullOrEmpty(urlMapping))
             {
                 frontendSiteUrl = ConfigurationCache.GetConfigurationValue(Constants.GlobalSettingFrontendSiteurl);
             }
             else
             {
                 // parse urlMapping
                 Regex urlmappingRegex = new Regex(Environment.MachineName.ToLower() + @"[^:]*:""([^""]*)""");
                 Match match = urlmappingRegex.Match(urlMapping);
                 if (match.Success)
                 {
                     frontendSiteUrl = match.Groups[1].Value;
                 }
                 else
                 {
                     frontendSiteUrl = ConfigurationCache.GetConfigurationValue(Constants.GlobalSettingFrontendSiteurl);
                 }
             }
             return frontendSiteUrl;
         }*/


        public static List<string> GetChoiceValues(SPListItem item, string internalName)
        {
            List<string> choiceValues = new List<string>();
            try
            {
                SPFieldMultiChoiceValue choices = new SPFieldMultiChoiceValue(item[internalName] + "");
                for (int i = 0; i < choices.Count; i++)
                {
                    choiceValues.Add(choices[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not add choice value to list", ex);
            }
            return choiceValues;
        }


        public static string GetChoiceValue(SPListItem item, string internalName)
        {
            string choiceValue = "";
            try
            {
                SPFieldMultiChoiceValue choice = new SPFieldMultiChoiceValue(item[internalName] + "");
                if (choice.Count == 1)
                {
                    choiceValue = choice[0];
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not add choice value to list", ex);
            }
            return choiceValue;
        }

        public static SPFile WriteStreamToDocLib(Stream fileStream, SPList docLib, string fileName)
        {
            return WriteStreamToDocLib(fileStream, docLib, fileName, true);
        }

        public static SPFile WriteStreamToDocLib(Stream fileStream, SPList docLib, string fileName, bool overwrite)
        {
            SPFile file = docLib.RootFolder.Files.Add(fileName, fileStream, overwrite);

            file.Update();

            return file;
        }
    }
}
