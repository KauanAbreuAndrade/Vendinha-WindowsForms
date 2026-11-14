using Npgsql;

namespace Vendinha;

public static class Banco
{
    public static NpgsqlConnection Abrir()
    {
        string conexao = Environment.GetEnvironmentVariable("VENDINHA_CONNECTION")
            ?? "Host=172.16.103.22;Port=5432;Database=techcore;Username=postgres;Password=postgres";

        var banco = new NpgsqlConnection(conexao);
        banco.Open();
        return banco;
    }
}


