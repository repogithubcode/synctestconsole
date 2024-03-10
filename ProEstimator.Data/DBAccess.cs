using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using ProEstimatorData.Helpers;

namespace ProEstimatorData
{
    public class DBAccess
    {
        private string _connectionString;
        private int _activeLoginID = 0;

        public static void RegisterThread(int threadID, int activeLoginID)
        {
            if (_threadMap.ContainsKey(threadID))
            {
                _threadMap.Remove(threadID);
            }

            _threadMap.Add(threadID, activeLoginID);
        }

        public static void UnregisterThread(int threadID)
        {
            if (_threadMap.ContainsKey(threadID))
            {
                _threadMap.Remove(threadID);
            }
        }

        private static Dictionary<int, int> _threadMap = new Dictionary<int, int>();

        /// <summary>
        /// Open access to the ProEstimator database
        /// </summary>
        public DBAccess()
        {
            Initialize(DatabaseName.ProEstimator);
        }

        /// <summary>
        /// Open access to the passed Database.
        /// </summary>
        public DBAccess(DatabaseName database)
        {
            Initialize(database);
        }

        public DBAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool FromNonProd()
        {
            string dataSource = new SqlConnectionStringBuilder(_connectionString).DataSource;
            if(dataSource.Contains("10.0.7.13"))
            {
                return false;
            }
            return true;
        }

        private void Initialize(DatabaseName database)
        {
            if (database == DatabaseName.ProEstimator)
            {
                _connectionString = ConfigurationManager.AppSettings["ProConnectionString"];
            }
            else if (database == DatabaseName.Mitchell)
            {
                _connectionString = ConfigurationManager.AppSettings["Mitchell3ConnectionString"];
            }
            else if (database == DatabaseName.ChangeLog)
            {
                _connectionString = ConfigurationManager.AppSettings["LogConnectionString"];
            }

            //int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //if (_threadMap.ContainsKey(threadID))
            //{
            //    _activeLoginID = _threadMap[threadID];
            //}

            _activeLoginID = HttpContextCurrentItemsHelper.GetItemValue("ActiveLoginID", 0);
        }

        private static bool? _doQueryLog;
        private static string _queryLogConnectionString;

        private void LogQueryData(string storedProcedure, List<SqlParameter> parameters, long executionTicks, int rows, string error = "")
        {
            // Get configuration only once
            if (!_doQueryLog.HasValue)
            {
                try
                {
                    string enableQueryLog = ConfigurationManager.AppSettings.Get("EnableQueryLog").ToLower();
                    _doQueryLog = enableQueryLog == "true";
                }
                catch 
                {
                    _doQueryLog = false;
                }

                try
                {
                    _queryLogConnectionString = ConfigurationManager.AppSettings.Get("LogConnectionString");
                }
                catch { }
            }

            if (_doQueryLog.Value)
            {
                StringBuilder builder = new StringBuilder();
                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        builder.Append("@" + parameter.ParameterName + " = " + (parameter.DbType == DbType.String ? "'" : "") + parameter.Value + (parameter.DbType == DbType.String ? "'" : "") + ", ");
                    }
                }

                // Get a string representing the function call stack
                Stack<string> functionNames = new Stack<string>();
                
                StackTrace trace = new StackTrace();
                for (int i = 2; i < trace.FrameCount; i++)
                {
                    StackFrame frame = trace.GetFrame(i);
                    string callerName = frame.GetMethod().Name;

                    if (callerName.StartsWith("get_") || callerName == "CallEndDel" || callerName == "End" || callerName == ".ctor" || callerName == "lambda_method")
                    {
                        break;
                    }
                    else
                    {
                        functionNames.Push(callerName);
                    }
                }

                StringBuilder callBuilder = new StringBuilder();
                while (functionNames.Count > 0)
                {
                    callBuilder.Append("/" + functionNames.Pop());
                }

                string callStack = callBuilder.ToString();

                // Do the insert
                try
                {
                    using (SqlConnection connection = new SqlConnection(string.IsNullOrEmpty(_queryLogConnectionString) ? _connectionString : _queryLogConnectionString))
                    {
                        using (SqlCommand command = new SqlCommand("QueryLog_Insert", connection))
                        {
                            command.CommandType  = CommandType.StoredProcedure;

                            command.Parameters.Add(new SqlParameter("StoredProcedure", storedProcedure));
                            command.Parameters.Add(new SqlParameter("Parameters", builder.ToString()));
                            command.Parameters.Add(new SqlParameter("ExecutionMilliseconds", Math.Round(executionTicks / 10000.0, 3)));
                            command.Parameters.Add(new SqlParameter("ActiveLoginID", _activeLoginID));
                            try
                            {
                                if (System.Web.HttpContext.Current != null)
                                {
                                    command.Parameters.Add(new SqlParameter("PageRequest", System.Web.HttpContext.Current.Request.Path));
                                }
                                else
                                {
                                    command.Parameters.Add(new SqlParameter("PageRequest", ""));
                                }
                            }
                            catch 
                            {
                                command.Parameters.Add(new SqlParameter("PageRequest", ""));
                            }
                            command.Parameters.Add(new SqlParameter("CallStack", callStack));
                            command.Parameters.Add(new SqlParameter("ReturnedRows", rows));
                            command.Parameters.Add(new SqlParameter("Error", error));

                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.WriteToLogFile("DBAccess LogQueryData: " + ex.Message);
                }
            }
        }

