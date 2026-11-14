using Npgsql;
using NpgsqlTypes;
using Vendinha.Models;

namespace Vendinha.Services;

public class ClienteService
{
    public List<Cliente> Listar(string pesquisa, int pagina)
    {
        var lista = new List<Cliente>();
        using var conexao = Banco.Abrir();
        const string sql = """
            SELECT c.id, c.nome, c.cpf, c.data_nascimento, c.email,
                   COALESCE(SUM(CASE WHEN d.paga = FALSE THEN d.valor ELSE 0 END), 0) AS total
            FROM clientes c
            LEFT JOIN dividas d ON d.cliente_id = c.id
            WHERE c.nome ILIKE @busca
            GROUP BY c.id, c.nome, c.cpf, c.data_nascimento, c.email
            ORDER BY total DESC, c.nome, c.id
            LIMIT 10 OFFSET @inicio
            """;
        using var cmd = new NpgsqlCommand(sql, conexao);
        cmd.Parameters.AddWithValue("busca", "%" + pesquisa.Trim() + "%");
        cmd.Parameters.AddWithValue("inicio", (Math.Max(1, pagina) - 1) * 10);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Cliente
            {
                Id = reader.GetInt32(0), Nome = reader.GetString(1),
                CPF = reader.GetString(2), DataNascimento = reader.GetDateTime(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                TotalEmAberto = reader.GetDecimal(5)
            });
        }
        return lista;
    }

    public int Contar(string pesquisa)
    {
        using var conexao = Banco.Abrir();
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM clientes WHERE nome ILIKE @busca", conexao);
        cmd.Parameters.AddWithValue("busca", "%" + pesquisa.Trim() + "%");
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Cliente? Buscar(int id)
    {
        using var conexao = Banco.Abrir();
        using var cmd = new NpgsqlCommand("SELECT id, nome, cpf, data_nascimento, email FROM clientes WHERE id = @id", conexao);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new Cliente
        {
            Id = reader.GetInt32(0), Nome = reader.GetString(1),
            CPF = reader.GetString(2), DataNascimento = reader.GetDateTime(3),
            Email = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    public void Salvar(Cliente cliente)
    {
        cliente.Nome = cliente.Nome.Trim();
        cliente.CPF = new string(cliente.CPF.Where(char.IsDigit).ToArray());
        cliente.Email = string.IsNullOrWhiteSpace(cliente.Email) ? null : cliente.Email.Trim();

        if (cliente.Nome.Length < 3 || cliente.Nome.Length > 100)
            throw new Exception("O nome deve ter de 3 a 100 caracteres.");
        if (cliente.CPF.Length != 11)
            throw new Exception("O CPF deve conter exatamente 11 números.");
        if (cliente.DataNascimento.Date < new DateTime(1900, 1, 1) || cliente.DataNascimento.Date > DateTime.Today)
            throw new Exception("Informe uma data de nascimento válida.");
        if (cliente.Email != null && (cliente.Email.Length > 120 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(cliente.Email)))
            throw new Exception("Informe um e-mail válido.");

        using var conexao = Banco.Abrir();
        using var verificar = new NpgsqlCommand("SELECT COUNT(*) FROM clientes WHERE cpf = @cpf AND id <> @id", conexao);
        verificar.Parameters.AddWithValue("cpf", cliente.CPF);
        verificar.Parameters.AddWithValue("id", cliente.Id);
        if (Convert.ToInt32(verificar.ExecuteScalar()) > 0)
            throw new Exception("Já existe um cliente com esse CPF.");

        string sql = cliente.Id == 0
            ? "INSERT INTO clientes (nome,cpf,data_nascimento,email) VALUES (@nome,@cpf,@nascimento,@email)"
            : "UPDATE clientes SET nome=@nome,cpf=@cpf,data_nascimento=@nascimento,email=@email WHERE id=@id";
        using var cmd = new NpgsqlCommand(sql, conexao);
        cmd.Parameters.AddWithValue("nome", cliente.Nome);
        cmd.Parameters.AddWithValue("cpf", cliente.CPF);
        cmd.Parameters.AddWithValue("nascimento", NpgsqlDbType.Timestamp, cliente.DataNascimento.Date);
        cmd.Parameters.AddWithValue("email", (object?)cliente.Email ?? DBNull.Value);
        if (cliente.Id != 0) cmd.Parameters.AddWithValue("id", cliente.Id);
        cmd.ExecuteNonQuery();
    }

    public void Excluir(int id)
    {
        using var conexao = Banco.Abrir();
        using var verificar = new NpgsqlCommand("SELECT COUNT(*) FROM dividas WHERE cliente_id=@id AND paga=FALSE", conexao);
        verificar.Parameters.AddWithValue("id", id);
        if (Convert.ToInt32(verificar.ExecuteScalar()) > 0)
            throw new Exception("Não é possível excluir cliente com dívida em aberto.");
        using var cmd = new NpgsqlCommand("DELETE FROM clientes WHERE id=@id", conexao);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }
}
