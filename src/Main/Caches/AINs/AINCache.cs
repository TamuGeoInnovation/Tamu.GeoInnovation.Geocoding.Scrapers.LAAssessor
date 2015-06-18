using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLab.Common.Utils.Databases;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Parcels;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.AINs
{
    public class AINCache : DatabaseTableSource
    {
        #region Properties
        private ParcelCache _ParcelCache;
        private AddressCache _AddressCache;

        public AddressCache AddressCache
        {
            get { return _AddressCache; }
            set { _AddressCache = value; }
        }
	

        public ParcelCache ParcelCache
        {
            get { return _ParcelCache; }
            set { _ParcelCache = value; }
        }
	
        #endregion

        public AINCache(string connectionString, string tableName, AddressCache addressCache, ParcelCache parcelCache)
            : base(connectionString, tableName)
        {
            AddressCache = addressCache;
            ParcelCache = parcelCache;
        }

        public AINCache(string dataSource, string catalog, string userName, string password, string tableName, AddressCache addressCache, ParcelCache parcelCache)
            : base(dataSource, catalog, userName, password, tableName)
        {
            AddressCache = addressCache;
            ParcelCache = parcelCache;
        }

        // get the exact cached assessorId for an address
        public string GetAIN(StreetAddress address)
        {

            string ret = null;
            if (address.Number != null && address.Number != "" && address.StreetName != null && address.StreetName != "")
            {

                if (StringUtils.IsEmpty(address.Number))
                {
                    address.Number = "0";
                }

                Read();

                string sql = "select assessorid ";
                sql += "from la_assessor_cache_parcels where ";
                sql += "number='" + address.Number + "' ";
                sql += DatabaseUtils.ParameterOrBlank("pre", address.PreDirectional, true, true);
                sql += "and name='" + DatabaseUtils.AsDbString(address.StreetName) + "' ";
                sql += DatabaseUtils.ParameterOrBlank("suffix", address.Suffix, true, true);
                sql += DatabaseUtils.ParameterOrBlank("post", address.PostDirectional, true, true);
                sql += "and State='" + address.State + "' ";
                sql += "and ( (city = '" + address.City + "') " +
                    " OR " +
                    " (zip = '" + address.ZIP + "')) ";

                try
                {


                    SqlCommand cmd1 = new SqlCommand(sql, OpenedConnection);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.CommandTimeout = 0;
                    SqlDataReader dr = cmd1.ExecuteReader();
                    if (dr.HasRows)
                    {
                        Hit();
                        while (dr.Read())
                        {
                            ret = dr["assessorid"].ToString();
                        }
                    }
                    else
                    {
                        Miss();
                    }

                    dr.Close();

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + " - at: " + e.StackTrace + " : " + sql);
                }
            }
            return ret;

        }


        // get the closest assessor Id for an address
        // - ret[0] = assesorId
        // - ret[1] = this addressExisted
        // - ret[2] = the address used (who's assessorId is being returned)
        public AssessorAddressInfo getAIN(ValidateableStreetAddress address, int from, int to)
        {
            AssessorAddressInfo assessorAddressInfo = new AssessorAddressInfo();


            CachedParcel cachedParcel = ParcelCache.checkExtractedParcelCache(address);
            // if it is in the cache return the assessorId
            if (cachedParcel != null)
            {
                assessorAddressInfo.AssessorId = cachedParcel.StreetAddress.AddressId;
                assessorAddressInfo.Valid = "true";
                assessorAddressInfo.AddressUsed = address.Number;
            }
            // otherwise find the closest address on the segment and return it's assessorId
            else
            {
                //AddressSegment addressRange = getActualAddressesOnSegment(address, from, to, segmentId, statistics);
                AddressRange addressRange = AddressCache.GetAddressesOnSegment(address, from, to);
                if (addressRange != null)
                {
                    int closest = addressRange.getLotNumber(address.NumberInt);
                    if (closest >= 0)
                    {
                        CachedParcel closestCachedAddress = ParcelCache.checkExtractedParcelCache(address, addressRange.getAddressAt(closest).ToString());
                        if (closestCachedAddress != null)
                        {
                            assessorAddressInfo.AssessorId = closestCachedAddress.StreetAddress.AddressId;
                            assessorAddressInfo.Valid = "false";
                            assessorAddressInfo.AddressUsed = closestCachedAddress.StreetAddress.Number;
                        }
                    }
                }
            }

            return assessorAddressInfo;
        }

    }
}
