using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Zero.DataAccess
{
    /// <summary>
    /// Sql参数化
    /// </summary>
    public class SqlParamHelper
    {
        /// <summary>
        /// 生成Sql参数
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <param name="Direction"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static SqlParameter MakeParam(string ParamName, SqlDbType DbType, int Size, ParameterDirection Direction, object Value)
        {
            SqlParameter parameter;
            if (Size > 0)
            {
                parameter = new SqlParameter(ParamName, DbType, Size);
            }
            else
            {
                parameter = new SqlParameter(ParamName, DbType);
            }
            parameter.Direction = Direction;
            if ((Direction != ParameterDirection.Output) || (Value != null))
            {
                parameter.Value = Value;
            }
            return parameter;
        }
        /// <summary>
        /// 生成Sql输出参数,如果是变长度类型的建议传入数据库定义长度以便共享计划缓存
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static SqlParameter MakeOutParam(string ParamName, SqlDbType DbType, int Size = 0)
        {
            if (Size == 0) Size = GetColumnSize(DbType);
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        /// <summary>
        /// 生成Sql输入参数,如果是变长度类型的建议传入数据库定义长度以便共享计划缓存
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }
        /// <summary>
        /// 生成Sql输入参数,如果是变长度类型的建议传入数据库定义长度以便共享计划缓存
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static SqlParameter MakeInParam(string ParamName, SqlDbType DbType, object Value)
        {
            return MakeParam(ParamName, DbType, GetColumnSize(DbType), ParameterDirection.Input, Value);
        }
        /// <summary>
        /// 生成Sql输入参数,如果是变长度类型的建议传入数据库定义长度以便共享计划缓存,不建议使用此方法
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static SqlParameter MakeInParam(string ParamName, object Value)
        {
            return new SqlParameter(ParamName, Value);
        }

        /// <summary>
        /// 返回相应的数据类型长度
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static int GetColumnSize(SqlDbType dataType)
        {
            switch (dataType)
            {
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                    return 1;
                case SqlDbType.SmallInt:
                    return 2;
                case SqlDbType.Date:
                    return 3;
                case SqlDbType.Int:
                case SqlDbType.Real:
                case SqlDbType.SmallMoney:
                case SqlDbType.SmallDateTime:
                    return 4;
                case SqlDbType.Time:
                    return 5;
                case SqlDbType.BigInt:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.Money:
                case SqlDbType.Float:
                case SqlDbType.Timestamp:
                    return 8;
                case SqlDbType.Decimal:
                    return 9;
                case SqlDbType.DateTimeOffset:
                    return 10;
                case SqlDbType.UniqueIdentifier:
                    return 16;
                case SqlDbType.NText:
                case SqlDbType.Xml:
                case SqlDbType.Image:
                case SqlDbType.Text:
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                    return int.MaxValue;
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    return 8000;
                case SqlDbType.Variant:
                    return 8016;
                default:
                    return 0;
            }
        }


    }
}
