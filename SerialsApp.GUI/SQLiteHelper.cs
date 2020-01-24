using System;
using System.Windows.Controls;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;

namespace SerialsApp.GUI
{
    public class SQLiteHelper
    {
        private readonly SerialsCache _cache = new SerialsCache();

        public void PopulateTree(TreeView tree)
        {
            using var connection = OpenConnection();
            var command = BuildGetAllCommand(connection);
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

                    var node = serialNode;
                    var id = serialId;
                    serialNode.Expanded += (sender, args) => { RefineNodeHeader(args, node, id); };
                    serialNode.Collapsed += (sender, args) => { ClearNodeHeader(args, node); };
                }

                if (reader["season_id"] is long seasonId)
                {
                    var seasonName = (string) reader["season_name"];
                    if (seasonId != prevSeasonId)
                    {
                        var index = serialNode.Items.Add(new TreeViewItem {Header = seasonName});
                        seasonNode = (TreeViewItem) serialNode.Items.GetItemAt(index);
                        prevSeasonId = seasonId;
                    }

                    if (reader["episode_name"] is string episodeName)
                    {
                        seasonNode.Items.Add(episodeName);
                    }
                }
            }
        }

        private static SQLiteCommand BuildGetAllCommand(SQLiteConnection connection)
        {
            return new SQLiteCommand
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
        }

        private static void ClearNodeHeader(RoutedEventArgs args, HeaderedItemsControl node)
        {
            if (!ReferenceEquals(args.OriginalSource, node))
            {
                return;
            }

            var s = Regex.Match(node.Header.ToString(), @"(.*?) - \d+/\d+").Groups[1].Value;
            node.Header = s;
        }

        private static SQLiteConnection OpenConnection()
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                ForeignKeys = true,
                DataSource = @"C:\Projects\Databases\hw1.sqlite"
            };

            var connection = new SQLiteConnection(builder.ConnectionString);
            connection.Open();
            return connection;
        }

        private void RefineNodeHeader(RoutedEventArgs args, HeaderedItemsControl node, long id)
        {
            if (!ReferenceEquals(args.OriginalSource, node))
            {
                return;
            }
            
            if (!_cache.Contains(id))
            {
                ReadAndCache();
            }
            var (seasonsWithEpisodes, seasonsWithoutEpisodes) = _cache.Get(id);
            node.Header = $@"{node.Header} - {seasonsWithEpisodes}/{seasonsWithoutEpisodes}";
        }

        private void ReadAndCache()
        {
            using var con = OpenConnection();
            var seasonsCommand = BuildAllSeasonsCommand(con);
            using var reader = seasonsCommand.ExecuteReader();
            while (reader.Read())
            {
                var id = (long) reader["id"];
                var total = (long) reader["total"];
                var withEpisodes = (long) reader["with_episodes"];
                _cache.Add(id, withEpisodes, total - withEpisodes);
            }
        }

        private static SQLiteCommand BuildAllSeasonsCommand(SQLiteConnection con)
        {
            return new SQLiteCommand
            {
                Connection = con,
                CommandText = @"-- total seasons
SELECT serials.id,
       count(DISTINCT seasons.id) AS total,
       query.with_episodes
FROM serials
         LEFT JOIN seasons ON serials.id = seasons.serial_id
         LEFT JOIN (
    -- seasons with episodes
    SELECT s.id,
           count(DISTINCT query.season_id) AS with_episodes
    FROM serials AS s
             LEFT JOIN (
        SELECT seasons.id        AS season_id,
               seasons.serial_id AS serial_id
        FROM seasons
                 JOIN episodes ON seasons.id = episodes.season_id
    ) AS query ON s.id = query.serial_id
    GROUP BY s.id
) AS query ON query.id = serials.id
GROUP BY serials.id"
            };
        }
    }
}
