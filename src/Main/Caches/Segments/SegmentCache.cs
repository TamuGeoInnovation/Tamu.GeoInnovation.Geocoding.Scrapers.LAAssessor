using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.Exceptions.Geocoding.Cache;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Segments
{
    public class SegmentCache : DatabaseTableSource
    {
        public SegmentCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public SegmentCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }

        public void CacheSegment(string segmentId, string source)
        {

            Write();
            string sql = null;
            sql = "insert into  " + TableName +
                " (sourceid, source, evenparcelsonblock, oddparcelsonblock, added) " +
                "  VALUES " +
                " ('" + segmentId + "', '" + source + "', '-1', '-1', '" + DateTime.Now + "'," + ")";

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

        public CachedSegment GetSegment(string source, int segmentId)
        {
            Read();
            CachedSegment ret = null;

            string sql = null;
            sql = "select id, evenparcelsonblock,  oddparcelsonblock ";
            sql += " from  " + TableName;
            sql += " where ";
            sql += " sourceid='" + segmentId + "' ";
            sql += " and source='" + source + "' ";

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
                        ret = new CachedSegment();
                        ret.id = dr.GetInt32(0);
                        ret.parcelsOnEvenSide = dr.GetInt32(1);
                        ret.parcelsOnOddSide = dr.GetInt32(2);
                        ret.segmentid = segmentId;
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
                throw new SegmentCacheException(e.Message + " - at: " + e.StackTrace + " : " + sql);
            }
            return ret;
        }

        public CachedSegment GetSegment(int streetNumber, int segmentId)
        {
            Read();
            CachedSegment ret = null;
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

            string sql = null;
            sql = "select id, " + evenOddColumn +
                " from la_assessor_cache_segments " +
                " where " +
                " sourceid='" + segmentId + "'";

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
                        ret = new CachedSegment();
                        ret.id = dr.GetInt32(0);
                        ret.parcelsonblock = dr.GetInt32(1);
                        ret.segmentid = segmentId;
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
                throw new SegmentCacheException(e.Message + " - at: " + e.StackTrace + " : " + sql);
            }



            return ret;

        }
    }
}
