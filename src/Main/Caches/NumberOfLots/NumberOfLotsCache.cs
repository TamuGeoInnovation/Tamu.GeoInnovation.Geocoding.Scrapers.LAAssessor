using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Databases;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.NumberOfLots
{
    public class NumberOfLotsCache : DatabaseTableSource
    {

        public NumberOfLotsCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public NumberOfLotsCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }


        public int[] GetNumberOfLots(string source, string segmentId)
        {
            int[] ret = null;

            Read();
            string sql = "";
            try
            {
                sql += " select evenparcelsonblock, oddparcelsonblock";
                sql += " from  " + TableName;
                sql += " where ";
                sql += " sourceid='" + segmentId + "'";
                sql += " and ";
                sql += " source='" + source + "'";

                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Hit();
                    while (dr.Read())
                    {
                        ret = new int[2];
                        ret[0] = dr.GetInt32(0);
                        ret[1] = dr.GetInt32(1);
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


            return ret;

        }

        public int GetNumberOfLots(StreetAddress address, string segmentId, string source)
        {
            int ret = -1;

            Read();
            string sql = "";
            try
            {
                int streetNumber = Convert.ToInt32(address.Number);
                int evenOdd = streetNumber % 2;
                string evenOddColumn;
                if (evenOdd == 0)
                {
                    evenOddColumn = "evenparcelsonblock";
                }
                else
                {
                    evenOddColumn = "oddparcelsonblock";
                }
                sql += " select " + evenOddColumn;
                sql += " from  " + TableName;
                sql += " where ";
                sql += " sourceid='" + segmentId + "'";
                sql += " and ";
                sql += " source='" + source + "'";

                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Hit();
                    while (dr.Read())
                    {

                        ret = dr.GetInt32(0);
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


            return ret;

        }


        public void CacheNumberOfLots(StreetAddress address, double parcelsOnBlock, int segmentId, string source)
        {
            Update();
            string sql = "";
            try
            {

                int streetNumber = Convert.ToInt32(address.Number);
                int evenOdd = streetNumber % 2;
                string evenOddColumn;

                if (evenOdd == 0)
                {
                    evenOddColumn = "evenparcelsonblock";
                }
                else
                {
                    evenOddColumn = "oddparcelsonblock";
                }
                sql = "update  " + TableName +
                    " set " + evenOddColumn + " ='" + parcelsOnBlock + "' " +
                    " where " +
                    " sourceid ='" + segmentId + "' and source='" + source + "'";


                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " - at: " + e.StackTrace + " : " + sql);
            }

        }


        public void CacheNumberOfLotsEven(double parcelsOnBlock, string source, string segmentId)
        {
            Update();
            string sql = null;
            sql = "update  " + TableName;
            sql += " set evenparcelsonblock ='" + parcelsOnBlock + "' ";
            sql += " where ";
            sql += " sourceid ='" + segmentId + "' ";
            sql += " and source ='" + source + "' ";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " - at: " + e.StackTrace + " : " + sql);
            }
        }


        public void CacheNumberOfLotsOdd(double parcelsOnBlock, string source, string segmentId)
        {
            Update();
            string sql = null;
            sql = "update  " + TableName;
            sql += " set oddparcelsonblock ='" + parcelsOnBlock + "' ";
            sql += " where ";
            sql += " sourceid ='" + segmentId + "' ";
            sql += " and source ='" + source + "' ";

            try
            {

                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " - at: " + e.StackTrace + " : " + sql);
            }

        }


    }
}
