using Npgsql;
using NpgsqlTypes;
using Vendinha.Models;

namespace Vendinha.Services;

public class DividaService
{
    public List<Divida> Listar(int clienteId)
    {
        var lista = new List<Divida>();
        using var conexao = Banco.Abrir();
        using var cmd = new NpgsqlCommand("SELECT id,cliente_id,valor,paga,data_criacao,data_pagamento FROM dividas WHERE cliente_id=@id ORDER BY data_criacao DESC,id DESC", conexao);
        cmd.Parameters.AddWithValue("id", clienteId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new Divida
            {
                Id = reader.GetInt32(0), ClienteId = reader.GetInt32(1),
                Valor = reader.GetDecimal(2), Paga = reader.GetBoolean(3),
                DataCriacao = reader.GetDateTime(4),
                DataPagamento = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });
        return lista;
    }

    public void Criar(int clienteId, decimal valor)
    {
        if (valor <= 0) throw new Exception("O valor precisa ser maior que zero.");
        using var conexao = Banco.Abrir();
        using var verificar = new NpgsqlCommand("SELECT COUNT(*) FROM dividas WHERE cliente_id=@id AND paga=FALSE", conexao);
        verificar.Parameters.AddWithValue("id", clienteId);
        if (Convert.ToInt32(verificar.ExecuteScalar()) > 0)
            throw new Exception("Este cliente já possui uma dívida em aberto.");
        using var cmd = new NpgsqlCommand("INSERT INTO dividas(cliente_id,valor,paga,data_criacao) VALUES (@id,@valor,FALSE,@data)", conexao);
        cmd.Parameters.AddWithValue("id", clienteId);
        cmd.Parameters.AddWithValue("valor", valor);
        cmd.Parameters.AddWithValue("data", NpgsqlDbType.Timestamp, DateTime.Today);
        cmd.ExecuteNonQuery();
    }

    public void Pagar(int dividaId)
    {
        using var conexao = Banco.Abrir();
        using var cmd = new NpgsqlCommand("UPDATE dividas SET paga=TRUE,data_pagamento=@data WHERE id=@id AND paga=FALSE", conexao);
        cmd.Parameters.AddWithValue("id", dividaId);
        cmd.Parameters.AddWithValue("data", NpgsqlDbType.Timestamp, DateTime.Today);
        if (cmd.ExecuteNonQuery() == 0) throw new Exception("Dívida não encontrada ou já paga.");
    }
}
