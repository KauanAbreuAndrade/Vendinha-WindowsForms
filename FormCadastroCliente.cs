using Vendinha.Models;
using Vendinha.Services;

namespace Vendinha;

public class FormCadastroCliente : Form
{
    private readonly int? id;
    private readonly ClienteService servico = new();
    private readonly TextBox nome = new() { Width = 320, MaxLength = 100 };
    private readonly TextBox cpf = new() { Width = 320, MaxLength = 14 };
    private readonly TextBox email = new() { Width = 320, MaxLength = 120 };
    private readonly DateTimePicker nascimento = new()
    {
        Width = 320, Format = DateTimePickerFormat.Short,
        MinDate = new DateTime(1900, 1, 1), MaxDate = DateTime.Today
    };

    public FormCadastroCliente(int? id)
    {
        this.id = id;
        Text = id == null ? "Novo cliente" : "Editar cliente";
        Width = 410; Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1,
            Padding = new Padding(15), AutoScroll = true
        };
        foreach (var (titulo, campo) in new (string, Control)[]
        {
            ("Nome completo *", nome), ("CPF *", cpf),
            ("Data de nascimento *", nascimento), ("E-mail (opcional)", email)
        })
        {
            painel.Controls.Add(new Label { Text = titulo, AutoSize = true });
            painel.Controls.Add(campo);
        }
        var salvar = new Button { Text = "Salvar", Width = 100 };
        salvar.Click += (_, _) => Salvar();
        painel.Controls.Add(salvar);
        Controls.Add(painel);
        AcceptButton = salvar;
        if (id.HasValue) Carregar();
    }

    private void Carregar()
    {
        try
        {
            var cliente = servico.Buscar(id!.Value);
            if (cliente == null) { FormClientes.Aviso("Cliente não encontrado."); Close(); return; }
            nome.Text = cliente.Nome;
            cpf.Text = cliente.CPF;
            nascimento.Value = cliente.DataNascimento.Date;
            email.Text = cliente.Email ?? "";
        }
        catch (Exception ex) { FormClientes.Erro(ex); }
    }

    private void Salvar()
    {
        try
        {
            servico.Salvar(new Cliente
            {
                Id = id ?? 0, Nome = nome.Text, CPF = cpf.Text,
                DataNascimento = nascimento.Value.Date, Email = email.Text
            });
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { FormClientes.Erro(ex); }
    }
}
