using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APBD_s31722_8_API.Datalayer;

public class DbClient(IConfiguration configuration)
{
    //For lists, read list of users etc
    public async IAsyncEnumerable<T> ReadDataAsync<T>(string query, Func<SqlDataReader, T> map, Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
               new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }
            await sqlConnection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                yield return map(reader);
            }
        }
    }
    //"SQL" + return 1 scalar var of type int
    public async Task<int?> ReadScalarAsync(string query, Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
               new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }
            await sqlConnection.OpenAsync();
            return (int?)await command.ExecuteScalarAsync();
        }
    }
    //Like  ReadScalarASyn but <T>
    public async Task<T> ReadScalarAsync<T>(string query, Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
               new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }
            await sqlConnection.OpenAsync();
            return (T)await command.ExecuteScalarAsync();
        }
    }

    //Update,INSERT,DELETE
    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
               new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }
            await sqlConnection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
    }
    
    //"SQL" + return 1 valueю Return sum after update/insert + return ID
    public async Task<T> ExecuteScalarInTransactionAsync<T>(string query, Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
                     new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            await sqlConnection.OpenAsync();
            var transaction = await sqlConnection.BeginTransactionAsync();
            command.Transaction = (SqlTransaction) transaction;
            try
            {
                var result = (T)await command.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    //"SQL" + affecting more than one row like in Scalar(there can be 2 updates like from one balance take ... to another ...) 
    public async Task<int> ExecuteNonQueryInTransactionAsync<T>(string query,
        Dictionary<string, object> parameters = null)
    {
        await using (var sqlConnection =
                     new SqlConnection(configuration.GetConnectionString("Default")))
        await using (var command = new SqlCommand(query, sqlConnection))
        {
            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            var transaction = await sqlConnection.BeginTransactionAsync();
            command.Transaction = (SqlTransaction)transaction;

            try
            {
                var result = await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }
    }
    
    /// <summary>
    /// Executes a stored procedure that does not return a result set
    /// and returns the number of rows affected.
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure.</param>
    /// <param name="parameters">Optional dictionary of parameters.</param>
    /// <returns>Number of rows affected.</returns>
    public async Task<int> ExecuteProcedureAsync(string procedureName,
        Dictionary<string, object> parameters = null)
    {
        await using var sqlConnection =
            new SqlConnection(configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(procedureName, sqlConnection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters?.Count > 0)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        await sqlConnection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }
}