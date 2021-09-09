using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace SocialSpace
{
    public class Helper
    {
        public static string GetConnectionString()
        {
            return @"Data Source = .\Database\SocialHubDb.db; Version=3; providerName=System.Data.SqlClient";
        }
        public static List<SocialApp> GetSocialApps()
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());
            string query = "SELECT * FROM SocialApps;";
            var items = con.Query<SocialApp>(query, new DynamicParameters()).ToList();

            foreach (var item in items)
            {
                item.IconSource = "https://api.statvoo.com/favicon/?url="+item.DefaultAddress;
            }
            return items;
        }
        public static bool SaveSocialApp(SocialApp item)
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());

            string query = "INSERT INTO SocialApps (AppTitle, DefaultAddress) VALUES (@AppTitle,@DefaultAddress)";
            if (item.Id > 0)
            {
                query = @"UPDATE SocialMenuItems SET AppTitle = @AppTitle, DefaultAddress = @DefaultAddress WHERE Id = @Id;";
            }

            var rowsAffected = con.Execute(query, item);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public static bool DeleteSocialApp(SocialApp item)
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());
            string query = @"DELETE FROM SocialApps WHERE Id = @Id; DELETE FROM WorkSpacePages Where @AppId = @AppId";
            var rowsAffected = con.Execute(query, new
            {
                Id = item.Id,
                AppId = item.Id
            });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public static List<WorkSpace> GetWorkSpaces()
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());
            string query = "SELECT * FROM WorkSpaces ws " +
                           "LEFT JOIN WorkSpacePages wp On wp.WorkSpaceId = ws.Id;";

            List<WorkSpace> spaces = new List<WorkSpace>();

            var data = con.Query<WorkSpace, WorkSpacePage, WorkSpace>(query, (space, item) =>
            {
                var spaceExisted = spaces.FirstOrDefault(x => x.Id == space.Id);
                if (spaceExisted == null)
                {
                    spaces.Add(space);
                    space.WorkSpaceItems = new List<WorkSpacePage>();
                    space.WorkSpaceItems.Add(item);
                }
                else
                {
                    spaceExisted.WorkSpaceItems.Add(item);
                }

                return space;
            });

            return spaces;
        }

        public static int ChangeActiveSpace(WorkSpace space)
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());

            string query = @"Update WorkSpaces SET IsActive = 'False' WHERE IsActive = 'True' ;";
            var s = con.Execute(query);

            query = @"UPDATE WorkSpaces SET WorkSpaceName = @WorkSpaceName, IsActive = @IsActive WHERE Id = @Id;";
            var result = con.Execute(query, space);
            if (result > 0)
            {
                return space.Id;
            }
            else
            {
                return 0;
            }
        }

        public static int SaveWorkSpace(WorkSpace space)
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());

            string query =
                "INSERT INTO WorkSpaces (WorkSpaceName , IsActive) VALUES (@WorkSpaceName,@IsActive); Select last_insert_rowid()";
            int spaceId = 0;
            if (space.Id > 0)
            {
                spaceId = ChangeActiveSpace(space);
            }
            else
            {
                ChangeActiveSpace(space); // update previous spaces to False
                var result = con.ExecuteScalar(query, space);
                spaceId = Convert.ToInt32(result == DBNull.Value ? 0 : result);
            }


            if (spaceId > 0)
            {
                query = @"DELETE FROM WorkSpacePages WHERE WorkSpaceId = " + spaceId + ";";
                var s = con.Execute(query);

                foreach (var item in space.WorkSpaceItems)
                {
                    item.WorkSpaceId = spaceId;
                    query = "INSERT INTO WorkSpacePages (WorkSpaceId, AppId , Address) VALUES (@WorkSpaceId, @AppId,@Address)";
                    if (item.Id > 0)
                    {
                        query =
                            @"UPDATE WorkSpacePages SET WorkSpaceId = @WorkSpaceId, AppId = @AppId, Address = @Address WHERE Id = @Id;";
                    }

                    var resp = con.Execute(query, item);
                }

                return spaceId;
            }

            return 0;
        }

        public static bool DeleteWorkSpace(WorkSpace workspace)
        {
            using IDbConnection con = new SQLiteConnection(GetConnectionString());
            string query = @"Delete from WorkSpaces WHERE Id = @Id; Delete from WorkSpacePages Where WorkSpaceId = @WorkSpaceId";
            var rowsAffected = con.Execute(query, new
            {
                Id = workspace.Id,
                WorkSpaceId = workspace.Id
            });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }
    } // end of class
}