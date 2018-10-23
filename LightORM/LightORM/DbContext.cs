using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM
{
  public class DbContext
  {
    private static string ConnectionString = "";
    private static string Server = "";
    private static string Port = "";
    private static string Database = "";
    private static string Uid = "";
    private static string Pwd = "";

    private static string TestServer = "";
    private static string TestPort = "";
    private static string TestDatabase = "";
    private static string TestUid = "";
    private static string TestPwd = "";

    static DbContext()
    {
      ConnectionString = System.Diagnostics.Debugger.IsAttached ?
        "Server=" + TestServer + ";Database=" + TestDatabase + ";Port=" + TestPort + ";Uid=" + TestUid + ";Pwd=" + TestPwd + ";SslMode=none;" :
        "Server=" + Server + ";Database=" + Database + ";Port=" + Port + ";Uid=" + Uid + ";Pwd=" + Pwd + ";SslMode=none;";
    }
    public IEnumerable<T> SelectQuery<T>(string query)
    {
      return SelectQuery<T>(query, new { });
    }
    public IEnumerable<T> SelectQuery<T>(string query, object parameters)
    {
      var genericObj = typeof(T);
      var list = new List<T>();
      var con = new MySqlConnection(ConnectionString);
      MySqlDataReader reader = null;
      try
      {
        var myProperties = new List<PropertyInfo>(parameters.GetType().GetProperties());
        using (var cmd = new MySqlCommand())
        {
          cmd.CommandText = query;
          cmd.Connection = con;
          foreach (PropertyInfo property in myProperties)
          {
            if (property.GetValue(parameters, null) != null)
            {
              if (query.Contains("@" + property.Name))
              {
                cmd.Parameters.Add(new MySqlParameter("@" + property.Name, property.GetValue(parameters, null)));
              }
              else
              {
                Console.WriteLine("Property : " + property.Name + " not defined!");
              }
            }
            else
            {
              Console.WriteLine("Property : " + property.Name + " is empty!");
            }
          }
          con.Open();
          reader = cmd.ExecuteReader();
          IList<PropertyInfo> genericProperties = new List<PropertyInfo>(genericObj.GetProperties());
          while (reader.Read())
          {
            var newObj = (T)Activator.CreateInstance(typeof(T));
            for (int i = 0; i < reader.FieldCount; i++)
            {
              try
              {
                genericProperties.FirstOrDefault(x => x.Name.ToLower().Replace("ı", "i") == reader.GetName(i).ToLower().Replace("ı", "i")).SetValue(newObj, reader[i] == DBNull.Value ? null : reader[i], null);
              }
              catch (Exception ex)
              {
                Console.WriteLine("Please check : " + reader.GetName(i) + ", Class : " + genericObj.Name);
                Console.WriteLine(ex);
              }
            }
            list.Add(newObj);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        Console.WriteLine(ex.InnerException);
      }
      finally
      {
        if (reader != null)
          reader.Close();

        if (con.State == System.Data.ConnectionState.Open)
          con.Dispose();
      }
      return list;
    }
    public IEnumerable<dynamic> SelectQuery(string query)
    {
      return SelectQuery(query, new { });
    }
    public IEnumerable<dynamic> SelectQuery(string query, object parameters)
    {
      var list = new List<dynamic>();
      var con = new MySqlConnection(ConnectionString);
      MySqlDataReader reader = null;
      try
      {
        var myProperties = new List<PropertyInfo>(parameters.GetType().GetProperties());
        using (var cmd = new MySqlCommand())
        {
          cmd.CommandText = query;
          cmd.Connection = con;
          foreach (PropertyInfo item in myProperties)
          {
            if (item.GetValue(parameters, null) != null)
            {
              if (query.Contains("@" + item.Name))
              {
                cmd.Parameters.Add(new MySqlParameter("@" + item.Name, item.GetValue(parameters, null)));
              }
              else
              {
                Console.WriteLine("Property : " + item.Name + " is not defined!");
              }
            }
            else
            {
              Console.WriteLine("Property : " + item.Name + " is empty!");
            }
          }
          con.Open();
          reader = cmd.ExecuteReader();
          while (reader.Read())
          {
            dynamic newObj = new System.Dynamic.ExpandoObject();
            var dictionary = (IDictionary<string, object>)newObj;
            for (int i = 0; i < reader.FieldCount; i++)
            {
              dictionary.Add(reader.GetName(i), reader[i] == DBNull.Value ? null : reader[i]);
            }
            list.Add(dictionary);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      finally
      {
        if (reader != null)
          reader.Close();

        if (con.State == System.Data.ConnectionState.Open)
          con.Dispose();
      }
      return list;
    }
    // Begin INSERT      
    public bool InsertData(object data)
    {
      return InsertData(data, "", null);
    }
    public bool InsertDataIgnore(object data)
    {
      return InsertData(data, "IGNORE", null);
    }
    public bool InsertDataOnDuplicate(object data, string[] keys)
    {
      return InsertData(data, "", keys);
    }
    private bool InsertData(object data, string ignore, string[] keys)
    {
      var con = new MySqlConnection(ConnectionString);
      using (var cmd = new MySqlCommand())
      {
        string table_name = data.GetType().Name.ToLower().Replace("ı", "i");
        var prop = new List<PropertyInfo>(data.GetType().GetProperties());
        string query = "INSERT " + ignore + " INTO ";
        query = query + table_name + " (";
        string subQuery = "VALUES (";
        foreach (var item in prop)
        {
          query += item.Name.ToLower().Replace("ı", "i").Replace("ü", "u") + ",";
          subQuery += "@" + item.Name + ",";
          cmd.Parameters.Add(new MySqlParameter("@" + item.Name, item.GetValue(data, null)));
        }
        query = query.TrimEnd(',');
        subQuery = subQuery.TrimEnd(',');
        query += ") ";
        subQuery += ")";
        if (keys != null)
        {
          subQuery += " ON DUPLICATE KEY UPDATE ";
          foreach (var key in keys)
          {
            subQuery += key + "=VALUES(" + key + "),";
          }
        }
        subQuery = subQuery.TrimEnd(',');
        query += subQuery;
        try
        {
          con.Open();
          cmd.Connection = con;
          cmd.CommandText = query;
          cmd.ExecuteNonQuery();
          con.Dispose();
          return true;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
        finally
        {
          if (con.State == System.Data.ConnectionState.Open)
          {
            con.Dispose();
          }
        }
      }
      return false;
    }
    // End INSERT
    // Begin Insert Range
    public bool InsertDataRange(IEnumerable<object> dataList)
    {
      return InsertDataRange(dataList, "", null);
    }
    public bool InsertDataIgnoreRange(IEnumerable<object> dataList)
    {
      return InsertDataRange(dataList, "IGNORE", null);
    }
    public bool InsertDataOnDuplicateRange(IEnumerable<object> dataList, string[] keys)
    {
      return InsertDataRange(dataList, "", keys);
    }
    private bool InsertDataRange(IEnumerable<object> dataList, string ignore, string[] keys)
    {
      var con = new MySqlConnection(ConnectionString);
      var parameter = new MySqlParameter();
      using (var cmd = new MySqlCommand())
      {
        string tableName = dataList.FirstOrDefault().GetType().Name.ToLower();
        var prop = new List<PropertyInfo>(dataList.FirstOrDefault().GetType().GetProperties());
        string query = "INSERT " + ignore + " INTO ";
        query += tableName + " (";
        string subQuery = " VALUES ";

        foreach (var item in prop)
        {
          query += item.Name.ToLower().Replace("ı", "i").Replace("ü", "u") + ",";
        }
        query = query.TrimEnd(',') + ")";

        int i = 0;
        foreach (var data in dataList)
        {
          ++i;
          subQuery += "(";

          foreach (var item in prop)
          {
            cmd.Parameters.Add(new MySqlParameter("@" + item.Name + "" + i, item.GetValue(data, null)));
            subQuery += "@" + item.Name + "{0},";
          }
          subQuery = subQuery.TrimEnd(',');
          subQuery += ")";
          subQuery = string.Format(subQuery, i);
          subQuery += ",";
        }
        subQuery = subQuery.TrimEnd(',');

        if (keys != null)
        {
          subQuery += " ON DUPLICATE KEY UPDATE ";
          foreach (var key in keys)
          {
            subQuery += key + "=VALUES(" + key + "),";
          }
        }
        subQuery = subQuery.TrimEnd(',');
        try
        {
          con.Open();
          query = query + subQuery;
          cmd.Connection = con;
          cmd.CommandText = query;
          cmd.ExecuteNonQuery();
          con.Dispose();
          return true;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
        finally
        {
          if (con.State == System.Data.ConnectionState.Open)
          {
            con.Dispose();
          }
        }
      }
      return false;
    }
    // End Insert Range 
    public bool ExecuteQuery(string query, object parameters)
    {
      using (var con = new MySqlConnection(ConnectionString))
      {
        using (var cmd = new MySqlCommand() { Connection = con, CommandText = query, CommandType = CommandType.Text })
        {
          try
          {
            IList<PropertyInfo> properties = new List<PropertyInfo>(parameters.GetType().GetProperties());
            foreach (PropertyInfo item in properties)
            {
              cmd.Parameters.AddWithValue("@" + item.Name, item.GetValue(parameters, null));
            }
            con.Open();
            cmd.ExecuteNonQuery();
            return true;
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
          }
          finally
          {
            if (con.State == ConnectionState.Open)
            {
              con.Dispose();
            }
          }
        }
      }
      return false;
    }
  }
}

