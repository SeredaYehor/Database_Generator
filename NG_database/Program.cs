using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace NG_database
{
    class Program
    {
        private struct Data //Struct that represents columns in db
        {
           public string atribute; //Name of column
           public string data_type; //Type of column
        }

        public static MySqlConnection GetDBConnection(string username, string password)
        {
            string host = "127.0.0.1"; //ip of your server
            int port = 3306; 
            string database = "mydb"; //name of db

            String connection = "Server=" + host + ";Database=" + database +    //Maybe I should change it to common string object
                ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connection); //Creating object for connecting to database
            return conn;
        }

        public static bool Connecting(MySqlConnection conn)
        {
            bool result = true;
            try
            {
                conn.Open();
            }
            catch (Exception except)
            {
                Console.WriteLine("Error: " + except);
                Console.WriteLine(except.StackTrace);
                result = false;
                CloseConnection(conn);
            }
            return result;
        }

        private static void CloseConnection(MySqlConnection conn)
        {
            conn.Close();
            conn.Dispose(); //free all resources, that was used by connection to db
            conn = null;
        }
        static void Main(string[] args)
        {
            string name, password;
            Console.WriteLine("Enter name");
            name = Console.ReadLine(); //Login of your user
            Console.WriteLine("Enter password"); //Password of your user
            password = Console.ReadLine(); //Authentification

            string[] mobile = { "12345" };      //Example arrays
            string[] email = { "Hungry_and_humble@gmail.com" };
            string[] details = { "Bad boys" };

            MySqlConnection conn = GetDBConnection(name, password);

            if(Connecting(conn))
            {
                ShowTables(conn);
                Generator(conn, "customer", mobile, email, details); //Insert data, that gets from arrays

                Console.WriteLine(GetPriKey(conn, "feautures_on_cars_for_sale")); //Function of getting public keys of table
                Console.WriteLine(GetPriKey(conn, "car_sold"));
            }
            CloseConnection(conn);
        }

        private static string GetPriKey(MySqlConnection conn, string table_name)
        {
            string sql = " SHOW KEYS FROM  " + table_name + " WHERE Key_name = \'PRIMARY\'" +
                " AND Column_name NOT IN (SELECT COLUMN_NAME FROM " +
                "INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE " +
                "REFERENCED_TABLE_SCHEMA = 'mydb' AND TABLE_NAME = \'" + table_name + "\')";

            string pri_key;

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn; //Use connection to db
            cmd.CommandText = sql; //Set sql query for execution
            DbDataReader reader = cmd.ExecuteReader(); //Execute query

            if(reader.Read()) //Read output
            {
                pri_key = reader.GetString(4); //Get name of public key
            }    
            else
            {
                pri_key = "none";
            }
            reader.Close();
            return pri_key;
        }

        private static List<Data> GetColumns(MySqlConnection conn, string table_name)
        {
            List<Data> result = new List<Data>(); //List for storaging info about columns
            string sql = "show columns in " + table_name;
            string pri_key_column = GetPriKey(conn, table_name);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;

            DbDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) //While output isn't over
            {
                Data column = new Data();
                if(reader.GetString(0) != pri_key_column) //If column isn't public key
                {
                    column.atribute = reader.GetString(0); //Get Name of column
                    column.data_type = reader.GetString(1); //Get type of column
                    result.Add(column); 
                }
            }
            reader.Close();
            return result;
        }

        private static MySqlDbType GetType(string attribute_type) //Function which indicates which type is use to insert data
        {
            MySqlDbType result = new MySqlDbType();
            MySqlDbType[] types = { MySqlDbType.VarChar, MySqlDbType.UInt32, //Data type
                                    MySqlDbType.Double, MySqlDbType.Datetime, 
                                    MySqlDbType.Decimal, MySqlDbType.Date};

            string[] name_types = {"varchar", "int unsigned", "double", //Name of data
                                  "datetime", "decimal", "date"};
            
            //Two arrays should replace with list of these objects, but i'm lazy ass:3

            for(int i = 0; i < types.Length; i++)
            {
                if(attribute_type.Contains(name_types[i])) //for example: if 'varchar(255)' contains varchar, then type was identified
                {
                    result = types[i];
                    break;
                }
            }
            return result;
        }

        private static void Generator(MySqlConnection conn, string table_name, params string[][] values) 
        {
            /* Main function
             * Input parameters is:
             * - db connection;
             * - name of table;
             * - multiple arrays with data;
             * It can get any amount of arrays!*/

            List<Data> columns = new List<Data>();
            columns = GetColumns(conn, table_name); //Get all columns , in which add data from params

            string sql = MakeQuery(table_name, columns); //Create insert query

            Console.WriteLine("Generating rows for " + table_name + " table.");
            try
            {
                for(int i = 0; i < values[0].Length; i++)
                {
                    MySqlCommand cmd = conn.CreateCommand(); 
                    cmd.CommandText = sql;
                    for (int j = 0; j < columns.Count; j++)
                    {
                        if(values[j][i] != "null") //If value from params not null
                        {
                            cmd.Parameters.Add("@" + columns[j].atribute,           //Insert data with specified type
                                GetType(columns[j].data_type)).Value = values[j][i];
                        }
                    }
                    cmd.ExecuteNonQuery(); //Execute command
                }
            }
            catch (Exception except)
            {
                Console.WriteLine("Error: " + except);
                Console.WriteLine(except.StackTrace);
                CloseConnection(conn);
            }
        }

        private static string MakeQuery(string table_name, List<Data> columns) 
        {
            /* Funct of creating insert query for adding data to db
             * Input data:
             * - name of table;
             * - not public key columns;*/

            string sql = "insert into " + table_name + "(";
            sql = AddAtributes(sql, false, columns); //Indentifying all columns
            sql += ") values (";
            sql = AddAtributes(sql, true, columns); //Setting variables for adding data\
            sql += ')';
            return sql;
        }


        private static string AddAtributes(string sql, bool values_mode, List<Data> columns)  //Funct of creating insert query
        {
            //If values_mode == true, then it will append query with variables (.ex @name_of_student)
            foreach (Data column in columns)
            {
                if(values_mode)
                {
                    sql += "@";
                }
                sql += column.atribute + ", ";
            }
            sql = sql.Remove(sql.Length - 2); //Remove last space and ',' symbols
            return sql;
        }

        private static void ShowTables(MySqlConnection conn) //Funct of showing tables
        {
            string sql = "show tables";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;

            DbDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                Console.WriteLine(reader.GetString(0)); //Write name of table
            }
            reader.Close();
        }
    }
}
