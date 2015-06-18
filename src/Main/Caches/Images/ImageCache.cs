using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.Utils.Web.Images;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Images
{
    public class ImageCache : DatabaseTableSource
    {
        public ImageCache(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public ImageCache(string dataSource, string catalog, string userName, string password, string tableName)
            : base(dataSource, catalog, userName, password, tableName)
        { }

        public AssessorImage GetImage(string assessorid)
        {
            AssessorImage ret = null;

            string sql = null;
            try
            {
                Read();

                sql = " select ";
                sql += " [imageFormat], [image], [mapX], ";
                sql += " [mapY], [mapXMax], [mapYMax], [added], ";
                sql += " [srs], [size], [resolution]";
                sql += " from " + TableName;
                sql += " where ";
                sql += " [assessorid] = '" + assessorid + "'";

                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    Hit();
                    while (dr.Read())
                    {
                        ret = new AssessorImage();
                        ret.X = Convert.ToDouble(dr["mapX"]);
                        ret.XMax = Convert.ToDouble(dr["mapXMax"]);
                        ret.Y = Convert.ToDouble(dr["mapY"]);
                        ret.YMax = Convert.ToDouble(dr["mapYMax"]);
                        ret.SRS = Convert.ToString(dr["srs"]);
                        ret.Size = Convert.ToInt32(dr["size"]);
                        ret.Resolution = Convert.ToInt32(dr["resolution"]);
                        ret.ImageFormat = Convert.ToString(dr["imageFormat"]);

                        byte[] imageBytes = (byte[])dr["image"];
                        ret.Image = ImagesUtils.ImageFromByteArray(imageBytes);

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
                throw new Exception("image caching error : " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }

            return ret;
        }

        public void CacheImage(string assessorid, string url, Image image, double mapX, double mapY, double mapXMax, double mapYMax, string srs, double size, double resolution)
        {
            string sql = null;
            try
            {
                Write();
                ImageFormat imageFormat = ImagesUtils.GetFormat(url);
                string imageFormatString = ImagesUtils.GetExtension(imageFormat);

                byte[] content = ImagesUtils.ImageToByteArray(image, imageFormat);
                //ImageSre imagestream = info.OpenRead();
                //imagestream.Read(content, 0, content.Length);
                //imagestream.Close();

                sql = " insert into " + TableName;
                sql += " (";
                sql += " [assessorid],[imageFormat], [image], [mapX], [mapY], [mapXMax], [mapYMax], [added], ";
                sql += " [srs], [size], [resolution])values(@assessorid, @imageFormat, @image, @mapX, @mapY, @mapXMax, @mapYMax, @added, @srs, @size, @resolution)";
                SqlCommand cmd = new SqlCommand(sql, OpenedConnection);

                cmd.Parameters.Add(new SqlParameter("@assessorid", assessorid));
                cmd.Parameters.Add(new SqlParameter("@imageformat", imageFormatString));
                cmd.Parameters.Add(new SqlParameter("@mapX", mapX));
                cmd.Parameters.Add(new SqlParameter("@mapY", mapY));
                cmd.Parameters.Add(new SqlParameter("@mapXMax", mapXMax));
                cmd.Parameters.Add(new SqlParameter("@mapYMax", mapYMax));
                cmd.Parameters.Add(new SqlParameter("@added", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("@srs", srs));
                cmd.Parameters.Add(new SqlParameter("@size", size));
                cmd.Parameters.Add(new SqlParameter("@resolution", resolution));
                SqlParameter imageParameter = new SqlParameter("@image", SqlDbType.Image);
                imageParameter.Value = content;
                cmd.Parameters.Add(imageParameter);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw new Exception("image caching error : " + e.Message + " - at: " + e.StackTrace + " : " + sql);
            }


        }
    }
}
