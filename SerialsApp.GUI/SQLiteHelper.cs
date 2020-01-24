﻿using System.Windows.Controls;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace SerialsApp.GUI
{
    public static class SQLiteHelper
    {
        public static void PopulateTree(TreeView tree)
        {
            using var connection = OpenConnection();
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

                    var node = serialNode;
                    var id = serialId;
                    serialNode.Expanded += (sender, args) => { RefineNodeHeader(node, id); };
                }

                if (!(reader["season_id"] is long seasonId)) continue;

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

        private static void RefineNodeHeader(HeaderedItemsControl node, long id)
        {
            if (Regex.IsMatch(node.Header.ToString(), @".* - (\d)+?/(\d)+?"))
            {
                return;
            }

            using var con = OpenConnection();
            var seasonsCommand = new SQLiteCommand
            {
                Connection = con,
                CommandText = $@"SELECT count(DISTINCT seasons.id) as count
FROM seasons
WHERE seasons.serial_id = {id}"
            };

            long seasons;
            using (var reader = seasonsCommand.ExecuteReader())
            {
                reader.Read();
                seasons = (long) reader["count"];
            }

            var seasonsWithEpisodesCommand = new SQLiteCommand()
            {
                Connection = con,
                CommandText = $@"SELECT count(DISTINCT seasons.id) as count
FROM seasons
INNER JOIN episodes ON seasons.id = episodes.season_id
WHERE seasons.serial_id = {id}"
            };

            long seasonsWithEpisodes;
            using (var reader = seasonsWithEpisodesCommand.ExecuteReader())

            {
                reader.Read();
                seasonsWithEpisodes = (long) reader["count"];
            }

            node.Header = $@"{node.Header} - {seasonsWithEpisodes}/{seasons - seasonsWithEpisodes}";
        }
    }
}
