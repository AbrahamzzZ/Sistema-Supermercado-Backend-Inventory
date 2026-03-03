using Domain.Model.Dto;
using FluentValidation;

namespace Infrastructure.Services.Validators
{
    public class ProductoStockValidator : AbstractValidator<ProductoMovimientoStock>
    {
        public ProductoStockValidator()
        {
            RuleFor(x => x.IdProducto).GreaterThan(0).WithMessage("IdProducto inválido");

            RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("Cantidad debe ser mayor a cero");

            RuleFor(x => x.Tipo).NotEmpty().Must(t => t == "ENTRADA" || t == "SALIDA").WithMessage("Tipo debe ser ENTRADA o SALIDA");

            RuleFor(x => x.Referencia).NotEmpty().WithMessage("Referencia requerida");
        }
    }
}
