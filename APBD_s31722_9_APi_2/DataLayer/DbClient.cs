using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace APBD_s31722_8_API.Datalayer;

public class CommandConfig
{
    public string Query { get; set; }
    public object Parameters { get; set; }
}

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
    public async Task<T> ReadScalarAsync<T>(string query, 
        Dictionary<string, object> parameters = null, 
        CommandType commandType = CommandType.Text)
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

            command.CommandType = commandType;
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
    
    //transaction UPDATE/INSER/DELETE many in one func
    public async Task<int> ExecuteNonQueriesAsTransactionAsync(List<CommandConfig> commands)
    {
        var result = 0;
        await using var sqlConnection =
                     new SqlConnection(configuration.GetConnectionString("Default"));
        await sqlConnection.OpenAsync();
        var transaction = sqlConnection.BeginTransaction();
        try
        {
            foreach (var commandConfig in commands)
            {
                var command = new SqlCommand(commandConfig.Query, sqlConnection, transaction);
                
                if (commandConfig.Parameters != null)
                {
                    var type = commandConfig.Parameters.GetType();
                    var fields = type.GetProperties();
                    
                    // map parameters using reflection
                    foreach (var fieldInfo in commandConfig.Parameters.GetType().GetProperties())
                    {
                        command.Parameters.AddWithValue($"@{fieldInfo.Name}", fieldInfo.GetValue(commandConfig.Parameters));
                    }
                }

                result = await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
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
}