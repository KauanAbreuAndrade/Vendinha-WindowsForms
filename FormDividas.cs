using Vendinha.Services;

namespace Vendinha;

public class FormDividas : Form
{
    private readonly int clienteId;
    private readonly DividaService servico = new();
    private readonly DataGridView tabela = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AllowUserToAddRows = false
    };
    private readonly Label titulo = new() { AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
    private readonly NumericUpDown valor = new()
    {
        Width = 140, DecimalPlaces = 2, Minimum = 0.01m,
        Maximum = 999999999m, ThousandsSeparator = true
    };

    public FormDividas(int clienteId)
    {
        this.clienteId = clienteId;
        Text = "Dívidas do cliente";
        Width = 770; Height = 430;
        StartPosition = FormStartPosition.CenterParent;
        var topo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
        topo.Controls.Add(titulo);
        var rodape = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(8) };
        rodape.Controls.Add(new Label { Text = "Valor (R$):", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        rodape.Controls.Add(valor);
        var criar = new Button { Text = "Cadastrar dívida", AutoSize = true };
        criar.Click += (_, _) => Criar();
        rodape.Controls.Add(criar);
        var pagar = new Button { Text = "Marcar como paga", AutoSize = true };
        pagar.Click += (_, _) => Pagar();
        rodape.Controls.Add(pagar);
        Controls.Add(tabela);
        Controls.Add(rodape);
        Controls.Add(topo);
        Shown += (_, _) => Carregar();
    }

    private void Carregar()
    {
        try
        {
            var cliente = new ClienteService().Buscar(clienteId);
            if (cliente == null) { FormClientes.Aviso("Cliente não encontrado."); Close(); return; }
            var lista = servico.Listar(clienteId);
            titulo.Text = $"{cliente.Nome} | Em aberto: {lista.Where(d => !d.Paga).Sum(d => d.Valor):C2}";
            tabela.DataSource = lista.Select(d => new
            {
                d.Id, d.Valor, Situacao = d.Paga ? "Paga" : "Em aberto",
                CriadaEm = d.DataCriacao.ToShortDateString(),
                PagaEm = d.DataPagamento?.ToShortDateString() ?? "-"
            }).ToList();
            if (tabela.Columns.Contains("Id")) tabela.Columns["Id"].Visible = false;
            if (tabela.Columns.Contains("Valor")) tabela.Columns["Valor"].DefaultCellStyle.Format = "C2";
        }
        catch (Exception ex) { FormClientes.Erro(ex); }
    }

    private void Criar()
    {
        try
        {
            servico.Criar(clienteId, valor.Value);
            valor.Value = valor.Minimum;
            Carregar();
        }
        catch (Exception ex) { FormClientes.Erro(ex); }
    }

    private void Pagar()
    {
        if (tabela.CurrentRow?.Cells["Id"].Value is not int id)
        {
            FormClientes.Aviso("Selecione uma dívida."); return;
        }
        if (MessageBox.Show("Confirmar pagamento?", "Vendinha",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { servico.Pagar(id); Carregar(); }
        catch (Exception ex) { FormClientes.Erro(ex); }
    }
}
