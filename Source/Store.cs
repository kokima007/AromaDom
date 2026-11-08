using Microsoft.Data.Sqlite;
namespace AromaDom;
public class Store(IWebHostEnvironment environment)
{
    private string DirectoryPath => Path.Combine(environment.ContentRootPath, "Database");
    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(DirectoryPath, "aroma.db"), ForeignKeys = true }.ToString());
        connection.Open();
        return connection;
    }
    public void Initialize()
    {
        Directory.CreateDirectory(DirectoryPath);
        using var connection = Open();
        foreach (var file in new[] { "01_schema.sql", "02_import.sql" })
        {
            using var command = connection.CreateCommand();
            command.CommandText = File.ReadAllText(Path.Combine(DirectoryPath, file));
            command.ExecuteNonQuery();
        }
    }
    public List<Dictionary<string, object>> Query(string sql, params (string, object)[] parameters)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        using var reader = command.ExecuteReader();
        var rows = new List<Dictionary<string, object>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }
}
