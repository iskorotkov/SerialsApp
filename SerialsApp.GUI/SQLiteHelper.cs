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

            long seasonsWithEpisodes;
            long seasonsWithoutEpisodes;
            if (_cache.Contains(id))
            {
                (seasonsWithEpisodes, seasonsWithoutEpisodes) = _cache.Get(id);
            }
            else
            {
                using var con = OpenConnection();
                var seasonsCommand = BuildSeasonsCommand(id, con);

                long seasons;
                using (var reader = seasonsCommand.ExecuteReader())
                {
                    reader.Read();
                    seasons = (long) reader["count"];
                }

                var seasonsWithEpisodesCommand = BuildSeasonsWithEpisodesCommand(id, con);
                using (var reader = seasonsWithEpisodesCommand.ExecuteReader())
                {
                    reader.Read();
                    seasonsWithEpisodes = (long) reader["count"];
                }

                seasonsWithoutEpisodes = seasons - seasonsWithEpisodes;
                _cache.Add(id, seasonsWithEpisodes, seasonsWithoutEpisodes);
            }

            node.Header = $@"{node.Header} - {seasonsWithEpisodes}/{seasonsWithoutEpisodes}";
        }

        private static SQLiteCommand BuildSeasonsWithEpisodesCommand(long id, SQLiteConnection con)
        {
            return new SQLiteCommand()
            {
                Connection = con,
                CommandText = $@"SELECT count(DISTINCT seasons.id) as count
FROM seasons
INNER JOIN episodes ON seasons.id = episodes.season_id
WHERE seasons.serial_id = {id}"
            };
        }

        private static SQLiteCommand BuildSeasonsCommand(long id, SQLiteConnection con)
        {
            return new SQLiteCommand
            {
                Connection = con,
                CommandText = $@"SELECT count(DISTINCT seasons.id) as count
FROM seasons
WHERE seasons.serial_id = {id}"
            };
        }
    }
}
