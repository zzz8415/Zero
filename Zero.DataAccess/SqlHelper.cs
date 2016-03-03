using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Collections;
using System.Configuration;

namespace Zero.DataAccess
{
    /// <summary>
    /// 将常用的数据操作以高性能、可扩展方式进行封装，适用于 Microsoft SQLServer 2000 及以上版本。
    /// </summary>
    public static class SqlHelper
    {
        private static int _commandTimeout = 30;
        /// <summary>
        /// 获取或设置SqlCommand的CommandTimeout属性，默认30秒，设置小于1时,不生效
        /// </summary>
        public static int CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (_commandTimeout > 0)
                    _commandTimeout = value;
            }
        }

        //public static readonly string CONN_STRING_NON_DTC = ConfigHelper.ConnectionString;
        // 仅提供静态方法，因此设置私有构造函数以避免在外部被实例化。

        #region 简化方法
        /// <summary>
        /// 使用默认连接串  用ConnectionString节里面的 LocalMySqlServer
        /// </summary>
        public static string defaultConnectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["DefaultConnectionString"] != null)
            {
                return ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString;
            }
            return null;
        }
        /// <summary>
        /// 使用默认连接串 执行sql返回一个执行结果
        /// </summary>
        /// <param name="commandText">sql文本</param>
        /// <returns>结果</returns>
        public static object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(defaultConnectionString, CommandType.Text, commandText);
        }
        /// <summary>
        /// 使用默认连接串 执行sql返回一个MySqlDataReader
        /// </summary>
        /// <param name="commandText">sql文本</param>
        /// <returns>MySqlDataReader对象</returns>
        public static SqlDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(defaultConnectionString, CommandType.Text, commandText);
        }
        /// <summary>
        /// 使用默认连接串 执行sql返回影响到的行
        /// </summary>
        /// <param name="commandText">sql文本</param>
        /// <returns>影响到的行</returns>
        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(defaultConnectionString, CommandType.Text, commandText);
        }
        /// <summary>
        /// 使用默认连接串 执行sql返回一个DataSet
        /// </summary>
        /// <param name="commandText">sql文本</param>
        /// <returns>DataSet对象</returns>
        public static DataSet ExecuteDataset(string commandText)
        {
            return ExecuteDataset(defaultConnectionString, CommandType.Text, commandText);
        }
        #endregion


        #region 私有静态工具方法

        /// <summary>
        /// 将参数数组关系到命令对象。
        /// 任何可输入输出参数或者空值，通过该方法将被分配一个 DbNull 值。
        /// </summary>
        /// <param name="command">要添加参数的命令对象</param>
        /// <param name="commandParameters">被添加到命令对象的参数数组</param>
        private static void AttachParameters(SqlCommand command, IEnumerable<SqlParameter> commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                foreach (SqlParameter parameter in commandParameters)
                {
                    if (parameter != null)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }

        /// <summary>
        /// 给参数数组分配数据列值
        /// </summary>
        /// <param name="commandParameters">待赋值的参数数组</param>
        /// <param name="dataRow">保持存储过程参数值的数据行</param>
        private static void AssignParameterValues(IEnumerable<SqlParameter> commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || dataRow == null)
            {
                return;
            }

            int i = 0;

            // 设置参数值
            foreach (SqlParameter commandParameter in commandParameters)
            {
                // 检查参数名
                if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
                {
                    throw new Exception(string.Format("请在 #{0} 处提供一个有效的参数名, 参数名属性现有值如下: '{1}'.", i, commandParameter.ParameterName));
                }

                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                {
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                }

                i++;
            }
        }

        /// <summary>
        /// 把一数组值赋给参数数组
        /// </summary>
        /// <param name="commandParameters">待赋值的参数数组</param>
        /// <param name="parameterValues">赋值的对象数组</param>
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters == null || parameterValues == null)
            {
                return;
            }

            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("参数数量与参数值数量不匹配.");
            }

            int j = commandParameters.Length;
            for (int i = 0; i < j; i++)
            {
                // 当数组值实现 IDbDataParameter 接口，马上赋值
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = parameterValues[i] as IDbDataParameter;
                    if (paramInstance == null || paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// 准备数据操作命令
        /// </summary>
        /// <param name="command">待准备的命令对象</param>
        /// <param name="connection">执行该命令的有效数据库连接</param>
        /// <param name="transaction">有效数据事务对象，或者 null</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">SqlParameter 参数数组，如果无参数则为 null</param>
        /// <param name="mustCloseConnection"><c>true</c> 如果打开数据库连接则为 true，否则为 false</param>
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, IEnumerable<SqlParameter> commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            // 如果该数据库连接没有打开，则设置为打开状态
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            // 如果有提供数据事务
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("打开状态的事务允许数据操作回滚或者提交。", "transaction");
                }
                command.Transaction = transaction;
            }

            command.CommandType = commandType;

            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            int result = ExecuteNonQuery(connectionString, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int result = ExecuteNonQuery(connection, commandType, commandText, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 此方法不支持对输出参数或者存储过程里的返回参数的访问
        /// 
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">赋值给存储过程输入参数的数组</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                int result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);

                return result;
            }
            else
            {
                int result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            int result = ExecuteNonQuery(connection, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);
            int result = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            int result;

            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                result = ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            int result = ExecuteNonQuery(transaction, commandType, commandText, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            int result = cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            int result;

            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                result = ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }

            return result;
        }

        #endregion ExecuteNonQuery

        #region ExecuteNonQueryWithTimeout

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout)
        {
            int result = ExecuteNonQueryWithTimeout(connectionString, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(string connectionString, CommandType commandType, string commandText,
            int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int result = ExecuteNonQueryWithTimeout(connection, commandType, commandText, commandTimeout, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 此方法不支持对输出参数或者存储过程里的返回参数的访问
        /// 
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">赋值给存储过程输入参数的数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(string connectionString, string spName,
            int commandTimeout, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                int result = ExecuteNonQueryWithTimeout(connectionString, CommandType.StoredProcedure,
                    spName, commandTimeout, commandParameters);

                return result;
            }
            else
            {
                int result = ExecuteNonQueryWithTimeout(connectionString, CommandType.StoredProcedure,
                    spName, commandTimeout);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="spName">储存过程名称</param>
        /// <param name="commandTimeout">命令执行的超时时间(单位:秒,不允许传入0)</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后受影响的行数</returns>
        /// <remarks>
        /// 当对上下文连接（要用连接字符串中的“context connection=true”打开的 SqlConnection）执行命令时，CommandTimeout 将不起作用。
        /// 此属性是在执行命令或处理结果期间所有网络读取的累积超时。 在返回第一行之后，超时仍然可能发生，但只包括网络读取时间，而不包括用户处理时间。
        /// 示例:
        /// int result = ExecuteNonQueryWithTimeout(conn, "PublishOrders", 100, new SqlParameter("@prodid", 24));
        /// </remarks>
        public static int ExecuteNonQueryWithTimeout(string connectionString, string spName, int commandTimeout, params SqlParameter[] commandParameters)
        {

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand();
                bool mustCloseConnection;
                PrepareCommand(cmd, connection, default(SqlTransaction), CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);
                if (commandTimeout > 0)
                    cmd.CommandTimeout = commandTimeout;
                int result = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlConnection connection, CommandType commandType, string commandText, int commandTimeout)
        {
            int result = ExecuteNonQueryWithTimeout(connection, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlConnection connection, CommandType commandType, string commandText,
            int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);
            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            int result = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlConnection connection, string spName, int commandTimeout, params object[] parameterValues)
        {
            int result;

            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                result = ExecuteNonQueryWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
            }
            else
            {
                result = ExecuteNonQueryWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout);
            }

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout)
        {
            int result = ExecuteNonQueryWithTimeout(transaction, commandType, commandText, commandTimeout, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText,
            int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            int result = cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQueryWithTimeout(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryWithTimeout(SqlTransaction transaction, string spName,
            int commandTimeout, params object[] parameterValues)
        {
            int result;

            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                result = ExecuteNonQueryWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
            }
            else
            {
                result = ExecuteNonQueryWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout);
            }

            return result;
        }

        #endregion ExecuteNonQuery

        #region ExecuteDataset

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            DataSet result = ExecuteDataset(connectionString, commandType, commandText, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DataSet result = ExecuteDataset(connection, commandType, commandText, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);

                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            DataSet result = ExecuteDataset(connection, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用于执行命令的参数数组</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);

                cmd.Parameters.Clear();

                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return ds;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);

                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            DataSet result = ExecuteDataset(transaction, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用于执行命令的参数数组</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);

                cmd.Parameters.Clear();

                return ds;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion ExecuteDataset

        #region ExecuteDatasetWithTimeout

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout)
        {
            DataSet result = ExecuteDatasetWithTimeout(connectionString, commandType, commandText, commandTimeout, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DataSet result = ExecuteDatasetWithTimeout(connection, commandType, commandText, commandTimeout, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(string connectionString, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDatasetWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);

                return result;
            }
            else
            {
                DataSet result = ExecuteDatasetWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout);

                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlConnection connection, CommandType commandType, string commandText, int commandTimeout)
        {
            DataSet result = ExecuteDatasetWithTimeout(connection, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用于执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlConnection connection, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);
            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);

                cmd.Parameters.Clear();

                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return ds;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlConnection connection, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDatasetWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);

                return result;
            }
            else
            {
                DataSet result = ExecuteDatasetWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout);
                return result;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout)
        {
            DataSet result = ExecuteDatasetWithTimeout(transaction, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用于执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);

                cmd.Parameters.Clear();

                return ds;
            }
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDatasetWithTimeout(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetWithTimeout(SqlTransaction transaction, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                DataSet result = ExecuteDatasetWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return result;
            }
            else
            {
                DataSet result = ExecuteDatasetWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout);
                return result;
            }
        }

        #endregion ExecuteDataset

        #region ExecuteReader

        /// <summary>
        /// 指示调用方是否提供数据库连接，或者由数据访问帮助类创建，这样当调用 ExecuteReader() 时方便我们设置正确适当的 CommandBehavior 属性。
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>数据库连接属于数据访问帮助类，并由该类管理。</summary>
            Internal,
            /// <summary>数据库连接属于调用方，并由调用方管理。</summary>
            External
        }

        /// <summary>
        /// 创建并准备数据执行命令对象，同时以适当的 CommandBehavior 属性调用 ExecuteReader 方法。
        /// </summary>
        /// <remarks>
        /// 如果创建并打开数据库连接对象，当 DataReader 被关闭时必须关闭数据库连接。
        /// 如果是由调用方提供数据库连接对象，不必进行任何关闭操作，由调用方进行管理。
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="transaction">有效的事务对象，或者为 null</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组，如果不必提供参数数组可设置为 null</param>
        /// <param name="connectionOwnership">指示调用方是否提供数据库连接，或者由数据访问帮助类创建</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, 
            CommandType commandType, string commandText, IEnumerable<SqlParameter> commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                SqlDataReader dataReader;

                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                bool canClear = true;
                foreach (SqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                    {
                        canClear = false;
                    }
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            SqlDataReader dataReader = ExecuteReader(connectionString, commandType, commandText, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                SqlDataReader dataReader = ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
                return dataReader;
            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }

        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);

                SqlDataReader dataReader = ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
                return dataReader;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            SqlDataReader dataReader = ExecuteReader(connection, commandType, commandText, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlDataReader dataReader = ExecuteReader(connection, default(SqlTransaction), commandType, commandText, commandParameters, SqlConnectionOwnership.External);

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                SqlDataReader dataReader = ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReader(connection, CommandType.StoredProcedure, spName);
                return dataReader;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            SqlDataReader dataReader = ExecuteReader(transaction, commandType, commandText, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlDataReader dataReader = ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                SqlDataReader dataReader = ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReader(transaction, CommandType.StoredProcedure, spName);
                return dataReader;
            }
        }

        #endregion ExecuteReader

        #region ExecuteReaderWithTimeout
        /// <summary>
        /// 创建并准备数据执行命令对象，同时以适当的 CommandBehavior 属性调用 ExecuteReader 方法。
        /// </summary>
        /// <remarks>
        /// 如果创建并打开数据库连接对象，当 DataReader 被关闭时必须关闭数据库连接。
        /// 如果是由调用方提供数据库连接对象，不必进行任何关闭操作，由调用方进行管理。
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="transaction">有效的事务对象，或者为 null</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组，如果不必提供参数数组可设置为 null</param>
        /// <param name="connectionOwnership">指示调用方是否提供数据库连接，或者由数据访问帮助类创建</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        private static SqlDataReader ExecuteReaderWithTimeout(SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, int commandTimeout, IEnumerable<SqlParameter> commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                if (commandTimeout > 0)
                    cmd.CommandTimeout = commandTimeout;
                SqlDataReader dataReader;

                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                bool canClear = true;
                foreach (SqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                    {
                        canClear = false;
                    }
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout)
        {
            SqlDataReader dataReader = ExecuteReaderWithTimeout(connectionString, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                SqlDataReader dataReader = ExecuteReaderWithTimeout(connection, null, commandType, commandText,
                    commandTimeout, commandParameters, SqlConnectionOwnership.Internal);
                return dataReader;
            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }

        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(string connectionString, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);

                SqlDataReader dataReader = ExecuteReaderWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReaderWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout);
                return dataReader;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlConnection connection, CommandType commandType, string commandText, int commandTimeout)
        {
            SqlDataReader dataReader = ExecuteReaderWithTimeout(connection, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlConnection connection, CommandType commandType,
            string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            SqlDataReader dataReader = ExecuteReaderWithTimeout(connection, default(SqlTransaction), commandType,
                commandText, commandTimeout, commandParameters, SqlConnectionOwnership.External);

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlConnection connection, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                SqlDataReader dataReader = ExecuteReaderWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReaderWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout);
                return dataReader;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout)
        {
            SqlDataReader dataReader = ExecuteReaderWithTimeout(transaction, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///   SqlDataReader dr = ExecuteReaderWithTimeout(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlTransaction transaction, CommandType commandType,
            string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlDataReader dataReader = ExecuteReaderWithTimeout(transaction.Connection, transaction, commandType,
                commandText, commandTimeout, commandParameters, SqlConnectionOwnership.External);

            return dataReader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个数据读取对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlDataReader dr = ExecuteReaderWithTimeout(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderWithTimeout(SqlTransaction transaction, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                SqlDataReader dataReader = ExecuteReaderWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return dataReader;
            }
            else
            {
                SqlDataReader dataReader = ExecuteReaderWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout);
                return dataReader;
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            object result = ExecuteScalar(connectionString, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                object result = ExecuteScalar(connection, commandType, commandText, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            object result = ExecuteScalar(connection, commandType, commandText, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();

            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);

            object retval = cmd.ExecuteScalar();

            cmd.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return retval;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            object result = ExecuteScalar(transaction, commandType, commandText, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            object retval = cmd.ExecuteScalar();

            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion ExecuteScalar

        #region ExecuteScalarWithTimeout

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout)
        {
            object result = ExecuteScalarWithTimeout(connectionString, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(string connectionString, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                object result = ExecuteScalarWithTimeout(connection, commandType, commandText, commandTimeout, commandParameters);

                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(string connectionString, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalarWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalarWithTimeout(connectionString, CommandType.StoredProcedure, spName, commandTimeout);
                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlConnection connection, CommandType commandType, string commandText, int commandTimeout)
        {
            object result = ExecuteScalarWithTimeout(connection, commandType, commandText, commandTimeout, default(SqlParameter[]));

            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlConnection connection, CommandType commandType, 
            string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand cmd = new SqlCommand();

            bool mustCloseConnection;
            PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);

            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            object retval = cmd.ExecuteScalar();

            cmd.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return retval;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlConnection connection, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalarWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalarWithTimeout(connection, CommandType.StoredProcedure, spName, commandTimeout);
                return result;
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout)
        {
            object result = ExecuteScalarWithTimeout(transaction, commandType, commandText, commandTimeout, default(SqlParameter[]));
            return result;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlTransaction transaction, CommandType commandType, string commandText, int commandTimeout, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            if (commandTimeout > 0)
                cmd.CommandTimeout = commandTimeout;
            object retval = cmd.ExecuteScalar();

            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalarWithTimeout(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <param name="commandTimeout">设定Command执行时间，秒，大于0有效</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarWithTimeout(SqlTransaction transaction, string spName, int commandTimeout, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                object result = ExecuteScalarWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalarWithTimeout(transaction, CommandType.StoredProcedure, spName, commandTimeout);
                return result;
            }
        }

        #endregion ExecuteScalar

        #region ExecuteXmlReader

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的包含"FOR XML AUTO" 的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            XmlReader reader = ExecuteXmlReader(connection, commandType, commandText, default(SqlParameter[]));

            return reader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的包含"FOR XML AUTO" 的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, default(SqlTransaction), commandType, commandText, commandParameters, out mustCloseConnection);

                XmlReader retval = cmd.ExecuteXmlReader();

                cmd.Parameters.Clear();

                return retval;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">包含 "FOR XML AUTO" 语句的存储过程的名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                XmlReader reader = ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                return reader;
            }
            else
            {
                XmlReader reader = ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
                return reader;
            }
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的包含"FOR XML AUTO" 的 Transact-SQL 语句或存储过程</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            XmlReader reader = ExecuteXmlReader(transaction, commandType, commandText, default(SqlParameter[]));

            return reader;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的包含"FOR XML AUTO" 的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }

            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            XmlReader retval = cmd.ExecuteXmlReader();

            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// 将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  XmlReader reader = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                XmlReader reader = ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return reader;
            }
            else
            {
                XmlReader reader = ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
                return reader;
            }
        }

        #endregion ExecuteXmlReader

        #region FillDataset

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>    
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        public static void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>    
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        /// <param name="parameterValues">参数对象数组，赋值为存储过程输入参数</param>
        public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, parameterValues);
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// 在 DataSet 中添加或刷新行以匹配使用 DataSet 名称的数据源中的行，并创建一个名为 TableNames 数组表名。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="commandType">获取或设置一个值，该值指示如何解释 CommandText 属性</param>
        /// <param name="commandText">获取或设置要对数据源执行的Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">执行命令后返回包含结果集的数据集</param>
        /// <param name="tableNames">创建的表映射，该映射允许这些表通过用户自定义名（也可以为真实表名）被引用</param>
        /// <param name="commandParameters">用来执行命令的参数数组</param>
        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            SqlCommand command = new SqlCommand();
            bool mustCloseConnection;
            PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            {
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";
                    for (int index = 0; index < tableNames.Length; index++)
                    {
                        if (tableNames[index] == null || tableNames[index].Length == 0)
                        {
                            throw new ArgumentException("输入表名必须为数组列表, 无时可为null 或空字符串.", "tableNames");
                        }
                        dataAdapter.TableMappings.Add(tableName, tableNames[index]);
                        tableName += (index + 1).ToString();
                    }
                }

                dataAdapter.Fill(dataSet);

                command.Parameters.Clear();
            }

            if (mustCloseConnection)
            {
                connection.Close();
            }
        }

        #endregion

        #region UpdateDataset

        /// <summary>
        /// 从名为“Table”的 DataTable 为指定的 DataSet 中每个已插入、已更新或已删除的行调用相应的 INSERT、UPDATE 或 DELETE 语句。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">把新记录插入到数据源中的有效的 Transact-SQL 语句或存储过程</param>
        /// <param name="deleteCommand">从数据源中删除记录的有效的 Transact-SQL 语句或存储过程</param>
        /// <param name="updateCommand">更新记录到数据源中的有效的 Transact-SQL 语句或存储过程</param>
        /// <param name="dataSet">用来更新数据源的数据集</param>
        /// <param name="tableName">用来更新数据源的表</param>
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }
            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }
            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
            {
                dataAdapter.UpdateCommand = updateCommand;
                dataAdapter.InsertCommand = insertCommand;
                dataAdapter.DeleteCommand = deleteCommand;

                dataAdapter.Update(dataSet, tableName);

                dataSet.AcceptChanges();
            }
        }

        #endregion

        #region CreateCommand

        /// <summary>
        /// 创建并返回一个与该连接相关联的 Command 对象。
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="sourceColumns">一个数组, 被赋值为存储过程参数的源列集</param>
        /// <returns>有效的 Command 对象</returns>
        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                for (int index = 0; index < sourceColumns.Length; index++)
                {
                    commandParameters[index].SourceColumn = sourceColumns[index];
                }

                AttachParameters(cmd, commandParameters);
            }
            cmd.CommandTimeout = CommandTimeout;
            return cmd;
        }

        #endregion

        #region ExecuteNonQueryTypedParams

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行存储过程并返回受影响的行数
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, dataRow);

                int result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                int result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行存储过程
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                int result = ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                int result = ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行存储过程
        /// </summary>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后受影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                int result = ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                int result = ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion

        #region ExecuteDatasetTypedParams

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetTypedParams(string connectionString, String spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, dataRow);

                DataSet result = ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <param name="connection">有效的数据连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                DataSet result = ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接执行 Transact-SQL 语句并返回 DataSet 数据集。
        /// </summary>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回一个包含结果的数据集</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                DataSet result = ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                DataSet result = ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion

        #region ExecuteReaderTypedParams

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接创建并准备数据执行命令对象。
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, dataRow);

                SqlDataReader result = ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                SqlDataReader result = ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
                return result;
            }
        }


        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接创建并准备数据执行命令对象。
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                SqlDataReader result = ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                SqlDataReader result = ExecuteReader(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值对连接创建并准备数据执行命令对象。
        /// </summary>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回包含结果的数据读取对象</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                SqlDataReader result = ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                SqlDataReader result = ExecuteReader(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion

        #region ExecuteScalarTypedParams

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, dataRow);

                object result = ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                object result = ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(connection, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回结果集中第一行的第一列的值</returns>
        public static object ExecuteScalarTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                object result = ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return result;
            }
            else
            {
                object result = ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
                return result;
            }
        }

        #endregion

        #region ExecuteXmlReaderTypedParams

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                XmlReader reader = ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                return reader;
            }
            else
            {
                XmlReader reader = ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
                return reader;
            }
        }

        /// <summary>
        /// 通过 DataRow 值作为存储过程输入参数值将 CommandText 发送到 Connection 并生成一个 XmlReader 对象。
        /// </summary>
        /// <param name="transaction">有效的数据事务对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">保持存储过程输入参数值的 DataRow 对象</param>
        /// <returns>执行命令后返回包含结果集的 XmlReader 对象</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("该事务已经被回滚或提交, 请提供一个正打开的事务.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(commandParameters, dataRow);

                XmlReader reader = ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                return reader;
            }
            else
            {
                XmlReader reader = ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
                return reader;
            }
        }

        #endregion

    }

    /// <summary>
    /// 提供静态缓存用于存储参数，以使存储过程可以在运行时发现参数。
    /// </summary>
    public static class SqlHelperParameterCache
    {
        #region 私有方法、变量与构造函数

        private static readonly Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 在运行时发现存储过程的参数集合。
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">是否要包括返回值参数</param>
        /// <returns>待发现的参数数组</returns>
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            int count = cmd.Parameters.Count;
            SqlParameter[] discoveredParameters = new SqlParameter[count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            foreach (SqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }

            return discoveredParameters;
        }

        /// <summary>
        /// 复制参数
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            int length = originalParameters.Length;
            SqlParameter[] clonedParameters = new SqlParameter[length];

            for (int i = 0; i < length; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion

        #region 缓存函数

        /// <summary>
        /// 增加参数数组到缓存
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <param name="commandParameters">待缓存的参数数组</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// 从缓存中得到参数数组
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="commandText">获取或设置要对数据源执行的 Transact-SQL 语句或存储过程</param>
        /// <returns>得到的参数数组</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            string hashKey = connectionString + ":" + commandText;

            SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion

        #region 参数发现函数

        /// <summary>
        /// 取得存储过程的参数数组
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns>取到的参数数组</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            SqlParameter[] param = GetSpParameterSet(connectionString, spName, false);

            return param;
        }

        /// <summary>
        /// 取得存储过程的参数数组
        /// </summary>
        /// <param name="connectionString">有效的数据库连接串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">指示返回的参数值是否要被包含的结果里</param>
        /// <returns>取到的参数数组</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlParameter[] param = GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
                return param;
            }
        }

        /// <summary>
        /// 取得存储过程的参数数组
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns>取到的参数数组</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            SqlParameter[] param = GetSpParameterSet(connection, spName, false);

            return param;
        }

        /// <summary>
        /// 取得存储过程的参数数组
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">指示返回的参数值是否要被包含的结果里</param>
        /// <returns>取到的参数数组</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                SqlParameter[] param = GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
                return param;
            }
        }

        /// <summary>
        /// 取得存储过程的参数数组
        /// </summary>
        /// <param name="connection">有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">指示返回的参数值是否要被包含的结果里</param>
        /// <returns>取到的参数数组</returns>
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion

    }
}
