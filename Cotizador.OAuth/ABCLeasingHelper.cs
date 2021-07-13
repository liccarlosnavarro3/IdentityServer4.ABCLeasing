using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ABCLeasing.OAuth
{
    public class CustomUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
    }

    public class ABCLeasingHelper
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
        #region Private members
        //PROD
        //private const string _connectionString = "Data Source=ABCFoxtrot\\SQL2017Express;Initial Catalog=ids.abcleasing.com.mx;User ID=idsown;Password=l60g_1A4oe0c_I53;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //DEV
        //private const string _connectionString = "Data Source=SFACTELEC\\sqlexpress;Initial Catalog=auth_api.abcleasing.com.mx;User ID=user_fact;Password=Sf@cT_20_3l3c_20;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //private const string _connectionString = ConexionString;

        private SqlConnection _connection = null;
        private SqlTransaction _transaction = null;
        #endregion

        #region Properties
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }
        public SqlConnection Connection
        {
            get
            {
                return _connection;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Cretes connection with MSSQL Server.
        /// </summary>
        /// <returns>Conection state</returns>
        public bool Connect()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
                else if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _connection = new SqlConnection(_connectionString);
                }
                _connection.Open();

                return (_connection.State == ConnectionState.Open);
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Closes connection with MSSQL Server.
        /// </summary>
        /// <returns>Connection state</returns>
        public bool Close()
        {
            try
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                return (_connection.State == ConnectionState.Open);
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Executes querý without parameters. Does not read.
        /// </summary>
        /// <param name="query">Query string</param>
        /// <returns>Number of afected rows</returns>
        public int ExecuteQuery(string query)
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    if (_transaction != null)
                    {
                        command.Transaction = _transaction;
                    }
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Executes query with one parameter. Does not read.
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameter">SQL Parameter</param>
        /// <returns>Number of afected rows</returns>
        public int ExecuteQuery(string query, SqlParameter parameter)
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameter.Equals(null))
                throw new ArgumentException("SQL Parameter is empty");

            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    if (_transaction != null)
                    {
                        command.Transaction = _transaction;
                    }
                    command.Parameters.Add(parameter);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Executes query with multiple parameters. Does not read.
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Array of SQL Parameters</param>
        /// <returns>Number of afected rows</returns>
        public int ExecuteQuery(string query, SqlParameter[] parameters)
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameters.Equals(null))
                throw new ArgumentException("SQL Parameters is empty");

            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    if (_transaction != null)
                    {
                        command.Transaction = _transaction;
                    }
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of T elements from a query without parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <returns>List of T elements</returns>
        public List<T> GetList<T>(string query) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            List<T> list = new List<T>();
            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            element = (T)reader.GetValue(0);
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retrieves a list of T elements from a query without one parameter.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameter">SQL Parameter</param>
        /// <returns>List of T elements</returns>
        public List<T> GetList<T>(string query, SqlParameter parameter) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameter.Equals(null))
                throw new ArgumentException("SQL Parameter is empty");

            List<T> list = new List<T>();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.Add(parameter);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            element = (T)reader.GetValue(0);
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retrieves a list of T elements from a query without multiple parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Array of SQL Parameters</param>
        /// <returns>List of T elements</returns>
        public List<T> GetList<T>(string query, SqlParameter[] parameters) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameters.Equals(null))
                throw new ArgumentException("SQL Parameters is empty");

            List<T> list = new List<T>();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddRange(parameters);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            element = (T)reader.GetValue(0);
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retriebes a list of Classes from a query with no parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <returns>List of Classes</returns>
        public List<T> GetClassList<T>(string query) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            List<T> list = new List<T>();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = element.GetType();
                            string name = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(name);

                            if (prop != null)
                            {
                                if (name == prop.Name)
                                {
                                    var value = reader.GetValue(i);
                                    if (value != DBNull.Value)
                                    {
                                        //prop.SetValue(element, reader.GetValue(i), null);
                                        prop.SetValue(element, Convert.ChangeType(value, prop.PropertyType), null);
                                    }
                                }
                            }
                            
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retriebes a list of Classes from a query with one parameter.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameter">SQL Parameter</param>
        /// <returns>List of Classes</returns>
        public List<T> GetClassList<T>(string query, SqlParameter parameter) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameter.Equals(null))
                throw new ArgumentException("SQL Parameters is empty");

            List<T> list = new List<T>();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.Add(parameter);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = element.GetType();
                            PropertyInfo prop = type.GetProperty(reader.GetName(i));
                            prop.SetValue(element, reader.GetValue(i), null);
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retriebes a list of Classes from a query with multiple parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Array of SQL Parameter</param>
        /// <returns>List of Classes</returns>
        public List<T> GetClassList<T>(string query, SqlParameter[] parameters) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameters.Equals(null))
                throw new ArgumentException("SQL Parameters is empty");

            List<T> list = new List<T>();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddRange(parameters);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T element = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = element.GetType();
                            PropertyInfo prop = type.GetProperty(reader.GetName(i));
                            prop.SetValue(element, reader.GetValue(i), null);
                        }
                        list.Add(element);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retrieves a single element of type T from a query with no parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <returns>T element</returns>
        public T GetSingleElement<T>(string query) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            T element = new T();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            element = (T)reader.GetValue(0);
                    }

                }
                return element;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retrieves a single element of type T from a query with one parameter.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameter">SQL Parameter</param>
        /// <returns>T element</returns>
        public T GetSingleElement<T>(string query, SqlParameter parameter) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameter.Equals(null))
                throw new ArgumentException("SQL Parameter is empty");

            T element = new T();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.Add(parameter);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            element = (T)reader.GetValue(0);
                    }

                }
                return element;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Retrieves a single element of type T from a query with multiple parameters.
        /// </summary>
        /// <typeparam name="T">Type of element to retrieve</typeparam>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Array of SQL Parameters</param>
        /// <returns>T element</returns>
        public T GetSingleElement<T>(string query, SqlParameter[] parameters) where T : new()
        {
            if (query.Equals(string.Empty))
                throw new ArgumentException("Query string is empty");

            if (parameters.Equals(null))
                throw new ArgumentException("SQL Parameters is empty");

            T element = new T();

            try
            {
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddRange(parameters);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            element = (T)reader.GetValue(0);
                    }

                }
                return element;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }


        }

        /// <summary>
        /// Creates Transaccíon
        /// </summary>
        /// <returns>If transaccion started succesfully</returns>
        public bool CreateTransaction()
        {
            try
            {
                if (_transaction == null)
                {
                    _transaction = _connection.BeginTransaction();
                }
                return true;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Commits and delete Transaccíon
        /// </summary>
        /// <returns>If transaccion commited succesfully</returns>
        public bool CommitTransaction()
        {
            try
            {
                _transaction.Commit();
                _transaction = null;
                return true;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        /// <summary>
        /// Rollbacks and delete Transaccíon
        /// </summary>
        /// <returns>If transaccion rollbacked succesfully</returns>
        public bool RollBackTransaction()
        {
            try
            {
                _transaction.Rollback();
                _transaction = null;
                return true;
            }
            catch (Exception ex)
            {
                // Qué ha sucedido
                var mensaje = "Error message: " + ex.Message;

                // Información sobre la excepción interna
                if (ex.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + ex.InnerException.Message;
                }

                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + ex.StackTrace;
                throw;
            }
        }

        #endregion
    }
}