using System.Diagnostics.CodeAnalysis;

namespace EscolaBiblica.Tests.Helpers
{
    [ExcludeFromCodeCoverage] // Utilitários de teste não precisam de cobertura
    public static class TestDataGenerator
    {
        public static IEnumerable<object[]> GetValidCompeticaoData()
        {
            yield return new object[]
            {
                "Competição Individual",
                "Descrição para competição individual",
                EscolaBiblica.BlazorWASM.Models.TipoCompeticao.Individual,
                40m, 30m, 30m
            };

            yield return new object[]
            {
                "Competição em Equipe",
                "Descrição para competição em equipe",
                EscolaBiblica.BlazorWASM.Models.TipoCompeticao.Equipe,
                50m, 25m, 25m
            };

            yield return new object[]
            {
                "Competição por Turma",
                "Descrição para competição por turma",
                EscolaBiblica.BlazorWASM.Models.TipoCompeticao.PorTurma,
                60m, 20m, 20m
            };
        }

        public static IEnumerable<object[]> GetInvalidCompeticaoData()
        {
            yield return new object[] { "", "Descrição válida" }; // Nome vazio
            yield return new object[] { "Nome válido", "" }; // Descrição vazia
            yield return new object[] { new string('A', 101), "Descrição válida" }; // Nome muito longo
        }

        public static IEnumerable<object[]> GetValidNotasData()
        {
            yield return new object[] { 10.0m, 10.0m, 10.0m };
            yield return new object[] { 8.5m, 9.0m, 7.5m };
            yield return new object[] { 0.0m, 5.0m, 2.5m };
            yield return new object[] { 6.75m, 8.25m, 9.50m };
        }

        public static IEnumerable<object[]> GetValidPesoDistributions()
        {
            yield return new object[] { 40m, 30m, 30m }; // Distribuição padrão
            yield return new object[] { 50m, 25m, 25m }; // Foco na prova
            yield return new object[] { 33m, 33m, 34m }; // Distribuição equilibrada
            yield return new object[] { 100m, 0m, 0m }; // Apenas prova
            yield return new object[] { 0m, 50m, 50m }; // Sem prova
        }
    }

    [ExcludeFromCodeCoverage] // Utilitários de teste não precisam de cobertura
    public static class TestLogger
    {
        public static void LogTestStart(string testName)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIANDO: {testName}");
        }

        public static void LogTestEnd(string testName, bool success)
        {
            var status = success ? "SUCESSO" : "FALHA";
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {status}: {testName}");
        }

        public static void LogTestRetry(string testName, int attempt, int maxAttempts)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RETRY: {testName} - Tentativa {attempt}/{maxAttempts}");
        }
    }
}