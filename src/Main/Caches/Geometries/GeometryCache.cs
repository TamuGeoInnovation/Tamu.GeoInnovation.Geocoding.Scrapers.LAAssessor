using System;
using System.Data;
using System.Data.SqlClient;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.GeographicObjects.Coordinates;
using USC.GISResearchLab.Common.Geometries;
using USC.GISResearchLab.Common.Geometries.Polygons;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
    /// <summary>
    /// Summary description for GeometryCacheUtils.
    /// </summary>
    public class GeometryCache : DatabaseTableSource
    {

        public GeometryCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public GeometryCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }

        public Polygon GetGeometry(string featureId)
        {
            Polygon ret = null;
            Read();

            string sql = null;
            sql += " select id, centerLat, centerLon, coordinates, geometryType ";
            sql += " from " + TableName;
            sql += " where ain = '" + featureId + "' ";

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

                        int id = dr.GetInt32(0);
                        double centerLat = dr.GetDouble(1);
                        double centerLon = dr.GetDouble(2);
                        string coordinates = dr.GetString(3);
                        GeometryType geometryType = (GeometryType)Convert.ToInt32(dr["geometryType"]);

                        switch (geometryType)
                        {
                            case GeometryType.Polygon:
                                ret = Polygon.FromCoordinateString(coordinates);
                                ret.CoordinateUnits = new DecimalDegrees();
                                ((Polygon)ret).Centroid = new double[] { centerLon, centerLat };
                                break;
                            default:
                                throw new Exception("GeometryType in goemetry Cache is not yet supported: " + geometryType);
                        }
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

        public void CacheGeometry(string ain, double centerX, double centerY, string coordinates, GeometryType geometryType)
        {
            Write();
            string sql = null;

            sql += "insert into " + TableName;
            sql += " ( ";
            sql += " ain, ";
            sql += " centerlon, ";
            sql += " centerlat, ";
            sql += " added, ";
            sql += " coordinates, ";
            sql += " geometryType ";
            sql += "  ) VALUES ";
            sql += " ( ";
            sql += " '" + ain + "',";
            sql += " '" + centerX + "',";
            sql += " '" + centerY + "',";
            sql += " '" + DateTime.Now + "',";
            sql += " '" + coordinates + "'),";
            sql += " '" + (int)geometryType + "'),";

            try
            {
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("GeometryCache method error: " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }
        }
    }
}
