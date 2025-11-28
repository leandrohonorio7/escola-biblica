using FluentValidation;

namespace EscolaBiblica.Application.Features.Igrejas.Commands;

public class CriarIgrejaCommandValidator : AbstractValidator<CriarIgrejaCommand>
{
    public CriarIgrejaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da igreja é obrigatório")
            .Length(2, 200).WithMessage("Nome deve ter entre 2 e 200 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-'\.]+$").WithMessage("Nome contém caracteres inválidos");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.Endereco)
            .MaximumLength(500).WithMessage("Endereço deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Endereco));

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Formato de telefone inválido")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class AtualizarIgrejaCommandValidator : AbstractValidator<AtualizarIgrejaCommand>
{
    public AtualizarIgrejaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID da igreja é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da igreja é obrigatório")
            .Length(2, 200).WithMessage("Nome deve ter entre 2 e 200 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-'\.]+$").WithMessage("Nome contém caracteres inválidos");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.Endereco)
            .MaximumLength(500).WithMessage("Endereço deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Endereco));

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Formato de telefone inválido")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}