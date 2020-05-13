using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Collections;
using Microsoft.Extensions.Configuration;


public class DatabaseHelper : IDisposable
{
    private string strConnectionString;
    private DbConnection objConnection;
    private DbCommand objCommand;
    private DbDataAdapter objAdapter;
    private DbProviderFactory objFactory = null;
    private bool boolHandleErrors;
    private string strLastError;
    private bool boolLogError;
    private string strLogFile;
    public static string cn;
    public IConfigurationRoot GetConfiguration()
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        return builder.Build();
    }
    public DatabaseHelper()
    {
        var configuation = GetConfiguration();

        string strConnect = configuation.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value.ToString();
        strConnectionString = strConnect;
        //connnect string
        this.connectDB(strConnect, Providers.SqlServer);
    }
    public string Encrypt_SQL(string original)
    {
        return Encrypt_SQL(original, "INSP!@#456&*(987^%$321");//"!@#$%^&*()~_+|");
    }
    public string Encrypt_SQL(string original, string key)
    {
        TripleDESCryptoServiceProvider objDESProvider;
        MD5CryptoServiceProvider objHashMD5Provider;
        byte[] keyhash;
        byte[] buffer;
        try
        {
            objHashMD5Provider = new MD5CryptoServiceProvider();
            keyhash = objHashMD5Provider.ComputeHash(UnicodeEncoding.Unicode.GetBytes(key));
            objHashMD5Provider = null;
            objDESProvider = new TripleDESCryptoServiceProvider();
            objDESProvider.Key = keyhash;
            objDESProvider.Mode = CipherMode.ECB;
            buffer = UnicodeEncoding.Unicode.GetBytes(original);
            return Convert.ToBase64String(objDESProvider.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        catch
        {
            return string.Empty;
        }
    }
    public string Decrypt_SQL(string encrypted)
    {
        return Decrypt_SQL(encrypted, "INSP!@#456&*(987^%$321");
    }
    public string Decrypt_SQL(string encrypted, string key)
    {
        TripleDESCryptoServiceProvider objDESProvider;
        MD5CryptoServiceProvider objHashMD5Provider;
        byte[] keyhash;
        byte[] buffer;

        try
        {
            objHashMD5Provider = new MD5CryptoServiceProvider();
            keyhash = objHashMD5Provider.ComputeHash(UnicodeEncoding.Unicode.GetBytes(key));
            objHashMD5Provider = null;

            objDESProvider = new TripleDESCryptoServiceProvider();
            objDESProvider.Key = keyhash;
            objDESProvider.Mode = CipherMode.ECB;

            buffer = Convert.FromBase64String(encrypted);
            return UnicodeEncoding.Unicode.GetString(objDESProvider.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        catch
        {
            return string.Empty;
        }
    }
    public string con()
    {
        return cn;
    }
    public DatabaseHelper(string connectionstring, Providers provider)
    {
        this.connectDB(connectionstring, provider);
    }

    public DataRow GetDataRow(string pCommandText)
    {
        try
        {
            DataTable dt = new DataTable();
            DataRow mRow;
            dt = ExecuteDataTable(pCommandText);
            //dt = this.
            if (dt.Rows.Count != 0)
            {
                mRow = dt.Rows[0];
            }
            else
            {
                mRow = null;
            }
            dt.Dispose();
            return mRow;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// tra ve gia tri string cua fieldname duoc dua vao, neu khong co se tra ve chuoi rong			
    /// </summary>
    /// <param name="pSQLCommand">Command string</param>
    /// <param name="pFieldReturn">Ten cot lay gia tri traa ve</param>
    /// <returns>tra ve gia tri string cua fieldname duoc dua vao, neu khong co se tra ve chuoi rong</returns>

    public string LookUpTable(string pSQLCommand, string pFieldReturn)
    {
        try
        {
            DataRow dtRow;
            dtRow = GetDataRow(pSQLCommand);
            if (dtRow != null)
            {
                return Convert.ToString(dtRow[pFieldReturn]).Trim();
            }
            return "";
        }
        catch (SqlException Ex)
        {
            return "";
        }
    }


    private void connectDB(string connectionstring, Providers provider)
    {
        try
        {
            strConnectionString = connectionstring;
            switch (provider)
            {
                case Providers.SqlServer:
                    objFactory = SqlClientFactory.Instance;
                    break;
                case Providers.ConfigDefined:
                    string providername = connectionstring;
                    switch (providername)
                    {
                        case "System.Data.SqlClient":
                            objFactory = SqlClientFactory.Instance;
                            break;

                        default:
                            objFactory = SqlClientFactory.Instance;
                            break;
                    }
                    break;

            }
            objConnection = objFactory.CreateConnection();
            objCommand = objFactory.CreateCommand();
            //Chinh sua tang thoi gian xuat excel khi goi store len 20 phut
            objCommand.CommandTimeout = 1200;
            objConnection.ConnectionString = strConnectionString;
            objCommand.Connection = objConnection;
            objCommand.Parameters.Clear();
        }
        catch (Exception ex)
        {
        }
    }

    public DatabaseHelper(Providers provider)
        : this("")
    {
    }

    public DatabaseHelper(string connectionstring)
        : this(connectionstring, Providers.SqlServer)
    {
    }
    /*
    public DatabaseHelper()
        : this(ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString, Providers.ConfigDefined)
    {
    }
    */
    public bool HandleErrors
    {
        get
        {
            return boolHandleErrors;
        }
        set
        {
            boolHandleErrors = value;
        }
    }

    public string LastError
    {
        get
        {
            return strLastError;
        }
    }

    public bool LogErrors
    {
        get
        {
            return boolLogError;
        }
        set
        {
            boolLogError = value;
        }
    }

    public string LogFile
    {
        get
        {
            return strLogFile;
        }
        set
        {
            strLogFile = value;
        }
    }

    public int AddParameter(string name, object value)
    {
        DbParameter p = objFactory.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        return objCommand.Parameters.Add(p);
    }

    public int AddParameter(string name, DbType paraType, object value)
    {
        DbParameter p = objFactory.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        p.DbType = paraType;
        return objCommand.Parameters.Add(p);
    }

    public int AddParameter(string name, DbType paraType, int Size, object value)
    {
        DbParameter p = objFactory.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        p.DbType = paraType;
        p.Size = Size;
        return objCommand.Parameters.Add(p);
    }

    public int AddParameter(DbParameter parameter)
    {
        if (objCommand != null)
            return objCommand.Parameters.Add(parameter);
        else
            return -1;


    }

    public void ClearParameter()
    {
        if (objCommand != null)
        {
            objCommand.Parameters.Clear();
        }
    }

    public Object GetParameterValue(String pName)
    {
        if (objCommand != null)
            return objCommand.Parameters[pName].Value;
        else
            return null;
    }

    public DbCommand Command
    {
        get
        {
            return objCommand;
        }
    }

    public DbParameter Parameter
    {
        get
        {
            return objFactory.CreateParameter();
        }
    }

    public void BeginTransaction()
    {
        if (objConnection.State == System.Data.ConnectionState.Closed)
        {
            objConnection.Open();
        }
        objCommand.Transaction = objConnection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        objCommand.Transaction.Commit();
        objConnection.Close();
    }

    public void RollbackTransaction()
    {
        objCommand.Transaction.Rollback();
        objConnection.Close();
    }

    public int ExecuteNonQuery(string query)
    {
        return ExecuteNonQuery(query, CommandType.Text, ConnectionState.CloseOnExit);
    }

    public int ExecuteNonQuery(string query, CommandType commandtype)
    {
        return ExecuteNonQuery(query, commandtype, ConnectionState.CloseOnExit);
    }

    public int ExecuteNonQuery(string query, ConnectionState connectionstate)
    {
        return ExecuteNonQuery(query, CommandType.Text, connectionstate);
    }

    public int ExecuteNonQuery(string query, CommandType commandtype, ConnectionState connectionstate)
    {
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;
        int i = -1;
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {

                objConnection.Open();
            }
            i = objCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {

            if (connectionstate == ConnectionState.CloseOnExit)
            {
                objConnection.Close();
            }
        }

        return i;
    }

    public object ExecuteNonQuery(string query, CommandType commandtype, ConnectionState connectionstate, String outParameter)
    {
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;
        object i = null;
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }
            objCommand.ExecuteNonQuery();
            i = objCommand.Parameters[outParameter].Value;
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            objCommand.Parameters.Clear();
            if (connectionstate == ConnectionState.CloseOnExit)
            {
                objConnection.Close();
            }
        }

        return i;
    }

    public object ExecuteScalar(string query)
    {
        return ExecuteScalar(query, CommandType.Text, ConnectionState.CloseOnExit);
    }

    public object ExecuteScalar(string query, CommandType commandtype)
    {
        return ExecuteScalar(query, commandtype, ConnectionState.CloseOnExit);
    }

    public object ExecuteScalar(string query, ConnectionState connectionstate)
    {
        return ExecuteScalar(query, CommandType.Text, connectionstate);
    }

    public object ExecuteScalar(string query, CommandType commandtype, ConnectionState connectionstate)
    {
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;
        object o = null;
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }
            o = objCommand.ExecuteScalar();
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            objCommand.Parameters.Clear();
            if (connectionstate == ConnectionState.CloseOnExit)
            {
                objConnection.Close();
            }
        }

        return o;
    }

    public DbDataReader ExecuteReader(string query)
    {
        return ExecuteReader(query, CommandType.Text, ConnectionState.CloseOnExit);
    }

    public DbDataReader ExecuteReader(string query, CommandType commandtype)
    {
        return ExecuteReader(query, commandtype, ConnectionState.CloseOnExit);
    }

    public DbDataReader ExecuteReader(string query, ConnectionState connectionstate)
    {
        return ExecuteReader(query, CommandType.Text, connectionstate);
    }

    public DbDataReader ExecuteReader(string query, CommandType commandtype, ConnectionState connectionstate)
    {
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;
        DbDataReader reader = null;
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }
            if (connectionstate == ConnectionState.CloseOnExit)
            {
                reader = objCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            else
            {
                reader = objCommand.ExecuteReader();
            }

        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            objCommand.Parameters.Clear();
        }

        return reader;
    }

    public DataTable ExecuteDataTable(string query)
    {
        return ExecuteDataTable(query, CommandType.Text, ConnectionState.CloseOnExit);
    }

    public DataTable ExecuteDataTable(string query, CommandType commandtype)
    {
        return ExecuteDataTable(query, commandtype, ConnectionState.CloseOnExit);
    }

    public DataTable ExecuteDataTable(string query, CommandType commandtype, ConnectionState connectionstate)
    {
        DbDataAdapter adapter = objFactory.CreateDataAdapter();
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;

        adapter.SelectCommand = objCommand;
        DataTable dt = new DataTable();
        try
        {
            adapter.Fill(dt);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            objCommand.Parameters.Clear();
            if (connectionstate == ConnectionState.CloseOnExit)
            {
                if (objConnection.State == System.Data.ConnectionState.Open)
                {
                    objConnection.Close();
                }
            }
        }
        return dt;
    }


    public DataSet ExecuteDataSet(string query)
    {
        return ExecuteDataSet(query, CommandType.Text, ConnectionState.CloseOnExit);
    }

    public DataSet ExecuteDataSet(string query, CommandType commandtype)
    {
        return ExecuteDataSet(query, commandtype, ConnectionState.CloseOnExit);
    }

    public DataSet ExecuteDataSet(string query, ConnectionState connectionstate)
    {
        return ExecuteDataSet(query, CommandType.Text, connectionstate);
    }

    public DataSet ExecuteDataSet(string query, CommandType commandtype, ConnectionState connectionstate)
    {
        DbDataAdapter adapter = objFactory.CreateDataAdapter();
        objCommand.CommandText = query;
        objCommand.CommandType = commandtype;
        adapter.SelectCommand = objCommand;
        DataSet ds = new DataSet();
        try
        {
            adapter.Fill(ds);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            objCommand.Parameters.Clear();
            if (connectionstate == ConnectionState.CloseOnExit)
            {
                if (objConnection.State == System.Data.ConnectionState.Open)
                {
                    objConnection.Close();
                }
            }
        }
        return ds;
    }

    public void HandleExceptions(Exception ex)
    {
        if (LogErrors)
        {
            ///ghi loi vao log
            ///
        }
        if (HandleErrors)
        {
            strLastError = ex.Message;
        }
        else
        {
            throw ex;
        }
    }

    public void WriteToLog(string msg)
    {
        StreamWriter writer = File.AppendText(LogFile);
        writer.WriteLine(DateTime.Now.ToString() + " - " + msg);
        writer.Close();
    }

    public void Dispose()
    {
        objConnection.Close();
        objConnection.Dispose();
        objCommand.Dispose();
    }

    public DataSet GetSchema(DatabaseObjects type)
    {
        DataSet ds = new DataSet();
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }

            ds.Tables.Add(objConnection.GetSchema(type.ToString()));
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            if (objConnection.State == System.Data.ConnectionState.Open)
            {
                objConnection.Close();
            }
        }

        return ds;
    }

    public DataSet GetTables()
    {
        DataSet ds = new DataSet();
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }

            ds.Tables.Add(objConnection.GetSchema(DatabaseObjects.Tables.ToString(), new string[] { null, null, null, "TABLE" }));
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            if (objConnection.State == System.Data.ConnectionState.Open)
            {
                objConnection.Close();
            }
        }

        return ds;
    }



    public DataSet GetColumns(string tableName)
    {
        DataSet ds = new DataSet();
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }

            ds.Tables.Add(objConnection.GetSchema(DatabaseObjects.Columns.ToString(), new string[] { null, null, tableName, null }));
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            if (objConnection.State == System.Data.ConnectionState.Open)
            {
                objConnection.Close();
            }
        }

        return ds;
    }
    public DbType ConvertDataType(string strTypeName)
    {
        DbType TypeReturn = new DbType();
        switch (strTypeName.Trim().ToUpper())
        {
            case "NVARCHAR":
                TypeReturn = DbType.String;
                break;
            case "DATETIME":
                TypeReturn = DbType.String;
                break;
            case "VARCHAR":
                TypeReturn = DbType.String;
                break;
            case "BIT":
                TypeReturn = DbType.Boolean;
                break;
            case "INT":
                TypeReturn = DbType.Int32;
                break;
            case "MONEY":
                TypeReturn = DbType.Currency;
                break;
            case "REAL":
                TypeReturn = DbType.Double;
                break;
            case "NUMERIC":
                TypeReturn = DbType.Decimal;
                break;
            case "SMALLINT":
                TypeReturn = DbType.Int16;
                break;
            case "TINYINT":
                TypeReturn = DbType.Byte;
                break;
            case "FLOAT":
                TypeReturn = DbType.Decimal;
                break;
            default:
                TypeReturn = DbType.String;
                break;
        }
        return TypeReturn;
    }

    public bool DeleteById(int value, string strTableName, bool isDelete)
    {
        bool result = false;
        this.BeginTransaction();
        try
        {
            string Query = "";
            if (isDelete == true)
            {
                Query = "DELETE FROM [" + strTableName + "] WHERE Id=" + value;
            }
            else
            {
                Query = "UPDATE [" + strTableName + "] SET Status = 0, UpdateDate = GetDate() WHERE Id=" + value;
            }
            //this.AddParameter("Id", DbType.Int32, Id);
            //this.AddParameter("TableName", DbType.String, tableName);
            int iRecord = this.ExecuteNonQuery(Query, ConnectionState.KeepOpen);
            result = iRecord == 1 ? true : false;
        }
        catch (Exception)
        {
            this.RollbackTransaction();
            return result = false;
        }
        this.CommitTransaction();
        return result;
        /*Ex
            DatabaseHelper DB = new DatabaseHelper();
           bool result = DB.DeleteById(2, "Test");
           Utils.ShowMessageBox(this.Page, "ket qua" + result);
         * */
    }
    /*public bool SelectById(int value, string strTableName)
    {
        bool result = false;
        this.BeginTransaction();
        try
        {
            string Query = string.Empty;
            Query = "SELECT FROM [" + strTableName + "] WHERE Id=" + value;
        }
        catch (Exception)
        {
            this.RollbackTransaction();
            return result = false;
        }
        this.CommitTransaction();
        return result;

    }*/

    public bool Insert(Hashtable hashValue, string strTableName)
    {
        bool result = false;
        DataTable dtObj = GetColumnsToDataTable(strTableName);
        this.BeginTransaction();
        try
        {
            int len = dtObj.Rows.Count;
            string Query = "INSERT INTO [" + strTableName + "](";
            string listColumnName = "";
            string listValue = "";
            for (int i = 0; i < len; i++)
            {
                string columnNameDB = dtObj.Rows[i]["COLUMN_NAME"].ToString();
                Object value = null;

                value = hashValue[columnNameDB];
                if (columnNameDB == "Order")
                {
                    columnNameDB = "[" + columnNameDB + "]";
                }
                if (value != null)
                {
                    DbType type = ConvertDataType(dtObj.Rows[i]["DATA_TYPE"].ToString());
                    if (type == DbType.Boolean)
                    {
                        value = (value.ToString() == "True" || value.ToString() == "1") ? 1 : 0;
                    }
                    else if (type == DbType.String)
                    {
                        value = "N'" + value.ToString() + "'";
                    }

                    listColumnName += (listColumnName != "") ? ("," + columnNameDB) : columnNameDB;
                    listValue += (listValue != "") ? ("," + value) : value;
                }
            }
            Query += listColumnName + ") VALUES(" + listValue + ");";

            int iRecord = this.ExecuteNonQuery(Query, ConnectionState.KeepOpen);
            result = iRecord == 1 ? true : false;

        }
        catch (Exception)
        {
            this.RollbackTransaction();
            return result = false;
        }
        this.CommitTransaction();
        return result;
        /*Ex
            DatabaseHelper DB = new DatabaseHelper();
           Hashtable hash = new Hashtable();
           hash.Add("Name","honglk");
           hash.Add("BirthDay",Utils.ConvertDMYtoMMDDYYYY("20/01/1985"));
           hash.Add("Status",1);
           bool result = DB.Insert(hash, "Test");
         * */
    }


    public bool Update(Hashtable hashValueUpdate, string keyField, Object keyValue, string strTableName)
    {
        bool result = false;
        DataTable dtObj = GetColumnsToDataTable(strTableName);
        this.BeginTransaction();
        try
        {
            int len = dtObj.Rows.Count;
            string Query = "UPDATE [" + strTableName + "] SET ";
            string listCond = "";
            for (int i = 0; i < len; i++)
            {
                string columnNameDB = dtObj.Rows[i]["COLUMN_NAME"].ToString();
                if (columnNameDB.Equals(keyField))
                {
                    if (ConvertDataType(dtObj.Rows[i]["DATA_TYPE"].ToString()) == DbType.String)
                    {
                        keyValue = "'" + keyValue + "'";
                    }
                }
                Object value = null;

                value = hashValueUpdate[columnNameDB];

                if (value != null)
                {
                    DbType type = ConvertDataType(dtObj.Rows[i]["DATA_TYPE"].ToString());
                    if (type == DbType.Boolean)
                    {
                        value = (value.ToString() == "True" || value.ToString() == "1") ? 1 : 0;
                    }
                    else if (type == DbType.String)
                    {
                        value = "N'" + value.ToString() + "'";
                    }
                    if (listCond == "")
                    {
                        listCond += columnNameDB + "=" + value.ToString();
                    }
                    else
                    {
                        listCond += "," + columnNameDB + "=" + value.ToString();
                    }
                }
            }
            Query += listCond + " WHERE " + keyField + "=" + keyValue;
            int iRecord = this.ExecuteNonQuery(Query, ConnectionState.KeepOpen);
            result = iRecord == 1 ? true : false;

        }
        catch (Exception)
        {
            this.RollbackTransaction();
            return result = false;
        }
        this.CommitTransaction();
        return result;
        /*Ex
           DatabaseHelper DB = new DatabaseHelper();
           Hashtable hash = new Hashtable();
           hash.Add("Name","honglk");
           hash.Add("BirthDay",Utils.ConvertDMYtoMMDDYYYY("20/01/1985"));
           hash.Add("Status",1);
           bool result = DB.Insert(hash, "Test");
         * */
    }

    public DataTable GetColumnsToDataTable(string tableName)
    {
        DataTable dt = new DataTable();
        try
        {
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                objConnection.Open();
            }
            dt = objConnection.GetSchema(DatabaseObjects.Columns.ToString(), new string[] { null, null, tableName, null });
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
        finally
        {
            if (objConnection.State == System.Data.ConnectionState.Open)
            {
                objConnection.Close();
            }
        }

        return dt;
    }

    public bool UpdateLocation(Hashtable hashValueUpdate, string keyField, Object keyValue, string strTableName)
    {
        bool result = false;
        DataTable dtObj = GetColumnsToDataTable(strTableName);
        this.BeginTransaction();
        try
        {
            int len = dtObj.Rows.Count;
            string Query = "UPDATE [" + strTableName + "] SET ";
            string listCond = "";
            for (int i = 0; i < len; i++)
            {
                string columnNameDB = dtObj.Rows[i]["COLUMN_NAME"].ToString();
                if (columnNameDB.Equals(keyField))
                {
                    if (ConvertDataType(dtObj.Rows[i]["DATA_TYPE"].ToString()) == DbType.String)
                    {
                        keyValue = "'" + keyValue + "'";
                    }
                }
                Object value = null;

                value = hashValueUpdate[columnNameDB];

                if (columnNameDB == "Order")
                {
                    columnNameDB = "[" + columnNameDB + "]";
                }

                if (value != null)
                {
                    DbType type = ConvertDataType(dtObj.Rows[i]["DATA_TYPE"].ToString());
                    if (type == DbType.Boolean)
                    {
                        value = (value.ToString() == "True" || value.ToString() == "1") ? 1 : 0;
                    }
                    else if (type == DbType.String)
                    {
                        value = "N'" + value.ToString() + "'";
                    }
                    if (listCond == "")
                    {
                        listCond += columnNameDB + "=" + value.ToString();
                    }
                    else
                    {
                        listCond += "," + columnNameDB + "=" + value.ToString();
                    }
                }
            }
            Query += listCond + " WHERE " + keyField + "=" + keyValue;
            int iRecord = this.ExecuteNonQuery(Query, ConnectionState.KeepOpen);
            result = iRecord == 1 ? true : false;

        }
        catch (Exception)
        {
            this.RollbackTransaction();
            return result = false;
        }
        this.CommitTransaction();
        return result;
    }

    #region Encrypt - Decrypt Functions
    public string Encrypt(string original)
    {
        return Encrypt(original, "!@#$%^&*()~_+|");
    }

    public string EncryptIntranet(string original)
    {
        return Encrypt(original, "1qaz2wsx0okm9ijn");
    }

    public string Encrypt(string original, string key)
    {
        TripleDESCryptoServiceProvider objDESProvider;
        MD5CryptoServiceProvider objHashMD5Provider;
        byte[] keyhash;
        byte[] buffer;
        try
        {
            objHashMD5Provider = new MD5CryptoServiceProvider();
            keyhash = objHashMD5Provider.ComputeHash(UnicodeEncoding.Unicode.GetBytes(key));
            objHashMD5Provider = null;

            objDESProvider = new TripleDESCryptoServiceProvider();
            objDESProvider.Key = keyhash;
            objDESProvider.Mode = CipherMode.ECB;

            buffer = UnicodeEncoding.Unicode.GetBytes(original);
            return Convert.ToBase64String(objDESProvider.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        catch
        {
            return string.Empty;
        }
    }

    public string Decrypt(string encrypted)
    {
        return Decrypt(encrypted, "!@#$%^&*()~_+|");
    }

    public string DecryptIntranet(string encrypted)
    {
        return Decrypt(encrypted, "1qaz2wsx0okm9ijn");
    }

    public string Decrypt(string encrypted, string key)
    {
        TripleDESCryptoServiceProvider objDESProvider;
        MD5CryptoServiceProvider objHashMD5Provider;
        byte[] keyhash;
        byte[] buffer;

        try
        {
            objHashMD5Provider = new MD5CryptoServiceProvider();
            keyhash = objHashMD5Provider.ComputeHash(UnicodeEncoding.Unicode.GetBytes(key));
            objHashMD5Provider = null;

            objDESProvider = new TripleDESCryptoServiceProvider();
            objDESProvider.Key = keyhash;
            objDESProvider.Mode = CipherMode.ECB;

            buffer = Convert.FromBase64String(encrypted);
            return UnicodeEncoding.Unicode.GetString(objDESProvider.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
        }
        catch
        {
            return string.Empty;
        }
    }
    #endregion

    #region Import - Export Data
    public void UpdateDatabyAdapter(string selectcommand, DataTable dt)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = "UPDATE IncentivePointLimitLV5 SET ProductHierarchyDensity = @ProductHierarchyDensity WHERE (ID = @ID)";

        DbParameter Param1 = objupdatecmd.CreateParameter();
        Param1.ParameterName = "@ProductHierarchyDensity";
        Param1.DbType = DbType.Double;
        Param1.Value = DataRowVersion.Original;
        Param1.SourceColumn = "ProductHierarchyDensity";

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "@ID";
        Param2.DbType = DbType.Int32;
        Param2.Value = DataRowVersion.Original;
        Param2.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param1);
        objupdatecmd.Parameters.Add(Param2);
        objAdapter.UpdateCommand = objupdatecmd;


        //dt.AcceptChanges();
        //for (int i = 0; i < dt.Rows.Count; i++)
        //{
        //    if (dt.Rows[i]["ID"].ToString() == "")
        //        dt.Rows[i].SetAdded();
        //    //else
        //    //    dt.Rows[i].SetModified();
        //}
        objAdapter.Update(dt);
    }
    public void InsertDatabyAdapter(string selectcommand, DataTable dt)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        objAdapter.Update(dt);
    }
    public void UpdateDatabyAdapter_pointLimitL5(string selectcommand, DataTable dt)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = @"UPDATE IncentivePointLimitLV5 SET CrossProfitRate=@CrossProfitRate,
                        ProductHierarchyDensity = @ProductHierarchyDensity,CrossProfitRatebyDensity=@CrossProfitRatebyDensity,
        ProductHierarchyProfit=@ProductHierarchyProfit,PointLimitbyDensity=@PointLimitbyDensity,DeviationPoints=@DeviationPoints,
        PointLimit=@PointLimit,Remark=@Remark,LastUpdatedBy=@LastUpdatedBy,LastUpdatedDateTime=@LastUpdatedDateTime WHERE (ID = @ID)";

        DbParameter Param1 = objupdatecmd.CreateParameter();
        Param1.ParameterName = "@CrossProfitRate";
        Param1.DbType = DbType.Double;
        Param1.Value = DataRowVersion.Original;
        Param1.SourceColumn = "CrossProfitRate";

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "@ProductHierarchyDensity";
        Param2.DbType = DbType.Double;
        Param2.Value = DataRowVersion.Original;
        Param2.SourceColumn = "ProductHierarchyDensity";

        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "@CrossProfitRatebyDensity";
        Param3.DbType = DbType.Double;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "CrossProfitRatebyDensity";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "@ProductHierarchyProfit";
        Param4.DbType = DbType.Double;
        Param4.Value = DataRowVersion.Original;
        Param4.SourceColumn = "ProductHierarchyProfit";

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "@PointLimitbyDensity";
        Param5.DbType = DbType.Double;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "PointLimitbyDensity";

        DbParameter Param6 = objupdatecmd.CreateParameter();
        Param6.ParameterName = "@DeviationPoints";
        Param6.DbType = DbType.Double;
        Param6.Value = DataRowVersion.Original;
        Param6.SourceColumn = "DeviationPoints";


        DbParameter Param8 = objupdatecmd.CreateParameter();
        Param8.ParameterName = "@PointLimit";
        Param8.DbType = DbType.Double;
        Param8.Value = DataRowVersion.Original;
        Param8.SourceColumn = "PointLimit";

        DbParameter Param9 = objupdatecmd.CreateParameter();
        Param9.ParameterName = "@Remark";
        Param9.DbType = DbType.String;
        Param9.Value = DataRowVersion.Original;
        Param9.SourceColumn = "Remark";

        DbParameter Param10 = objupdatecmd.CreateParameter();
        Param10.ParameterName = "@LastUpdatedBy";
        Param10.DbType = DbType.Int32;
        Param10.Value = DataRowVersion.Original;
        Param1.SourceColumn = "LastUpdatedBy";

        DbParameter Param11 = objupdatecmd.CreateParameter();
        Param11.ParameterName = "@LastUpdatedDateTime";
        Param11.DbType = DbType.DateTime;
        Param11.Value = DataRowVersion.Original;
        Param11.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param12 = objupdatecmd.CreateParameter();
        Param12.ParameterName = "@ID";
        Param12.DbType = DbType.Int32;
        Param12.Value = DataRowVersion.Original;
        Param12.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param1);
        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param5);
        objupdatecmd.Parameters.Add(Param6);
        objupdatecmd.Parameters.Add(Param8);
        objupdatecmd.Parameters.Add(Param9);
        objupdatecmd.Parameters.Add(Param10);
        objupdatecmd.Parameters.Add(Param11);
        objupdatecmd.Parameters.Add(Param12);
        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }

    public void UpdateDatabyAdapter_IncentiveTargetLeader(string selectcommand, DataTable dt)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = @"UPDATE IncentiveTargetLeaderLV4 
                                    SET [Status]='I',
                                        LastUpdatedBy=@LastUpdatedBy,
                                        LastUpdatedDateTime=@LastUpdatedDateTime,
                                        [RowVersion]=RowVersion+1
                                    WHERE (ID = @ID)";

        DbParameter Param1 = objupdatecmd.CreateParameter();
        Param1.ParameterName = "Status";
        Param1.DbType = DbType.String;
        Param1.Value = DataRowVersion.Original;

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "@LastUpdatedBy";
        Param2.DbType = DbType.Int32;
        Param2.Value = DataRowVersion.Original;
        Param2.SourceColumn = "LastUpdatedBy";

        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "@LastUpdatedDateTime";
        Param3.DbType = DbType.DateTime;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "RowVersion";
        Param4.DbType = DbType.Int32;
        Param4.Value = DataRowVersion.Original;

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "@ID";
        Param5.DbType = DbType.Int32;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param1);
        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param5);
        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }

    public void UpdateDatabyAdapter_AdjustTarget(string selectcommand, DataTable dt, string commandText)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = commandText;

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "Status";
        Param2.DbType = DbType.String;
        Param2.Value = DataRowVersion.Original;

        DbParameter Param6 = objupdatecmd.CreateParameter();
        Param6.ParameterName = "ToDate";
        Param6.DbType = DbType.DateTime;
        Param6.Value = DataRowVersion.Original;
        Param6.SourceColumn = "ToDate";

        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "LastUpdatedBy";
        Param3.DbType = DbType.Int32;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "LastUpdatedBy";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "LastUpdatedDateTime";
        Param4.DbType = DbType.DateTime;
        Param4.Value = DataRowVersion.Original;
        Param4.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param7 = objupdatecmd.CreateParameter();
        Param7.ParameterName = "RowVersion";
        Param7.DbType = DbType.Int32;
        Param7.Value = DataRowVersion.Original;

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "ID";
        Param5.DbType = DbType.Int32;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param6);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param7);
        objupdatecmd.Parameters.Add(Param5);

        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }

    public void UpdateDatabyAdapter_AdjustTarget1(string selectcommand, DataTable dt)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = @"UPDATE AdjustTarget SET 
                        TargetShopLastMonth=@TargetShopLastMonth,
                        TargetOnlineLastMonth = @TargetOnlineLastMonth, 
                        TargetShop=@TargetShop,
                        TargetOnline=@TargetOnline, 
                        Total=@Total, 
                        Growth= @Growth / 100,
                        AdjustTarget=@AdjustTarget,
                        Remark=@Remark,
                        LastUpdatedBy=@LastUpdatedBy,
                        LastUpdatedDateTime=@LastUpdatedDateTime WHERE (ID = @ID)";

        DbParameter Param1 = objupdatecmd.CreateParameter();
        Param1.ParameterName = "@TargetShopLastMonth";
        Param1.DbType = DbType.Int32;
        Param1.Value = DataRowVersion.Original;
        Param1.SourceColumn = "TargetShopLastMonth";

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "@TargetOnlineLastMonth";
        Param2.DbType = DbType.Int32;
        Param2.Value = DataRowVersion.Original;
        Param2.SourceColumn = "TargetOnlineLastMonth";

        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "@TargetShop";
        Param3.DbType = DbType.Int32;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "TargetShop";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "@TargetOnline";
        Param4.DbType = DbType.Int32;
        Param4.Value = DataRowVersion.Original;
        Param4.SourceColumn = "TargetOnline";

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "@Total";
        Param5.DbType = DbType.Int32;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "Total";

        DbParameter Param6 = objupdatecmd.CreateParameter();
        Param6.ParameterName = "@Growth";
        Param6.DbType = DbType.Double;
        Param6.Value = DataRowVersion.Original;
        Param6.SourceColumn = "Growth";


        DbParameter Param7 = objupdatecmd.CreateParameter();
        Param7.ParameterName = "@AdjustTarget";
        Param7.DbType = DbType.Int32;
        Param7.Value = DataRowVersion.Original;
        Param7.SourceColumn = "AdjustTarget";

        DbParameter Param8 = objupdatecmd.CreateParameter();
        Param8.ParameterName = "@Remark";
        Param8.DbType = DbType.String;
        Param8.Value = DataRowVersion.Original;
        Param8.SourceColumn = "Remark";

        DbParameter Param9 = objupdatecmd.CreateParameter();
        Param9.ParameterName = "@LastUpdatedBy";
        Param9.DbType = DbType.Int32;
        Param9.Value = DataRowVersion.Original;
        Param9.SourceColumn = "LastUpdatedBy";

        DbParameter Param10 = objupdatecmd.CreateParameter();
        Param10.ParameterName = "@LastUpdatedDateTime";
        Param10.DbType = DbType.DateTime;
        Param10.Value = DataRowVersion.Original;
        Param10.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param11 = objupdatecmd.CreateParameter();
        Param11.ParameterName = "@ID";
        Param11.DbType = DbType.Int32;
        Param11.Value = DataRowVersion.Original;
        Param11.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param1);
        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param5);
        objupdatecmd.Parameters.Add(Param6);
        objupdatecmd.Parameters.Add(Param7);
        objupdatecmd.Parameters.Add(Param8);
        objupdatecmd.Parameters.Add(Param9);
        objupdatecmd.Parameters.Add(Param10);
        objupdatecmd.Parameters.Add(Param11);
        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }

    public void UpdateDatabyAdapter_AdjustPoint(string selectcommand, DataTable dt, string commandText)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = commandText;

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "Status";
        Param2.DbType = DbType.String;
        Param2.Value = DataRowVersion.Original;

        DbParameter Param6 = objupdatecmd.CreateParameter();
        Param6.ParameterName = "ToDate";
        Param6.DbType = DbType.DateTime;
        Param6.Value = DataRowVersion.Original;
        Param6.SourceColumn = "ToDate";

        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "LastUpdatedBy";
        Param3.DbType = DbType.Int32;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "LastUpdatedBy";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "LastUpdatedDateTime";
        Param4.DbType = DbType.DateTime;
        Param4.Value = DataRowVersion.Original;
        Param4.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param7 = objupdatecmd.CreateParameter();
        Param7.ParameterName = "RowVersion";
        Param7.DbType = DbType.Int32;
        Param7.Value = DataRowVersion.Original;

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "ID";
        Param5.DbType = DbType.Int32;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param6);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param7);
        objupdatecmd.Parameters.Add(Param5);

        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }


    public void UpdateDatabyAdapter_IncentivePointLimit(string selectcommand, DataTable dt, string commandText)
    {
        DbCommandBuilder builder = objFactory.CreateCommandBuilder();
        objAdapter = objFactory.CreateDataAdapter();
        builder.DataAdapter = objAdapter;
        objCommand.CommandText = selectcommand;
        objCommand.CommandType = CommandType.Text;
        objAdapter.SelectCommand = objCommand;
        DbCommand objupdatecmd = objFactory.CreateCommand();

        objupdatecmd.CommandText = commandText;

        DbParameter Param1 = objupdatecmd.CreateParameter();
        Param1.ParameterName = "ProductHierarchyDensity";
        Param1.DbType = DbType.Double;
        Param1.Value = DataRowVersion.Original;
        Param1.SourceColumn = "ProductHierarchyDensity";

        DbParameter Param2 = objupdatecmd.CreateParameter();
        Param2.ParameterName = "ProductHierarchyProfit";
        Param2.DbType = DbType.Double;
        Param2.Value = DataRowVersion.Original;
        Param2.SourceColumn = "ProductHierarchyProfit";


        DbParameter Param3 = objupdatecmd.CreateParameter();
        Param3.ParameterName = "LastUpdatedBy";
        Param3.DbType = DbType.Int32;
        Param3.Value = DataRowVersion.Original;
        Param3.SourceColumn = "LastUpdatedBy";

        DbParameter Param4 = objupdatecmd.CreateParameter();
        Param4.ParameterName = "LastUpdatedDateTime";
        Param4.DbType = DbType.DateTime;
        Param4.Value = DataRowVersion.Original;
        Param4.SourceColumn = "LastUpdatedDateTime";

        DbParameter Param7 = objupdatecmd.CreateParameter();
        Param7.ParameterName = "RowVersion";
        Param7.DbType = DbType.Int32;
        Param7.Value = DataRowVersion.Original;

        DbParameter Param5 = objupdatecmd.CreateParameter();
        Param5.ParameterName = "ID";
        Param5.DbType = DbType.Int32;
        Param5.Value = DataRowVersion.Original;
        Param5.SourceColumn = "ID";

        objupdatecmd.Parameters.Add(Param1);
        objupdatecmd.Parameters.Add(Param2);
        objupdatecmd.Parameters.Add(Param3);
        objupdatecmd.Parameters.Add(Param4);
        objupdatecmd.Parameters.Add(Param7);
        objupdatecmd.Parameters.Add(Param5);

        objAdapter.UpdateCommand = objupdatecmd;

        objAdapter.Update(dt);
    }
    #endregion
}

public enum Providers
{
    SqlServer, OleDb, Oracle, ODBC, ConfigDefined
}

public enum ConnectionState
{
    KeepOpen, CloseOnExit
}

public enum DatabaseObjects
{
    Columns, Tables
}
