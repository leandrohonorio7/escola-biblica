using FluentValidation;

namespace EscolaBiblica.Application.Features.Alunos.Commands;

public class CriarAlunoCommandValidator : AbstractValidator<CriarAlunoCommand>
{
    public CriarAlunoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do aluno é obrigatório")
            .Length(2, 200).WithMessage("Nome deve ter entre 2 e 200 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-'\.]+$").WithMessage("Nome contém caracteres inválidos");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThan(DateTime.Today).WithMessage("Data de nascimento deve ser anterior a hoje")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Data de nascimento muito antiga");

        RuleFor(x => x.NomeResponsavel)
            .MaximumLength(200).WithMessage("Nome do responsável deve ter no máximo 200 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-'\.]+$").WithMessage("Nome do responsável contém caracteres inválidos")
            .When(x => !string.IsNullOrEmpty(x.NomeResponsavel));

        RuleFor(x => x.TelefoneResponsavel)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Formato de telefone inválido")
            .When(x => !string.IsNullOrEmpty(x.TelefoneResponsavel));

        RuleFor(x => x.EmailResponsavel)
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.EmailResponsavel));

        RuleFor(x => x.Endereco)
            .MaximumLength(500).WithMessage("Endereço deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Endereco));

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("Observações devem ter no máximo 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observacoes));

        RuleFor(x => x.TurmaId)
            .NotEmpty().WithMessage("ID da turma é obrigatório")
            .When(x => x.TurmaId.HasValue);
    }
}

public class MatricularAlunoCommandValidator : AbstractValidator<MatricularAlunoCommand>
{
    public MatricularAlunoCommandValidator()
    {
        RuleFor(x => x.AlunoId)
            .NotEmpty().WithMessage("ID do aluno é obrigatório");

        RuleFor(x => x.TurmaId)
            .NotEmpty().WithMessage("ID da turma é obrigatório");
    }
}