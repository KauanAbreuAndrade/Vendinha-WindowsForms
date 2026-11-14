namespace Vendinha.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string CPF { get; set; } = "";
    public DateTime DataNascimento { get; set; }
    public string? Email { get; set; }
    public decimal TotalEmAberto { get; set; }

    public int Idade
    {
        get
        {
            int idade = DateTime.Today.Year - DataNascimento.Year;
            if (DataNascimento.Date > DateTime.Today.AddYears(-idade)) idade--;
            return idade;
        }
    }
}
