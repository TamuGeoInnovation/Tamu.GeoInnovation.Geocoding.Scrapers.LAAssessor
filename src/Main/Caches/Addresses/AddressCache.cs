using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLab.Common.Utils.Databases;
using USC.GISResearchLab.Common.Utils.Strings;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
	/// <summary>
	/// Summary description for AddressCache.
	/// </summary>
    public class AddressCache : DatabaseTableSource
    {

        public AddressCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }


        public AddressCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }

        public bool AddressExists(StreetAddress address)
        {
            bool ret = false;

            if (address.Number != null && address.Number != "" && address.StreetName != null && address.StreetName != "")
            {

                Read();

                string sql = "select id  ";
                sql += " from " + TableName;
                sql += " where ";
                sql += " number='" + address.Number + "' ";
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
                            string id = dr["id"].ToString();
                            ret = true;
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

        // get the actual addresses which exist on a segment
        public AddressRange GetAddressesOnSegment(StreetAddress address, int from, int to)
        {
            Read();
            AddressRange actualAddresses = new AddressRange();

            string sql = null;

            try
            {
                string sort;

                if (from < to)
                {
                    sort = "ASC";
                }
                else
                {
                    sort = "DESC";
                }

                int evenOdd = Convert.ToInt32(address.Number) % 2;


                sql = "select number ";
                sql += " from " + TableName;
                sql += " where ";
                sql += " name = '" + DatabaseUtils.AsDbString(address.StreetName) + "' ";
                sql += DatabaseUtils.ParameterOrBlank("pre", address.PreDirectional, true, true);
                sql += DatabaseUtils.ParameterOrBlank("suffix", address.Suffix, true, true);
                sql += DatabaseUtils.ParameterOrBlank("post", address.PostDirectional, true, true);
                sql += DatabaseUtils.ParameterOrBlank("state", address.State, true, true);

                sql += "and ( (city = '" + address.City + "') " +
                    " OR " +
                    " (zip = '" + address.ZIP + "')) ";

                sql += " and (number >= '" + Math.Min(from, to) + "' and number <= '" + Math.Max(from, to) + "')";
                sql += " and convert(int,number%2) = " + evenOdd;
                sql += " order by number " + sort;

                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Hit();
                    while (dr.Read())
                    {
                        actualAddresses.addAddress(dr.GetInt32(0));
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

            return actualAddresses;
        }

        public void CacheAddress(StreetAddress address, string streetNumber, string assessorId, int hasMultipleAssessorIds)
        {
            if (StringUtils.IsEmpty(streetNumber))
            {
                streetNumber = "0";
            }

            Write();

            string sql = "";
            sql += "insert into  " + TableName;
            sql += " ( ";
            sql += " hasMultipleAssessorIds, ";
            sql += " assessorId, ";
            sql += " number, ";
            sql += " pre, ";
            sql += " name, ";
            sql += " suffix, ";
            sql += " post, ";
            sql += " city, ";
            sql += " State, ";
            sql += " added, ";
            sql += " zip ";
            sql += " ) VALUES ";
            sql += " (";
            sql += " '" + hasMultipleAssessorIds + "',";
            sql += " '" + assessorId + "',";
            sql += " '" + streetNumber + "',";
            sql += " '" + DatabaseUtils.AsDbString(address.PreDirectional) + "',";
            sql += " '" + DatabaseUtils.AsDbString(address.StreetName) + "',";
            sql += " '" + DatabaseUtils.AsDbString(address.Suffix) + "',";
            sql += " '" + DatabaseUtils.AsDbString(address.PostDirectional) + "',";
            sql += " '" + DatabaseUtils.AsDbString(address.City) + "',";
            sql += " '" + address.State + "',";
            sql += " '" + DateTime.Now + "',";
            sql += " '" + address.ZIP + "' ";
            sql += " )";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("uniform-lot method error: " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }
        }


        //		

        /*
        public static void fillInEmptyInvalidAddresses(Address address, int fromAddress, int toAddress, string currentAddress, AssessorStatistics statistics)
        {
            try
            {
                int start = fromAddress;
                int end = toAddress;
                if (end < start)
                {
                    end = fromAddress;
                    start = toAddress;
                }

                string startString = start.ToString();
                int pos = currentAddress.Length;
                int size = startString.Length - pos;

                // if no number passesed to assessor and return < 500, then invalidate all addresses on segment
                if (pos == 0)
                {
                    for (int i=start; i<=end; i++)
                    {
                        cacheInvalidAddress(address, i.ToString(), true, 0, statistics);
                    }
                }
                    // in all other cases, append 0's to the remaining portion of start for the starting point and 9's to the remaining portion of start for the ending point 
                else
                {
				

                    // wierd cases to worry about:
                    //
                    // 1) range is 1400 - 1599 and current addressSubstring is 15
                    //     -- in this case the baseStartString should be 15 not 14
                    //     -- we can just check to see if the addressSubstring is greater than the equivalent length start substring
                    //     -- if so, use the addressSubstring as the startpiece instead
                    // 2) range is 707 - 4057 and current number is 1021
                    //     -- in this case the pos is greater than the length of the start string, so just use the whole start string

                    string baseStartString = null;
                    if (pos > startString.Length)
                    {
                        baseStartString = startString;
                    }
                    else
                    {
                        baseStartString = startString.Substring(0, pos);
                    }

                    if (System.Convert.ToInt32(baseStartString) < System.Convert.ToInt32(currentAddress))
                    {
                        baseStartString = currentAddress;
                    }
				
                    string newStartString = "";
                    string newEndString = "";
                    for (int i=0; i<size; i++)
                    {
                        newStartString += "0";
                        newEndString += "9";
                    }

                    int newStart = System.Convert.ToInt32(baseStartString + newStartString);
                    int newEnd = System.Convert.ToInt32(baseStartString + newEndString);

                    for (int i=newStart; i<=newEnd; i++)
                    {
                        cacheInvalidAddress(address, i.ToString(), true, 0, statistics);				
                    }
				

                }
            }
            catch (Exception e)
            {
                throw new GeocodeMethodUniformLotException("An error occurred while filling in empty invalid addresses: " + e.Message);
            }
        }
        */

    }
}
