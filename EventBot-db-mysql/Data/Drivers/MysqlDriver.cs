using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Discord;
using EventBot.lib.Data;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Digests;

namespace EventBot.db.mysql.Data.Drivers {
    public class MysqlDriverConfig {

        public string server;
        public string userid;
        public string password;
        public string database;

        public string GetConnectionString() {
            string s = $"server={server};userid={userid};password={password}";
            if (!string.IsNullOrEmpty(database))
                s += $";database={database}";
            return s;
        }

        public string connectionString => GetConnectionString();
    }
    
    public class MysqlDriver : IDatabaseDriver {

        private MySqlConnection connection;
        
        public void Init(MysqlDriverConfig config) {
            connection = new MySqlConnection(config.connectionString);
            connection.Open();
        }

        public void Close() {
            connection.Close();
        }

        public void CreateTable(string tableName, params DBColumnAttribute[] fields) {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = GetCreateString(tableName, fields);
            cmd.ExecuteNonQuery();
        }

        public string GetCreateString(string tableName, DBColumnAttribute[] fields) {
            string s = fields.Select(ConvertFieldToCreateString).Aggregate((c, s) => c + ", " + s);
            return $"CREATE TABLE {tableName}({s});";
        }

        private string ConvertFieldToCreateString(DBColumnAttribute field) {
            string s = field.name + " " + MapType(field.type);
            if (field.primaryKey)
                s += " PRIMARY KEY";
            if (field.autoincrement)
                s += " AUTO_INCREMENT";
            return s;
        }

        private string MapType(DBColumnType type) {
            switch(type) {
                case DBColumnType.STRING:
                    return "TEXT";
                case DBColumnType.INTEGER:
                    return "INTEGER";
                case DBColumnType.LONG_INTEGER:
                    return "BIGINT";
                case DBColumnType.LONG_INTEGER_UNSIGNED:
                    return "BIGINT UNSIGNED";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Insert<T>(T item) {
            
            MySqlCommand cmd = connection.CreateCommand();
            DBColumnAttribute[] dbColumnAttributes = DatabaseService.GetFields<T>();
            string aggregate = dbColumnAttributes.Select(x => x.name).Aggregate((c, s) => c + ", " + s);
            string values = dbColumnAttributes.Select(x => "@" + x.name).Aggregate((c, s) => c + ", " + s);
            cmd.CommandText = $"INSERT INTO {DatabaseService.GetTableName<T>()}({aggregate}) VALUES({values})";
            foreach (DBColumnAttribute field in dbColumnAttributes) {
                object value = field.attachedField.GetValue(item);
                cmd.Parameters.AddWithValue("@" + field.name, value);
            }

            try {
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            } catch (MySqlException e) {
                const int SQL_TABLE_DOESNT_EXIST = 1146;
                if (e.Number == SQL_TABLE_DOESNT_EXIST) {
                    DatabaseService.Instance.CreateTable<T>();
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                } else 
                    throw e;
            }
        }

        public void Update<T>(T item) {
            MySqlCommand cmd = connection.CreateCommand();
            DBColumnAttribute[] dbColumnAttributes = DatabaseService.GetFields<T>();
            DBColumnAttribute primaryKey = dbColumnAttributes.First(x => x.primaryKey);
            string values = dbColumnAttributes.Where(x => !x.primaryKey).Select(x => $"{x.name}=@{x.name}").Aggregate((c, s) => c + ", " + s);
            cmd.CommandText = $"UPDATE {DatabaseService.GetTableName<T>()} SET {values} WHERE {primaryKey.name}=@{primaryKey.name}";
            foreach (DBColumnAttribute field in dbColumnAttributes) {
                object value = field.attachedField.GetValue(item);
                cmd.Parameters.AddWithValue("@" + field.name, value);
            }
            
            try {
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            } catch (MySqlException e) {
                const int SQL_TABLE_DOESNT_EXIST = 1146;
                if (e.Number == SQL_TABLE_DOESNT_EXIST) {
                    DatabaseService.Instance.CreateTable<T>();
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                } else 
                    throw e;
            }
        }

        public void Delete<T>(T item) {
            MySqlCommand cmd = connection.CreateCommand();
            DBColumnAttribute[] dbColumnAttributes = DatabaseService.GetFields<T>();
            DBColumnAttribute primaryKey = dbColumnAttributes.First(x => x.primaryKey);
            cmd.CommandText = $"DELETE FROM {DatabaseService.GetTableName<T>()} WHERE {primaryKey.name}=@{primaryKey.name}";
            object value = primaryKey.attachedField.GetValue(item);
            cmd.Parameters.AddWithValue("@" + primaryKey.name, value);
            
            try {
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            } catch (MySqlException e) {
                const int SQL_TABLE_DOESNT_EXIST = 1146;
                if (e.Number == SQL_TABLE_DOESNT_EXIST) {
                    DatabaseService.Instance.CreateTable<T>();
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                } else 
                    throw e;
            }
        }

        public IQueryable<T> Query<T>(DatabaseFacade<T> facade) where T : class, new() {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {DatabaseService.GetTableName<T>()};";

            MySqlDataReader reader;
            try {
                cmd.Prepare();
                reader = cmd.ExecuteReader();
            } catch (MySqlException e) {
                const int SQL_TABLE_DOESNT_EXIST = 1146;
                if (e.Number == SQL_TABLE_DOESNT_EXIST) {
                    DatabaseService.Instance.CreateTable<T>();
                    cmd.Prepare();
                    reader = cmd.ExecuteReader();
                } else 
                    throw e;
            }
            
            List<T> l = new();

            DBColumnAttribute[] fields = DatabaseService.GetFields<T>();

            while (reader.Read()) {
                T t = facade.createNew();
                for (int i = 0; i < fields.Length; i++) {
                    DBColumnAttribute field = fields[i];
                    if(field.primaryKey) continue;
                    object value = reader.GetValue(i);
                    field.attachedField.SetValue(t, value);
                }
                l.Add(t);
            }
            
            reader.Close();

            return l.AsQueryable();
        }

        public T Select<T>(DatabaseFacade<T> facade, object primaryKeyValue) where T : class, new() {
            MySqlCommand cmd = connection.CreateCommand();
            DBColumnAttribute[] dbColumnAttributes = DatabaseService.GetFields<T>();
            DBColumnAttribute primaryKey = dbColumnAttributes.First(x => x.primaryKey);
            
            cmd.CommandText = $"SELECT * FROM {DatabaseService.GetTableName<T>()} WHERE {primaryKey.name}=@primary_key;";
            cmd.Parameters.AddWithValue("@table", DatabaseService.GetTableName<T>());
            cmd.Parameters.AddWithValue("@primary_key", primaryKeyValue);
            
            MySqlDataReader reader;
            try {
                cmd.Prepare();
                reader = cmd.ExecuteReader();
            } catch (MySqlException e) {
                const int SQL_TABLE_DOESNT_EXIST = 1146;
                if (e.Number == SQL_TABLE_DOESNT_EXIST) {
                    DatabaseService.Instance.CreateTable<T>();
                    cmd.Prepare();
                    reader = cmd.ExecuteReader();
                } else 
                    throw e;
            }
            
            DBColumnAttribute[] fields = DatabaseService.GetFields<T>();

            if (!reader.Read()) {
                reader.Close();
                return null;
            }
            
            T t = facade.createNew();
            for (int i = 0; i < fields.Length; i++) {
                DBColumnAttribute field = fields[i];
                if(field.primaryKey) continue;
                object value = reader.GetValue(i);
                field.attachedField.SetValue(t, value);
            }

            reader.Close();
            
            return t;

        }
    }
}