        #region ExecuteNonQuery

        public FunctionResult ExecuteNonQuery(string storedProcedureName)
        {
            return ExecuteNonQuery(storedProcedureName, new List<SqlParameter>());
        }

        public FunctionResult ExecuteNonQuery(string storedProcedureName, SqlParameter parameter)
        {
            return ExecuteNonQuery(storedProcedureName, parameter, -1);
        }

        public FunctionResult ExecuteNonQuery(string storedProcedureName, SqlParameter parameter, int timeout)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteNonQuery(storedProcedureName, parameters, timeout);
        }

        public FunctionResult ExecuteNonQuery(string storedProcedureName, SqlParameter parameter, int timeout, string conn)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteNonQuery(storedProcedureName, parameters, timeout, conn);
        }

        public FunctionResult ExecuteNonQuery(string commandText, List<SqlParameter> parameters, bool isStoredProc = true)
        {
            var commandtype = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
            return ExecuteNonQuery(commandText, parameters, -1, commandtype);
        }
               
        public FunctionResult ExecuteNonQuery(string storedProcedureName, List<SqlParameter> parameters, int timeout, CommandType commandType = CommandType.StoredProcedure)
        {
            string error = "";

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int rows = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = commandType;

                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        connection.Open();

                        if (timeout > -1)
                        {
                            command.CommandTimeout = timeout;
                        }

                        rows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("'" + storedProcedureName + "': " + ex.Message, "DBAccess ExecuteNonQuery");
                error = ex.Message;
            }

            LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks, rows, error);

            if (string.IsNullOrEmpty(error))
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult("Error executing stored procedure " + storedProcedureName + ": " + error);
            }
        }

        public FunctionResult ExecuteNonQuery(string storedProcedureName, List<SqlParameter> parameters, int timeout, string conn, CommandType commandType = CommandType.StoredProcedure)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string error = "";
            int rows = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = commandType;

                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        connection.Open();

                        if (timeout > -1)
                        {
                            command.CommandTimeout = timeout;
                        }

                        rows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                ErrorLogger.LogError("'" + storedProcedureName + "': " + ex.Message, "DBAccess ExecuteNonQuery");
            }

            LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks, rows, error);

            if (string.IsNullOrEmpty(error))
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult("Error executing stored procedure " + storedProcedureName + ": " + error);
            }
        }

        #endregion

        #region ExecuteWithTable

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName, bool setErrorOnNoData = true)
        {
            return ExecuteWithTable(storedProcedureName, new List<SqlParameter>(), CommandType.StoredProcedure, setErrorOnNoData);
        }

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName, SqlParameter parameter, bool setErrorOnNoData = true)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithTable(storedProcedureName, parameters, CommandType.StoredProcedure, setErrorOnNoData);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText, bool setErrorOnNoData = true)
        {
            return ExecuteWithTable(commandText, new List<SqlParameter>(), CommandType.Text, setErrorOnNoData);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText, SqlParameter parameter, bool setErrorOnNoData = true)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithTable(commandText, parameters, CommandType.Text, setErrorOnNoData);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText, List<SqlParameter> parameters, bool setErrorOnNoData = true)
        {
            return ExecuteWithTable(commandText, parameters, CommandType.Text, setErrorOnNoData);
        }

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName, List<SqlParameter> parameters,
            CommandType CommandType = CommandType.StoredProcedure, bool setErrorOnNoData = true)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string error = "";
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType;//CommandType.StoredProcedure;
                        command.CommandTimeout = 0;

                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        connection.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            da.Fill(table);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                ErrorLogger.LogError("'" + storedProcedureName + "': " + ex.Message, "DBAccess ExecuteWithTable");
            }

            LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks, table.Rows.Count, error);

            if (!string.IsNullOrEmpty(error))
            {
                return new DBAccessTableResult("Error executing stored procedure " + storedProcedureName + ": " + error);
            }
            else
            {
                if (table.Rows.Count > 0 || !setErrorOnNoData)
                {
                    return new DBAccessTableResult(table);
                }
                else
                {
                    return new DBAccessTableResult("No data.");
                }
            }
        }

        public DBAccessDataSetResult ExecuteWithDataSet(string storedProcedureName, List<SqlParameter> parameters, CommandType CommandType = CommandType.StoredProcedure)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataSet dataSet = new DataSet();
            string error = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType;//CommandType.StoredProcedure;
                        command.CommandTimeout = 0;

                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        connection.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            da.Fill(dataSet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                ErrorLogger.LogError("'" + storedProcedureName + "': " + ex.Message, "DBAccess ExecuteWithDataSet");
            }

            LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks, dataSet.Tables.Count, error);

            if (!string.IsNullOrEmpty(error))
            {
                return new DBAccessDataSetResult("Error executing stored procedure " + storedProcedureName + ": " + error);
            }
            else
            {
                if (dataSet.Tables.Count > 0)
                {
                    return new DBAccessDataSetResult(dataSet);
                }
                else
                {
                    return new DBAccessDataSetResult("No data.");
                }
            }
        }

        #endregion

        #region ExecuteWithIntReturn

        public DBAccessIntResult ExecuteWithIntReturn(string storedProcedureName, SqlParameter parameter = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (parameter != null)
                parameters.Add(parameter);
            return ExecuteWithIntReturn(storedProcedureName, parameters);
        }
        public DBAccessIntResult ExecuteWithIntReturnForQuery(string commandText, SqlParameter parameter = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (parameter != null)
                parameters.Add(parameter);

            return ExecuteWithIntReturn(commandText, parameters, CommandType.Text);
        }

        public string ExecuteScalar(string storedProcedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string error = "";
            string result = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = commandType;
                        command.Parameters.Clear(); 

                        if (parameters != null && parameters.Count > 0)
                            command.Parameters.AddRange(parameters.ToArray());

                        connection.Open();

                        result = command.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("'" + storedProcedureName + "': " + ex.Message, "DBAccess ExecuteWithScalarReturn");
                error = ex.Message;
            }

            LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks, 1, error);
            return result;
        }

        public DBAccessIntResult ExecuteWithIntReturn(string storedProcedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            try
            {
                int result = InputHelper.GetInteger(ExecuteScalar(storedProcedureName,parameters,commandType));
                return new DBAccessIntResult(result);
            }
            catch (Exception ex)
            {
                ErrorLogger.WriteToLogFile("DBAccess ExecuteWithIntReturn '" + storedProcedureName + "': " + ex.Message);
                return new DBAccessIntResult("Error executing stored procedure " + storedProcedureName + ": " + ex.Message);
            }
        }

        public DBAccessStringResult ExecuteWithStringReturn(string storedProcedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            try
            { 
                return new DBAccessStringResult(ExecuteScalar(storedProcedureName, parameters, commandType)); 
            }
            catch (Exception ex)
            {
                ErrorLogger.WriteToLogFile("DBAccess ExecuteWithStringReturn '" + storedProcedureName + "': " + ex.Message);
                return new DBAccessStringResult("Error executing stored procedure " + storedProcedureName + ": " + ex.Message,true);
            }
        }

        public DBAccessStringResult ExecuteWithStringReturn(string storedProcedureName, SqlParameter parameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithStringReturn(storedProcedureName, parameters);
        }

        #endregion

        public FunctionResult ExecuteSql(string sql, int timeout = -1)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;

                        if (timeout > -1)
                        {
                            command.CommandTimeout = timeout;
                        } 

                        connection.Open();
                        command.ExecuteNonQuery();

                        return new FunctionResult();
                    }
                }
            }
            catch (Exception ex)
            {
                string sqlToLog = sql;
                if (sqlToLog.Length > 1500)
                {
                    sqlToLog = sqlToLog.Substring(0, 1500) + "...";
                }

                ErrorLogger.LogError(ex.Message + Environment.NewLine + sqlToLog, 0, 0, "ExecuteSql");
                return new FunctionResult("Error executing sql " + sqlToLog + ": " + ex.Message);
            }
        }
    }

    public enum DatabaseName
    {
        ProEstimator,
        Mitchell,
        ChangeLog
    }    

    public class DBAccessTableResult : FunctionResult
    {
        public DataTable DataTable { get; private set; }

        public DBAccessTableResult()
            : base()
        {
            DataTable = new DataTable();
        }

        public DBAccessTableResult(string errorMessage) 
            : base(errorMessage)
        {
            DataTable = new DataTable();
        }

        public DBAccessTableResult(DataTable dataTable)
            : base()
        {
            DataTable = dataTable;
        }
    }

    public class DBAccessIntResult : FunctionResult
    {
        public int Value { get; private set; }

        public DBAccessIntResult()
            : base()
        { }

        public DBAccessIntResult(string errorMessage)
            : base(errorMessage)
        { }

        public DBAccessIntResult(int value)
            : base()
        {
            Value = value;
        }
    }

    public class DBAccessStringResult : FunctionResult
    {
        public string Value { get; private set; }

        public DBAccessStringResult()
            : base()
        { }
         
        public DBAccessStringResult(string value, bool isError=false)
            : base(!isError, isError ? value : null)
        {
            Value = value;
        }
    }

    public class DBAccessDataSetResult : FunctionResult
    {
        public DataSet DataSet { get; private set; }

        public DBAccessDataSetResult()
            : base()
        {
            DataSet = new DataSet();
        }

        public DBAccessDataSetResult(string errorMessage)
            : base(errorMessage)
        {
            DataSet = new DataSet();
        }

        public DBAccessDataSetResult(DataSet dataSet)
            : base()
        {
            DataSet = dataSet;
        }
    }
}
