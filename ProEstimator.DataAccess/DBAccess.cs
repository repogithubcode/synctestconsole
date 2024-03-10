using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ProEstimator.DataAccess
{
    public class DBAccess
    {
        private string _connectionString;

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

        /// <summary>
        /// Open access to a database with a custom connection string.
        /// </summary>
        public DBAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void Initialize(DatabaseName database)
        {
            if (database == DatabaseName.VehicleData)
            {
                _connectionString = ConfigurationManager.AppSettings["VehicleDataConnectionString"];
            }
            else
            {
                _connectionString = ConfigurationManager.AppSettings["ProConnectionString"];
            }
        }

        private static bool? _doQueryLog;
        private static string _queryLogConnectionString;

        private void LogQueryData(string storedProcedure, List<SqlParameter> parameters, long executionTicks)
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
                    _queryLogConnectionString = ConfigurationManager.AppSettings.Get("QueryLogConnectionString");
                }
                catch { }
            }

            if (_doQueryLog.Value)
            {
                StringBuilder builder = new StringBuilder();
                foreach (SqlParameter parameter in parameters)
                {
                    builder.Append(parameter.ParameterName + ": " + parameter.Value + ", ");
                }

                // Get the login and estimate IDs if they are there
                int loginID = 0;
                int estimateID = 0;

                try
                {
                    // The path could look like /47736/estimate/6798394/customer
                    // The first number is the loginID, the second is the estimate ID
                    List<string> pieces = System.Web.HttpContext.Current.Request.Path.Split('/').ToList();
                    foreach (string piece in pieces)
                    {
                        int number = InputHelper.GetInteger(piece);
                        if (number > 0)
                        {
                            if (loginID == 0)
                            {
                                loginID = number;
                            }
                            else if (estimateID == 0)
                            {
                                estimateID = number;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // doesn't matter
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
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add(new SqlParameter("StoredProcedure", storedProcedure));
                            command.Parameters.Add(new SqlParameter("Parameters", builder.ToString()));
                            command.Parameters.Add(new SqlParameter("ExecutionMilliseconds", Math.Round(executionTicks / 10000.0, 3)));
                            command.Parameters.Add(new SqlParameter("LoginID", loginID));
                            command.Parameters.Add(new SqlParameter("EstimateID", estimateID));
                            command.Parameters.Add(new SqlParameter("PageRequest", System.Web.HttpContext.Current.Request.Path));
                            command.Parameters.Add(new SqlParameter("CallStack", callStack));

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
            return ExecuteNonQuery(storedProcedureName, parameters, timeout, CommandType.StoredProcedure, false);
        }

        public FunctionResult ExecuteNonQuery(string commandText, List<SqlParameter> parameters, bool isStoredProc = true)
        {
            var commandtype = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
            return ExecuteNonQuery(commandText, parameters, -1, commandtype, false);
        }
               
        public FunctionResult ExecuteNonQuery(string storedProcedureName, List<SqlParameter> parameters, int timeout, CommandType commandType = CommandType.StoredProcedure, bool doNotLogError = false)
        {
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

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        
                        command.ExecuteNonQuery();

                        LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks);

                        return new FunctionResult();
                    }
                }
            }
            catch (Exception ex)
            {
                if (doNotLogError)
                {
                    ErrorLogger.WriteToLogFile("DBAccess ExecuteNonQuery '" + storedProcedureName + "': " + ex.Message);
                }
                else
                {
                    ErrorLogger.LogError(ex, 0, "DBAccess ExecuteNonQuery '" + storedProcedureName + "'");

                }
                
                return new FunctionResult("Error executing stored procedure " + storedProcedureName + ": " + ex.Message);
            }
        }

        #endregion

        #region ExecuteWithTable

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName)
        {
            return ExecuteWithTable(storedProcedureName, new List<SqlParameter>());
        }

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName, SqlParameter parameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithTable(storedProcedureName, parameters);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText)
        {
            return ExecuteWithTable(commandText, new List<SqlParameter>(), CommandType.Text);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText, SqlParameter parameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithTable(commandText, parameters, CommandType.Text);
        }

        public DBAccessTableResult ExecuteWithTableForQuery(string commandText, List<SqlParameter> parameters)
        {
            return ExecuteWithTable(commandText, parameters, CommandType.Text);
        }

        public DBAccessTableResult ExecuteWithTable(string storedProcedureName, List<SqlParameter> parameters, CommandType CommandType=CommandType.StoredProcedure)
        {
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

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        DataTable table = new DataTable();

                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            da.Fill(table);
                        }

                        LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks);

                        if (table.Rows.Count > 0)
                        {
                            return new DBAccessTableResult(table);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "DBAccess ExecuteWithTable '" + storedProcedureName + "'");
                return new DBAccessTableResult("Error executing stored procedure " + storedProcedureName + ": " + ex.Message);
            }

            return new DBAccessTableResult("No data.");
        }

        #endregion

        #region ExecuteWithIntReturn

        public DBAccessIntResult ExecuteWithIntReturn(string storedProcedureName)
        {
            return ExecuteWithIntReturn(storedProcedureName, new List<SqlParameter>());
        }

        public DBAccessIntResult ExecuteWithIntReturn(string storedProcedureName, SqlParameter parameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithIntReturn(storedProcedureName, parameters);
        }
        public DBAccessIntResult ExecuteWithIntReturnForQuery(string commandText, SqlParameter parameter)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(parameter);
            return ExecuteWithIntReturn(commandText, parameters,CommandType.Text);
        }

        public string ExecuteScalar(string storedProcedureName, List<SqlParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
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

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        string result = command.ExecuteScalar().ToString();

                        LogQueryData(storedProcedureName, parameters, stopwatch.ElapsedTicks);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "DBAccess ExecuteWithScalarReturn '" + storedProcedureName + "'");
                return string.Empty;
            }
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
                ErrorLogger.LogError(ex, 0, "DBAccess ExecuteWithIntReturn '" + storedProcedureName + "'");
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
                ErrorLogger.LogError(ex, 0, "DBAccess ExecuteWithStringReturn '" + storedProcedureName + "'");
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
                if (sqlToLog.Length > 30)
                {
                    sqlToLog = sqlToLog.Substring(0, 30) + "...";
                }

                ErrorLogger.LogError(ex, 0, "ExecuteSql " + sqlToLog);
                return new FunctionResult("Error executing sql " + sqlToLog + ": " + ex.Message);
            }
        }
    }

    public enum DatabaseName
    {
        ProEstimator,
        VehicleData
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
}
