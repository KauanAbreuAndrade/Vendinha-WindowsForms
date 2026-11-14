using Vendinha.Services;

namespace Vendinha;

public class FormClientes : Form
{
    private readonly ClienteService servico = new();
    private readonly TextBox busca = new() { Width = 230, PlaceholderText = "Buscar pelo nome" };
    private readonly DataGridView tabela = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AllowUserToAddRows = false
    };
    private readonly Button anterior = new() { Text = "Anterior", AutoSize = true };
    private readonly Button proxima = new() { Text = "Próxima", AutoSize = true };
    private readonly Label paginaTexto = new() { AutoSize = true, Margin = new Padding(10, 9, 10, 0) };
    private int pagina = 1;

    public FormClientes()
    {
        Text = "Vendinha - Clientes";
        Width = 930; Height = 550;
        StartPosition = FormStartPosition.CenterScreen;

        var topo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(7) };
        topo.Controls.Add(busca);
        topo.Controls.Add(Botao("Buscar", () => { pagina = 1; Carregar(); }));
        topo.Controls.Add(Botao("Novo cliente", () => AbrirCadastro(null)));
        topo.Controls.Add(Botao("Editar", Editar));
        topo.Controls.Add(Botao("Excluir", Excluir));
        topo.Controls.Add(Botao("Dívidas", AbrirDividas));

        var rodape = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8) };
        anterior.Click += (_, _) => { if (pagina > 1) { pagina--; Carregar(); } };
        proxima.Click += (_, _) => { pagina++; Carregar(); };
        rodape.Controls.AddRange(new Control[] { anterior, paginaTexto, proxima });

        Controls.Add(tabela);
        Controls.Add(rodape);
        Controls.Add(topo);
        busca.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                pagina = 1;
                Carregar();
                e.SuppressKeyPress = true;
            }
        };
        tabela.CellDoubleClick += (_, _) => AbrirDividas();
        Shown += (_, _) => Carregar();
    }

    private static Button Botao(string texto, Action acao)
    {
        var botao = new Button { Text = texto, AutoSize = true };
        botao.Click += (_, _) => acao();
        return botao;
    }

    private int? IdSelecionado() => tabela.CurrentRow?.Cells["Id"].Value is int id ? id : null;

    private void Carregar()
    {
        try
        {
            int total = servico.Contar(busca.Text);
            int paginas = Math.Max(1, (int)Math.Ceiling(total / 10.0));
            pagina = Math.Clamp(pagina, 1, paginas);
            var lista = servico.Listar(busca.Text, pagina);
            tabela.DataSource = lista.Select(c => new
            {
                c.Id, c.Nome, c.CPF, c.Idade, c.Email,
                c.TotalEmAberto
            }).ToList();
            if (tabela.Columns.Contains("Id")) tabela.Columns["Id"].Visible = false;
            if (tabela.Columns.Contains("TotalEmAberto"))
            {
                tabela.Columns["TotalEmAberto"].HeaderText = "Total em aberto";
                tabela.Columns["TotalEmAberto"].DefaultCellStyle.Format = "C2";
            }
            paginaTexto.Text = $"Página {pagina} de {paginas} ({total} clientes)";
            anterior.Enabled = pagina > 1;
            proxima.Enabled = pagina < paginas;
        }
        catch (Exception ex) { Erro(ex); }
    }

    private void AbrirCadastro(int? id)
    {
        using var tela = new FormCadastroCliente(id);
        if (tela.ShowDialog(this) == DialogResult.OK) Carregar();
    }

    private void Editar()
    {
        int? id = IdSelecionado();
        if (id == null) { Aviso("Selecione um cliente para editar."); return; }
        AbrirCadastro(id);
    }

    private void Excluir()
    {
        int? id = IdSelecionado();
        if (id == null) { Aviso("Selecione um cliente."); return; }
        if (MessageBox.Show("Excluir este cliente e o histórico das dívidas pagas?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { servico.Excluir(id.Value); Carregar(); }
        catch (Exception ex) { Erro(ex); }
    }

    private void AbrirDividas()
    {
        int? id = IdSelecionado();
        if (id == null) { Aviso("Selecione um cliente."); return; }
        using var tela = new FormDividas(id.Value);
        tela.ShowDialog(this);
        Carregar();
    }

    internal static void Aviso(string mensagem) => MessageBox.Show(mensagem, "Vendinha");
    internal static void Erro(Exception ex) => MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
