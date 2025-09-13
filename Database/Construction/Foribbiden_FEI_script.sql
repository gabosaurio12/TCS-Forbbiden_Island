CREATE TABLE player (
player_id int identity(1,1) primary key,
player_name nvarchar(100) not null,
player_username nvarchar(30) unique,
player_password nvarchar(64) not null,
player_email nvarchar(320) unique,
player_avatar nvarchar(500),
player_socialmedia nvarchar(500)
)

CREATE TABLE tile (
tile_id int identity(1,1) primary key,
flooded_level int not null,
type nvarchar(30) not null
)

CREATE TABLE card_catalog (
card_type nvarchar(10)
)

CREATE TABLE card (
card_id int identity(1,1) primary key,
card_name nvarchar(30) not null,
description nvarchar(150) not null,
type nvarchar(10) not null
)

CREATE TABLE minigame (
minigame_id int identity(1,1) primary key,
minigame_name nvarchar(100) unique,
timer int
)

CREATE TABLE matches (
match_id int identity(1,1) primary key
)

CREATE TABLE match_players (
match_players_id int identity(1,1) primary key,
match_id int not null,
player_id int not null,
CONSTRAINT FK_match FOREIGN KEY (match_id) REFERENCES matches(match_id),
CONSTRAINT FK_player FOREIGN KEY (player_id) REFERENCES player(player_id)
)

CREATE TABLE match_tiles (
match_tiles_id int identity(1,1) primary key,
match_id int not null,
tile_id not null,
CONSTRAINT FK_match FOREIGN KEY (match_id) REFERENCES matches(match_id),
CONSTRAINT FK_tile FOREIGN KEY (tile_id) REFERENCES tile(tile_id)
)

CREATE TABLE match_cards (
match_cards_id int identity(1,1) primary key,
match_id int not null,
card_id not null,
CONSTRAINT FK_match FOREIGN KEY (match_id) REFERENCES matches(match_id),
CONSTRAINT FK_card FOREIGN KEY (card_id) REFERENCES card(card_id)
)

CREATE TABLE match_minigames (
match_minigames_id int identity(1,1) primary key,
match_id int not null,
minigame_id not null,
CONSTRAINT FK_match FOREIGN KEY (match_id) REFERENCES matches(match_id),
CONSTRAINT FK_minigame FOREIGN KEY (minigame_id) REFERENCES minigame(minigame_id)
)