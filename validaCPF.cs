using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace validaCPF;

public class validaCPF
{
    private readonly ILogger<validaCPF> _logger;

    public validaCPF(ILogger<validaCPF> logger)
    {
        _logger = logger;
    }

    [Function("validaCPF")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Iniciando validação de CPF");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        
        // Fix: Use typed class instead of dynamic to avoid JsonElement issues
        var data = JsonSerializer.Deserialize<CpfRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (string.IsNullOrEmpty(data?.Cpf)) {
            return new BadRequestObjectResult("CPF não informado");
        }
        string cpf = data.Cpf!;
        if (cpf.Length != 11) {
            return new BadRequestObjectResult("CPF inválido");
        }

        bool resultado = IsCpfValid(cpf);
        var responseMessage = resultado ? "CPF válido na Receita Federal" : "CPF inválido na Receita Federal";
        return new OkObjectResult(responseMessage);
    }

    public class CpfRequest
    {
        public string? Cpf { get; set; }
    }

    public bool IsCpfValid(string cpf) {
        // Remove non-numeric characters just in case, though checking length 11 beforehand handles most
        cpf = cpf.Replace(".", "").Replace("-", "");

        if (cpf.Length != 11)
            return false;

        // Check for known invalid CPFs (all digits equal)
        if (cpf.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();

        return cpf.EndsWith(digito);
    }
}