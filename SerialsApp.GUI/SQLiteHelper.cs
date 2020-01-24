using System;
using System.Windows.Controls;
using System.Data.SQLite;

namespace SerialsApp.GUI
{
    public static class SQLiteHelper
    {
        public static void PopulateTree(TreeView tree)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                ForeignKeys = true,
                DataSource = @"C:\Projects\Databases\hw1.sqlite"
            };

            /*using */
            var connection = new SQLiteConnection(builder.ConnectionString);
            connection.Open();

            var command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = @"SELECT serials.id AS serial_id,
       serials.name AS serial_name,
       seasons.id AS season_id,
       seasons.name AS season_name,
       episodes.name AS episode_name
FROM serials
LEFT JOIN seasons ON serials.id = seasons.serial_id
LEFT JOIN episodes ON seasons.id = episodes.season_id
ORDER BY serials.name,
         serials.id,
         seasons.name,
         seasons.id,
         episodes.name"
            };
            using var reader = command.ExecuteReader();

            TreeViewItem serialNode = null;
            TreeViewItem seasonNode = null;
            long prevSerialId = -1;
            long prevSeasonId = -1;
            while (reader.Read())
            {
                var serialId = (long) reader["serial_id"];
                var serialName = (string) reader["serial_name"];
                if (serialId != prevSerialId)
                {
                    var index = tree.Items.Add(new TreeViewItem {Header = serialName});
                    serialNode = (TreeViewItem) tree.Items.GetItemAt(index);
                    prevSerialId = serialId;

                    var id = serialId;
                    var con = connection;
                    var node = serialNode;
                    
                }

                try
                {
                    var seasonId = (long) reader["season_id"];
                    var seasonName = (string) reader["season_name"];
                    if (seasonId != prevSeasonId)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var index = serialNode.Items.Add(new TreeViewItem {Header = seasonName});
                        seasonNode = (TreeViewItem) serialNode.Items.GetItemAt(index);
                        prevSeasonId = seasonId;
                    }

                    try
                    {
                        var episodeName = (string) reader["episode_name"];
                        // ReSharper disable once PossibleNullReferenceException
                        seasonNode.Items.Add(episodeName);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        static void Foo(TreeViewItem node, SQLiteConnection con, int id)
        {
            node.Expanded += (sender, args) =>
            {
                var seasonsCommand = new SQLiteCommand()
                {
                    Connection = con,
                    CommandText = $@"SELECT count(*)
FROM seasons
WHERE seasons.serial_id = {id}"
                };

                int seasons;
                using (var reader1 = seasonsCommand.ExecuteReader())
                {
                    reader1.Read();
                    seasons = (int) reader1["count"];
                }

                var seasonsWithEpisodesCommand = new SQLiteCommand()
                {
                    Connection = con,
                    CommandText = $@"SELECT count(*)
FROM (SELECT *
      FROM seasons
      WHERE seasons.serial_id = {id}) subquery
WHERE EXISTS(SELECT *
             FROM episodes
             WHERE episodes.id = subquery.id)"
                };

                int seasonsWithEpisodes;
                using (var reader2 = seasonsWithEpisodesCommand.ExecuteReader())

                {
                    reader2.Read();
                    seasonsWithEpisodes = (int) reader2["count"];
                }

                node.Header = $@"{node.Header} - {seasonsWithEpisodes}/{seasons - seasonsWithEpisodes}";
            };
        }
    }
}
