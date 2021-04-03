﻿using System;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcSharp.HttpModule.Mvc;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Model {
    public class Player {
        
        public string PlayerId { get; private set; }
        public DateTime LoginDateTime { get; private set; }

        public Player(string playerId) {
            PlayerId = playerId;
            LoginDateTime = DateTime.Now;
        }

        public async Task<short> GetWorldTendency() {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT tendency FROM players WHERE player_id = @playerId;", conn);
            cmd.Parameters.AddWithValue("playerId", PlayerId);
            short worldTendency = 0;
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || !reader.HasRows) throw new HttpException(500, "Failed to get world tendency.");
            worldTendency = reader.GetInt16(0);
            return worldTendency;
        }

        public async Task<int[]> GetMultiPlayGrade() {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT grade_s, grade_a, grade_b, grade_c, grade_d, sessions FROM players WHERE player_id = @playerId;", conn);
            cmd.Parameters.AddWithValue("playerId", PlayerId);
            var data = new int[6];
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || !reader.HasRows) throw new HttpException(500, "Failed to get multi play grade.");
            data[0] = reader.GetInt32(0);
            data[1] = reader.GetInt32(1);
            data[2] = reader.GetInt32(2);
            data[3] = reader.GetInt32(3);
            data[4] = reader.GetInt32(4);
            data[5] = reader.GetInt32(5);
            return data;
        }

        public async Task<int> GetBloodMessageGrade() {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT msg_rating  FROM players WHERE player_id = @playerId;", conn);
            cmd.Parameters.AddWithValue("playerId", PlayerId);
            var rating = 0;
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || !reader.HasRows) throw new HttpException(500, "Failed to get message rating.");
            rating = reader.GetInt32(0);
            return rating;
        }
    }
}