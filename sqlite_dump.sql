INSERT INTO episodes (id, name, season_id) VALUES (4, 'Beginning', 1);
INSERT INTO episodes (id, name, season_id) VALUES (5, 'Ending', 1);
INSERT INTO episodes (id, name, season_id) VALUES (6, 'Some episode', 4);
INSERT INTO episodes (id, name, season_id) VALUES (7, 'My first day', 5);
INSERT INTO episodes (id, name, season_id) VALUES (8, 'My mentor', 5);
INSERT INTO episodes (id, name, season_id) VALUES (9, 'My best friend''s mistake', 5);
INSERT INTO episodes (id, name, season_id) VALUES (10, 'My overkill', 6);
INSERT INTO episodes (id, name, season_id) VALUES (11, 'My nightingale', 6);
INSERT INTO episodes (id, name, season_id) VALUES (12, 'My own American girl', 7);
INSERT INTO episodes (id, name, season_id) VALUES (13, 'My journey', 7);
INSERT INTO episodes (id, name, season_id) VALUES (14, 'Episode from S4', 8);
INSERT INTO seasons (id, name, serial_id) VALUES (1, 'Season 20', 3);
INSERT INTO seasons (id, name, serial_id) VALUES (3, 'Season 21', 3);
INSERT INTO seasons (id, name, serial_id) VALUES (4, 'Something', 1);
INSERT INTO seasons (id, name, serial_id) VALUES (5, 'Season 1', 4);
INSERT INTO seasons (id, name, serial_id) VALUES (6, 'Season 2', 4);
INSERT INTO seasons (id, name, serial_id) VALUES (7, 'Season 3', 4);
INSERT INTO seasons (id, name, serial_id) VALUES (8, 'Season 4', 5);
INSERT INTO seasons (id, name, serial_id) VALUES (9, 'Season 4', 5);
INSERT INTO serials (id, name) VALUES (1, 'Simpsons');
INSERT INTO serials (id, name) VALUES (2, 'American Dad');
INSERT INTO serials (id, name) VALUES (3, 'South Park');
INSERT INTO serials (id, name) VALUES (4, 'Scrubs');
INSERT INTO serials (id, name) VALUES (5, 'Simpsons');
INSERT INTO sqlite_master (type, name, tbl_name, rootpage, sql) VALUES ('table', 'episodes', 'episodes', 2, 'CREATE TABLE episodes (
    id integer NOT NULL,
    name character varying NOT NULL,
    season_id integer NOT NULL
)');
INSERT INTO sqlite_master (type, name, tbl_name, rootpage, sql) VALUES ('table', 'seasons', 'seasons', 3, 'CREATE TABLE seasons (
    id integer NOT NULL,
    name character varying NOT NULL,
    serial_id integer NOT NULL
)');
INSERT INTO sqlite_master (type, name, tbl_name, rootpage, sql) VALUES ('table', 'serials', 'serials', 4, 'CREATE TABLE serials (
    id integer NOT NULL,
    name character varying NOT NULL
)');
INSERT INTO sqlite_master (type, name, tbl_name, rootpage, sql) VALUES ('table', 'sqlite_sequence', 'sqlite_sequence', 6, 'CREATE TABLE sqlite_sequence(name,seq)');
