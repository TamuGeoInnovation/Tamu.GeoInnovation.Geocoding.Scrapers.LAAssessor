using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.Utils.Databases;
using USC.GISResearchLab.Common.Utils.Strings;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
    /// <summary>
    /// Summary description for QueryCache.
    /// </summary>
    public class QueryCache : DatabaseTableSource
    {
        public QueryCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public QueryCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }

        public CachedQuery checkQueriesCache(StreetAddress address, string number)
        {
            return checkQueriesCache(number, address.StreetName);
        }

        public CachedQuery checkQueriesCache(string number, string name)
        {
            CachedQuery cachedQuery = null;
            Read();

            if (StringUtils.IsEmpty(number))
            {
                number = "0";
            }

            string sql = "";
            sql += " select [id], [resultCount] ";
            sql += " from  [" + TableName + "]";
            sql += " where ";
            sql += " [number]=" + number + " and ";
            sql += " [name]='" + DatabaseUtils.AsDbString(name) + "' ";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Hit();
                    while (dr.Read())
                    {
                        cachedQuery = new CachedQuery();

                        cachedQuery.id = dr.GetInt32(0);
                        cachedQuery.resultCount = dr.GetInt32(1);
                        cachedQuery.number = number;
                        cachedQuery.name = name;
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
                throw new Exception("checkQueryCache error: " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }
            return cachedQuery;
        }


        public void cacheQueryStart(ValidateableStreetAddress address, string number)
        {
            cacheQuery(number, address.StreetName, 0);
        }


        public void cacheQueryStart(string number, string name)
        {
            cacheQuery(number, name, 0);
        }


        public void cacheQuery(ValidateableStreetAddress address, string number, int resultCount)
        {
            cacheQuery(number, address.StreetName, resultCount);
        }


        public void cacheQuery(string number, string name, int resultCount)
        {
            Write();
            if (StringUtils.IsEmpty(number))
            {
                number = "0";
            }

            string sql = null;
            sql = "insert into " + TableName +
                " ( " +
                " number, " +
                " name, " +
                " added, " +
                " resultCount ) VALUES " +
                " ( " +
                " '" + number + "'," +
                " '" + name + "'," +
                " '" + DateTime.Now + "'," +
                " '" + resultCount + "')";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("queyr cache error: " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }

        }


        public void updateCacheQueryWithResultCount(string number, string name, int resultCount)
        {

            Write();
            if (StringUtils.IsEmpty(number))
            {
                number = "0";
            }

            string sql = null;
            sql = "update  " + TableName +
                " set " +
                " resultCount='" + resultCount + "' " +
                " where " +
                " number = '" + number + "' and " +
                " name = '" + DatabaseUtils.AsDbString(name) + "'";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw new Exception("query cache error: " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }


        }

    }
}
